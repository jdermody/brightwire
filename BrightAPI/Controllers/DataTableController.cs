using System.Text;
using System.Text.Json;
using BrightAPI.Database;
using BrightAPI.Helper;
using BrightAPI.Models;
using BrightAPI.Models.DataTable;
using BrightAPI.Models.DataTable.Requests;
using BrightData;
using BrightData.DataTable;
using BrightData.LinearAlgebra.ReadOnly;
using BrightData.Types;
using Microsoft.AspNetCore.Mvc;

namespace BrightAPI.Controllers
{
    [ApiController, Route("[controller]")]
    public class DataTableController : BaseController
    {
        readonly ILogger<DataTableController> _logger;
        readonly DatabaseManager              _databaseManager;
        readonly BrightDataContext            _context;
        readonly TempFileManager              _tempFileManager;

        public DataTableController(
            ILogger<DataTableController> logger,
            DatabaseManager databaseManager,
            BrightDataContext context,
            TempFileManager tempFileManager
        )
        {
            _logger = logger;
            _databaseManager = databaseManager;
            _context = context;
            _tempFileManager = tempFileManager;
        }

        [HttpPost, Route("csv/preview")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DataTablePreviewModel>> GetPreviewFromCsv([FromBody] DataTableCsvPreviewRequest request)
        {
            var filePreview = string.Join('\n', request.Lines);
            if (string.IsNullOrWhiteSpace(filePreview))
                return BadRequest();

            try {
                using var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(filePreview)));
                using var table = await _context.ParseCsv(reader, request.HasHeader, request.Delimiter);
                var columns = new DataTableColumnModel[table.ColumnCount];
                for (uint i = 0; i < table.ColumnCount; i++) {
                    var metaData = table.ColumnMetaData[i];
                    var name = metaData.GetName();
                    if (String.IsNullOrWhiteSpace(name))
                        name = GetDefaultColumnName(i);
                    columns[i] = new DataTableColumnModel {
                        ColumnType = table.ColumnTypes[i],
                        Name = name
                    };
                }
                return new DataTablePreviewModel {
                    Columns = columns,
                    PreviewRows = table.GetAllRows().Result.Select(r => r.Values.Select(x => x.ToString()!).ToArray()).ToArray()
                };
            }
            catch (Exception ex) {
                _logger.LogError(ex, null);
            }
            return BadRequest();
        }

        [HttpPost, Route("csv")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DataTableListItemModel>> CreateNewTableFromCsv([FromBody] DataTableCsvRequest request)
        {
            var fileData = string.Join('\n', request.Lines);
            if (string.IsNullOrWhiteSpace(fileData))
                return BadRequest();

            var columnNames = request.ColumnNames?.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            try {
                var dataTable = await CreateDataTableFromCsv(_databaseManager, _context, _tempFileManager, request.HasHeader, request.Delimiter, request.TargetIndex, columnNames, request.FileName, fileData);
                return AsListItem(dataTable);
            }
            catch (Exception ex) {
                _logger.LogError(ex, null);
            }
            return BadRequest();
        }
        public static async Task<DataTable> CreateDataTableFromCsv(
            DatabaseManager dataBase, 
            BrightDataContext context, 
            TempFileManager tempFileManager,
            bool hasHeader, 
            char delimiter, 
            uint? targetIndex, 
            string[]? columnNames, 
            string? fileName, 
            string fileData)
        {
            using var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(fileData)));
            using var table = await context.ParseCsv(reader, hasHeader, delimiter);
            if (targetIndex.HasValue)
                table.SetTargetColumn(targetIndex.Value);

            if (columnNames is not null) {
                uint columnIndex = 0;
                foreach (var columnName in columnNames) {
                    var column = table.ColumnMetaData[columnIndex++];
                    column.SetName(columnName);
                }
            }

            // analyse table data
            await table.GetColumnAnalysis();
            if (fileName is not null)
                table.MetaData.Set("file-name", fileName);
            table.MetaData.Set("date-created", DateTime.UtcNow);
            table.PersistMetaData();

            // save table to disk and the DB
            var (path, id) = tempFileManager.GetNewTempPath();
            table.WriteTo(path);
            return await dataBase.CreateDataTable(fileName ?? "", id, path, table.RowCount);
        }

        [HttpGet, Route("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IEnumerable<DataTableListItemModel>> GetDataTables()
        {
            var ret = await _databaseManager.GetAllDataTables();
            return ret.Select(AsListItem);
        }

        [HttpGet, Route("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DataTableInfoModel>> GetDataTable(string id)
        {
            var dataTableResult = await LoadDataTable(id);
            if (dataTableResult.Result is not null)
                return dataTableResult.Result;
            var dataTableInfo = dataTableResult.Value!;

            using var table = _context.LoadTable(dataTableInfo.LocalPath);
            var ret = new DataTableInfoModel {
                Id = id,
                Name = dataTableInfo.Name,
                Columns = table.ColumnCount.AsRange().Select(i => {
                    var metaData = table.ColumnMetaData[i];
                    return new DataTableColumnModel {
                        Name = metaData.GetName(),
                        ColumnType = table.ColumnTypes[i],
                        IsTarget = metaData.IsTarget(),
                        Metadata = AsModel(metaData)
                    };
                }).ToArray(),
                Metadata = AsModel(table.MetaData),
                RowCount = table.RowCount
            };
            return ret;
        }

        [HttpGet, Route("{id}/{start}/{count}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<string[][]>> GetDataTableData(string id, uint start, uint count)
        {
            var dataTableResult = await LoadDataTable(id);
            if (dataTableResult.Result is not null)
                return dataTableResult.Result;
            var dataTableInfo = dataTableResult.Value!;

            using var table = _context.LoadTable(dataTableInfo.LocalPath);
            var ret = table.GetSlice(start, count).Result.Select(r => r.Values.Select(x => x.ToString() ?? "-").ToArray()).ToArray();
            return ret;
        }

        [HttpPost, Route("{id}/convert")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<NamedItemModel>> ConvertTable(string id, [FromBody] ConvertDataTableColumnsRequest request)
        {
            int len;
            if ((len = request.ColumnIndices.Length) != request.ColumnConversions.Length || len == 0)
                return BadRequest();

            return await Transform(id, request, "Converted", async (table, path) => {
                var columnConversions = new ColumnConversion[table.ColumnCount];
                for (uint i = 0; i < len; i++) {
                    if (i > table.ColumnCount)
                        throw new BadHttpRequestException($"Column index exceeded column count: {i}");
                    columnConversions[request.ColumnIndices[i]] = request.ColumnConversions[i];
                }

                // check if there is anything to convert
                if(columnConversions.All(x => x == ColumnConversion.Unchanged))
                    return table;
                return await table.Convert(path, columnConversions.Where(x => x != ColumnConversion.Unchanged).Select((c, i) => c.ConvertColumn((uint)i)).ToArray());
            });
        }

        [HttpPost, Route("{id}/normalize")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<NamedItemModel>> NormalizeTable(string id, [FromBody] NormalizeDataTableColumnsRequest request)
        {
            int len;
            if ((len = request.ColumnIndices.Length) != request.Columns.Length || len == 0)
                return BadRequest();

            return await Transform(id, request, "Normalized", async (table, path) => {
                var columnConversions = new NormalizationType[table.ColumnCount];
                for (uint i = 0; i < len; i++) {
                    if (i > table.ColumnCount)
                        throw new BadHttpRequestException($"Column index exceeded column count: {i}");
                    columnConversions[request.ColumnIndices[i]] = request.Columns[i];
                }
                return await table.Normalize(path, columnConversions);
            });
        }

        [HttpPost, Route("{id}/reinterpret")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<NamedItemModel>> ReinterpretTableColumns(string id, [FromBody] ReinterpretDataTableColumnsRequest request)
        {
            if (request.Columns.Length == 0)
                return BadRequest();

            return await Transform(id, request, "Reinterpreted", async (table, path) => {
                using var tempStreams = _context.CreateTempDataBlockProvider();
                var inputColumnIndices = new HashSet<uint>();
                var newColumns = new Dictionary<uint, List<ICompositeBuffer>>();
                foreach (var column in request.Columns) {
                    if(column.ColumnIndices.Any(x => x > table.ColumnCount))
                        throw new BadHttpRequestException("Column index exceeded column count");
                    var inputColumns = column.ColumnIndices.Select(table.GetColumn).Cast<IReadOnlyBuffer>().ToArray();
                    var buffer = await inputColumns.Vectorise(tempStreams);
                    buffer.MetaData.SetName(column.Name);
                    foreach (var columnIndex in column.ColumnIndices)
                        inputColumnIndices.Add(columnIndex);
                    if(!newColumns.TryGetValue(column.ColumnIndices[0], out var list))
                        newColumns.Add(column.ColumnIndices[0], list = new());
                    list.Add(buffer);
                }

                var finalColumns = new List<IReadOnlyBufferWithMetaData>();
                for (uint i = 0; i < table.ColumnCount; i++) {
                    if(!inputColumnIndices.Contains(i))
                        finalColumns.Add(table.GetColumn(i));
                    else if(newColumns.TryGetValue(i, out var list))
                        finalColumns.AddRange(list);
                }

                await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
                var newTable = await table.Context.BuildDataTable(table.MetaData, finalColumns.ToArray(), stream);
                await newTable.GetColumnAnalysis();
                newTable.PersistMetaData();
                return newTable;
            });
        }

        [HttpPost, Route("{id}/vectorise")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<NamedItemModel>> VectoriseTable(string id, [FromBody] VectoriseDataTableColumnsRequest request)
        {
            if (request.ColumnIndices.Length == 0)
                return BadRequest();

            return await Transform(id, request, "Vectorised", async (table, path) => {
                var newTable = await table.Vectorise(request.OneHotEncodeToMultipleColumns, request.ColumnIndices, path);
                await newTable.GetColumnAnalysis();
                newTable.PersistMetaData();
                return newTable;
            });
        }

        [HttpPost, Route("{id}/copy-columns")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<NamedItemModel>> CopyTableColumns(string id, [FromBody] DataTableColumnsRequest request)
        {
            if (request.ColumnIndices.Length == 0)
                return BadRequest();

            return await Transform(id, request, "Column Subset", (table, path) => table.CopyColumnsToNewTable(path, request.ColumnIndices));
        }

        [HttpPost, Route("{id}/copy-rows")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<NamedItemModel>> CopyTableRows(string id, [FromBody] DataTableRowsRequest request)
        {
            if (request.RowRanges.Length == 0)
                return BadRequest();

            var rows = new List<uint>();
            foreach (var range in request.RowRanges) {
                if (range.FirstInclusiveRowIndex >= range.LastExclusiveRowIndex)
                    return BadRequest();
                for (var i = range.FirstInclusiveRowIndex; i < range.LastExclusiveRowIndex; i++)
                    rows.Add(i);
            }
            if (!rows.Any())
                return BadRequest();

            return await Transform(id, request, "Row Subset", async (table, path) => {
                var newTable = await table.CopyRowsToNewTable(path, rows.ToArray());
                await newTable.GetColumnAnalysis();
                newTable.PersistMetaData();
                return newTable;
            });
        }

        [HttpPost, Route("{id}/shuffle")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<NamedItemModel>> ShuffleTableRows(string id, [FromBody] EmptyTableRequest request)
        {
            return await Transform(id, request, "Shuffled", (table, path) => table.Shuffle(path));
        }

        [HttpPost, Route("{id}/bag")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<NamedItemModel>> BagTableRows(string id, [FromBody] BagTableRequest request)
        {
            if (request.RowCount == 0)
                return BadRequest();

            return await Transform(id, request, "Bagged", async (table, path) => {
                var newTable = await table.Bag(path, request.RowCount);
                await newTable.GetColumnAnalysis();
                newTable.PersistMetaData();
                return newTable;
            });
        }

        [HttpPost, Route("{id}/split")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<NamedItemModel[]>> SplitTableRows(string id, [FromBody] SplitTableRequest request)
        {
            if (request.TrainingPercentage is < 0 or > 100)
                return BadRequest();

            var dataTableResult = await LoadDataTable(id);
            if (dataTableResult.Result is not null)
                return dataTableResult.Result;
            var dataTableInfo = dataTableResult.Value!;

            using var table = _context.LoadTable(dataTableInfo.LocalPath);

            var (path1, newTableId1) = _tempFileManager.GetNewTempPath();
            var (path2, newTableId2) = _tempFileManager.GetNewTempPath();

            var (trainingTable, testTable) = await table.Split(request.TrainingPercentage / 100, path1, path2);
            try {
                await Task.WhenAll(trainingTable.GetColumnAnalysis(), testTable.GetColumnAnalysis());
                trainingTable.PersistMetaData();
                testTable.PersistMetaData();

                var training = await CreateTable(id, dataTableInfo, request, "Training", newTableId1, path1, trainingTable);
                var test = await CreateTable(id, dataTableInfo, request, "Test", newTableId2, path2, testTable);

                return new[] {
                    training,
                    test
                };
            }
            finally {
                trainingTable.Dispose();
                testTable.Dispose();
            }
        }

        async Task<ActionResult<NamedItemModel>> Transform<T>(string id, T request, string newTableSuffix, Func<IDataTable /* input */, string /* path */, Task<IDataTable> /* output */> callback)
        {
            var dataTableResult = await LoadDataTable(id);
            if (dataTableResult.Result is not null)
                return dataTableResult.Result;
            var dataTableInfo = dataTableResult.Value!;

            using var table = _context.LoadTable(dataTableInfo.LocalPath);

            // create a new converted table
            var (path, newTableId) = _tempFileManager.GetNewTempPath();
            using var newTable = await callback(table, path);

            // maybe there was nothing to transform
            if (newTable == table) {
                return new NamedItemModel {
                    Id = dataTableInfo.PublicId,
                    Name = dataTableInfo.Name
                };
            }

            return await CreateTable(id, dataTableInfo, request, newTableSuffix, newTableId, path, newTable);
        }

        async Task<NamedItemModel> CreateTable<T>(string sourceId, DataTable sourceDataTableInfo, T request, string newTableSuffix, Guid newTableId, string path, IDataTable newTable)
        {
            // set table metadata
            var requestJson = JsonSerializer.Serialize(request);
            await newTable.GetColumnAnalysis();
            newTable.MetaData.Set("based-on-table-id", sourceId);
            newTable.MetaData.Set("transformation-request", requestJson);
            newTable.MetaData.Set("date-created", DateTime.UtcNow);
            newTable.PersistMetaData();

            var newTableInfo = await _databaseManager.CreateDataTable(sourceDataTableInfo.Name + $" [{newTableSuffix}]", newTableId, path, newTable.RowCount);
            return new NamedItemModel {
                Id = newTableInfo.PublicId,
                Name = newTableInfo.Name
            };
        }


        [HttpDelete, Route("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteTable(string id)
        {
            var dataTableResult = await LoadDataTable(id);
            if (dataTableResult.Result is not null)
                return dataTableResult.Result;
            var dataTable = dataTableResult.Value!;

            await _databaseManager.DeleteDataTable(dataTable);

            // delete the file on disk
            var fileInfo = new FileInfo(dataTable.LocalPath);
            if(fileInfo.Exists)
                fileInfo.Delete();

            return Ok();
        }

        [HttpPost, Route("{id}/rename-columns")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DataTableInfoModel>> RenameTableColumns(string id, [FromBody] RenameTableColumnsRequest request)
        {
            if (request.ColumnIndices.Length == 0 || request.ColumnsNames.Length != request.ColumnIndices.Length)
                return BadRequest();
            var nameTable = request.ColumnIndices.Zip(request.ColumnsNames).ToDictionary(x => x.First, x => x.Second);

            var dataTableResult = await LoadDataTable(id);
            if (dataTableResult.Result is not null)
                return dataTableResult.Result;
            var dataTable = dataTableResult.Value!;

            {
                using var table = _context.LoadTable(dataTable.LocalPath);
                foreach (var item in nameTable)
                    table.ColumnMetaData[item.Key].SetName(item.Value);
                table.PersistMetaData();
            }

            return await GetDataTable(id);
        }

        [HttpPost, Route("{id}/set-target")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DataTableInfoModel>> SetTableColumnTarget(string id, [FromBody] SetColumnTargetRequest request)
        {
            var dataTableResult = await LoadDataTable(id);
            if (dataTableResult.Result is not null)
                return dataTableResult.Result;
            var dataTable = dataTableResult.Value!;

            {
                using var table = _context.LoadTable(dataTable.LocalPath);
                uint index = 0;
                foreach (var column in table.ColumnMetaData)
                    column.SetTarget(index++ == request.TargetColumn);
                table.PersistMetaData();
            }

            return await GetDataTable(id);
        }

        async Task<ActionResult<DataTable>> LoadDataTable(string id)
        {
            var dataTable = await _databaseManager.GetDataTable(id);

            if (dataTable is null)
                return NotFound($"Data table not found in database: {id}");

            if (!System.IO.File.Exists(dataTable.LocalPath))
                return Problem($"Data table not found on disk: {id}");

            return dataTable;
        }

        static NameValueModel[] AsModel(MetaData metadata) => metadata.GetNonEmpty().Select(x => new NameValueModel {
            Value = x.StringValue,
            Name = x.Name
        }).ToArray();

        static NamedItemModel AsNamedItem(DataTable dataTable) => new() {
            Name = dataTable.Name,
            Id = dataTable.PublicId
        };

        static DataTableListItemModel AsListItem(DataTable dataTable) => new() {
            Name = dataTable.Name,
            Id = dataTable.PublicId,
            RowCount = dataTable.RowCount,
            DateCreated = dataTable.DateCreated
        };

        static string GetDefaultColumnName(uint columnIndex) => $"Column {columnIndex+1}";
    }
}