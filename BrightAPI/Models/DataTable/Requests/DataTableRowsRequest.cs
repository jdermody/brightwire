using BrightData;
using System.ComponentModel.DataAnnotations;

namespace BrightAPI.Models.DataTable.Requests
{
    public class DataTableRowsRequest
    {
        public class RangeModel
        {
            [Required] public uint FirstInclusiveRow { get; set; }
            [Required] public uint LastInclusiveRow { get; set; }
        }

        [Required] public RangeModel[] RowRanges { get; set; } = Array.Empty<RangeModel>();
    }
}
