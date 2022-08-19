using System.Text;
using BrightAPI.Database;
using BrightAPI.Models.DataTable;
using BrightData;
using Microsoft.AspNetCore.Mvc;

namespace BrightAPI.Controllers
{
    [ApiController, Route("[controller]")]
    public class DataTableController : BaseController
    {
        readonly ILogger<DataTableController> _logger;
        readonly DatabaseManager              _databaseManager;
        readonly BrightDataContext            _context;

        public DataTableController(
            ILogger<DataTableController> logger, 
            DatabaseManager databaseManager,
            BrightDataContext context
        ) {
            _logger = logger;
            _databaseManager = databaseManager;
            _context = context;
        }

        [HttpGet, Route("preview")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<DataTablePreviewModel> GetPreviewFromCsv(bool hasHeader, char delimiter, uint? targetIndex, string fileType, [FromBody] string filePreview)
        {
            try {
                using var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(filePreview)));
                using var table = _context.ParseCsvIntoMemory(reader, hasHeader, delimiter);
                if (targetIndex.HasValue)
                    table.SetTargetColumn(targetIndex.Value);
                var columns = new DataTableColumnModel[table.ColumnCount];
                for (var i = 0; i < table.ColumnCount; i++) {
                    var metaData = table.ColumnMetaData[i];
                    columns[i] = new DataTableColumnModel {
                        ColumnType = table.ColumnTypes[i],
                        IsTarget = metaData.IsTarget(),
                        Name = metaData.GetName()
                    };
                }
                return new DataTablePreviewModel {
                    Columns = columns
                };
            }
            catch(Exception ex) {
                _logger.LogError(ex, null);
            }
            return BadRequest();
        }
    }
}