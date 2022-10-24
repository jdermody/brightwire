using BrightData;
using System.ComponentModel.DataAnnotations;

namespace BrightAPI.Models.DataTable.Requests
{
    public class ConvertDataTableColumnsRequest : DataTableColumnsRequest
    {
        [Required] public ColumnConversionType[] ColumnConversions { get; set; } = Array.Empty<ColumnConversionType>();
    }
}
