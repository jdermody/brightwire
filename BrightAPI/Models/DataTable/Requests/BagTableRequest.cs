using System.ComponentModel.DataAnnotations;

namespace BrightAPI.Models.DataTable.Requests
{
    public class BagTableRequest
    {
        [Required] public uint RowCount { get; set; }
    }
}
