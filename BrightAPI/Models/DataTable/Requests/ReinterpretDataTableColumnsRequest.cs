using System.ComponentModel.DataAnnotations;

namespace BrightAPI.Models.DataTable.Requests
{
    public class ReinterpretDataTableColumnsRequest
    {
        public class NewColumnFromExistingColumnsModel : DataTableColumnsRequest
        {
            [Required] public string Name { get; set; } = string.Empty;
        }

        [Required] public NewColumnFromExistingColumnsModel[] Columns { get; set; } = Array.Empty<NewColumnFromExistingColumnsModel>();
    }
}
