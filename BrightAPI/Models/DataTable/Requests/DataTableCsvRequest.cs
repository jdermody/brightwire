using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BrightAPI.Models.DataTable.Requests
{
    public class DataTableCsvRequest : DataTableCsvPreviewRequest
    {
        [Required, MaybeNull]
        public string FileName { get; set; }

        [Required, MaybeNull]
        public string ColumnNames { get; set; }

        public uint? TargetIndex { get; set; }
    }
}
