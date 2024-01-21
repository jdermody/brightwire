namespace BrightAPI.Models.DataTable.Requests
{
    public class RenameTableColumnsRequest : DataTableColumnsRequest
    {
        public string[] ColumnsNames { get; set; } = [];
    }
}
