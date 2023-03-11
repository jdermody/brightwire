namespace BrightAPI.Models.DataTable
{
    public class DataTablePreviewModel
    {
        public DataTableColumnModel[] Columns { get; set; } = Array.Empty<DataTableColumnModel>();

        public string[][] PreviewRows { get; set; } = Array.Empty<string[]>();
    }
}
