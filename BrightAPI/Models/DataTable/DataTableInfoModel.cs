using System.ComponentModel.DataAnnotations;

namespace BrightAPI.Models.DataTable
{
    public class DataTableInfoModel : NamedItemModel
    {
        [Required] public uint RowCount { get; set; }

        [Required] public NameValueModel[] Metadata { get; set; } = Array.Empty<NameValueModel>();

        [Required] public DataTableColumnModel[] Columns { get; set; } = Array.Empty<DataTableColumnModel>();
    }
}
