using System.ComponentModel.DataAnnotations;

namespace BrightAPI.Models.DataTable.Requests
{
    public class DataTableCsvPreviewRequest
    {
        [Required] public bool HasHeader { get; set; }
        [Required] public char Delimiter { get; set; }
        [Required] public string[] Lines { get; set; } = Array.Empty<string>();
    }
}
