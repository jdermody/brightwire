using System.ComponentModel.DataAnnotations;
using BrightData;

namespace BrightAPI.Models.DataTable
{
    public class DataTableColumnModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public BrightDataType ColumnType { get; set; }

        public bool IsTarget { get; set; }

        public NameValueModel[]? Metadata { get; set; }
    }
}
