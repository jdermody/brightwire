using System.ComponentModel.DataAnnotations;

namespace BrightAPI.Models.DataTable
{
    public class DataTableInfoModel : DataTableListItemModel
    {
        [Required] public NameValueModel[] Metadata { get; set; } = Array.Empty<NameValueModel>();

        [Required] public DataTableColumnModel[] Columns { get; set; } = Array.Empty<DataTableColumnModel>();
    }
}
