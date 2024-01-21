using System.ComponentModel.DataAnnotations;

namespace BrightAPI.Models.DataTable
{
    public class DataTableInfoModel : DataTableListItemModel
    {
        [Required] public NameValueModel[] Metadata { get; set; } = [];

        [Required] public DataTableColumnModel[] Columns { get; set; } = [];
    }
}
