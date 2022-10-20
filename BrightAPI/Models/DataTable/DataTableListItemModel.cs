using System.ComponentModel.DataAnnotations;

namespace BrightAPI.Models.DataTable
{
    public class DataTableListItemModel : NamedItemModel
    {
        [Required] public uint RowCount { get; set; }
        [Required] public DateTime DateCreated { get; set; }
    }
}
