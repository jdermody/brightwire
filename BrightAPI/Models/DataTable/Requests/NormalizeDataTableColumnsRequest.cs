using BrightData;
using System.ComponentModel.DataAnnotations;

namespace BrightAPI.Models.DataTable.Requests
{
    public class NormalizeDataTableColumnsRequest : DataTableColumnsRequest
    {
        [Required] public NormalizationType[] Columns { get; set; } = Array.Empty<NormalizationType>();
    }
}
