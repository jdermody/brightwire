using System.ComponentModel.DataAnnotations;

namespace BrightAPI.Models.DataTable.Requests
{
    public class VectoriseDataTableColumnsRequest : DataTableColumnsRequest
    {
        [Required] public bool OneHotEncodeToMultipleColumns { get; set; }

    }
}
