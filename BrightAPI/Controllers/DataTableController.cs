using System.Text;
using System.Text.Json;
using BrightAPI.Database;
using BrightAPI.Helper;
using BrightAPI.Models;
using BrightAPI.Models.DataTable;
using BrightData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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
        public ActionResult<DataTablePreviewModel> GetPreviewFromCsv([FromBody] DataTableCsvPreviewRequest request)
        {
            var filePreview = string.Join('\n', request.Lines);
            if (string.IsNullOrWhiteSpace(filePreview))
                return BadRequest();

            try {
                using var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(filePreview)));
                using var table = _context.ParseCsvIntoMemory(reader, request.HasHeader, request.Delimiter);
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
                    PreviewRows = table.AllRows.Select(r => r.ToArray().Select(x => x.ToString()!).ToArray()).ToArray()
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
        public async Task<ActionResult<NamedItemModel>> CreateNewTableFromCSV([FromBody] DataTableCsvRequest request)
        {
            var fileData = string.Join('\n', request.Lines);
            if (string.IsNullOrWhiteSpace(fileData))
                return BadRequest();

            var columnNames = request.ColumnNames?.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            try {
                var dataTable = await CreateDataTableFromCSV(_databaseManager, _context, _tempFileManager, request.HasHeader, request.Delimiter, request.TargetIndex, columnNames, request.FileName, fileData);
                return AsNamedItem(dataTable);
            }
            catch (Exception ex) {
                _logger.LogError(ex, null);
            }
            return BadRequest();
        }
        public static async Task<DataTable> CreateDataTableFromCSV(
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
            using var table = context.ParseCsvIntoMemory(reader, hasHeader, delimiter);
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
            table.GetColumnAnalysis(table.ColumnIndices);
            if (fileName is not null)
                table.TableMetaData.Set("file-name", fileName);
            table.TableMetaData.Set("date-created", DateTime.UtcNow);
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
            var dataTableInfo = await _databaseManager.GetDataTable(id);
            if (dataTableInfo is null)
                return NotFound();

            if (!System.IO.File.Exists(dataTableInfo.LocalPath))
                return Problem("Local file not found");

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
                Metadata = AsModel(table.TableMetaData),
                RowCount = table.RowCount
            };
            return ret;
        }

        [HttpGet, Route("{id}/{start}/{count}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<object[][]>> GetDataTableData(string id, uint start, uint count)
        {
            var dataTableInfo = await _databaseManager.GetDataTable(id);
            if (dataTableInfo is null)
                return NotFound();

            if (!System.IO.File.Exists(dataTableInfo.LocalPath))
                return Problem("Local file not found");

            using var table = _context.LoadTable(dataTableInfo.LocalPath);
            var ret = table.GetSlice(start, count).Select(r => r.ToArray().ToArray()).ToArray();
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

            var dataTableInfo = await _databaseManager.GetDataTable(id);
            if (dataTableInfo is null)
                return NotFound();

            using var table = _context.LoadTable(dataTableInfo.LocalPath);
            var columnConversions = new ColumnConversionType[table.ColumnCount];
            for (uint i = 0; i < len; i++) {
                if (i > table.ColumnCount)
                    return BadRequest($"Column index exceeded column count: {i}");
                columnConversions[request.ColumnIndices[i]] = request.ColumnConversions[i];
            }

            // check if there is anything to convert
            if (columnConversions.All(x => x == ColumnConversionType.Unchanged)) {
                return new NamedItemModel {
                    Id = dataTableInfo.PublicId,
                    Name = dataTableInfo.Name
                };
            }

            // create a new converted table
            var (path, newTableId) = _tempFileManager.GetNewTempPath();
            using var newTable = table.Convert(path, columnConversions.Where(x => x != ColumnConversionType.Unchanged).Select((c, i) => c.ConvertColumn((uint)i)).ToArray());

            // set table metadata
            var requestJson = JsonSerializer.Serialize(request);
            newTable.GetColumnAnalysis(newTable.ColumnIndices);
            newTable.TableMetaData.Set("based-on-table-id", id);
            newTable.TableMetaData.Set("transformation-request", requestJson);
            newTable.TableMetaData.Set("date-created", DateTime.UtcNow);
            newTable.PersistMetaData();

            var newTableInfo = await _databaseManager.CreateDataTable(dataTableInfo.Name + " [Converted]", newTableId, path, table.RowCount);
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
            var dataTable = await _databaseManager.GetDataTable(id);
            if (dataTable is null)
                return NotFound();

            await _databaseManager.DeleteDataTable(dataTable);

            // delete the file on disk
            var fileInfo = new FileInfo(dataTable.LocalPath);
            if(fileInfo.Exists)
                fileInfo.Delete();

            return Ok();
        }

        static NameValueModel[] AsModel(MetaData metadata) => metadata.GetNonEmpty().Select(x => new NameValueModel {
            Value = x.String,
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