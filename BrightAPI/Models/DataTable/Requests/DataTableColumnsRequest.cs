using System.ComponentModel.DataAnnotations;

namespace BrightAPI.Models.DataTable.Requests
{
    public class DataTableColumnsRequest
    {
        [Required] public uint[] ColumnIndices { get; set; } = Array.Empty<uint>();
    }
}
