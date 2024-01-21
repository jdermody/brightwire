namespace BrightAPI.Models.DataTable
{
    public class DataTablePreviewModel
    {
        public DataTableColumnModel[] Columns { get; set; } = [];

        public string[][] PreviewRows { get; set; } = [];
    }
}
