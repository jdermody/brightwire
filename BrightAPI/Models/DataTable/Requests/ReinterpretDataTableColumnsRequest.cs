using BrightData;
using System.ComponentModel.DataAnnotations;

namespace BrightAPI.Models.DataTable.Requests
{
    public class ReinterpretDataTableColumnsRequest
    {
        public class NewColumnFromExistingColumnsModel : DataTableColumnsRequest
        {
            [Required] public BrightDataType NewType { get; set; }
            [Required] public string Name { get; set; }
            [Required] public uint OutputColumnIndex { get; set; }
        }

        [Required] public NewColumnFromExistingColumnsModel[] Columns { get; set; } = Array.Empty<NewColumnFromExistingColumnsModel>();
    }
}
