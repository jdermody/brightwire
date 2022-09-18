using BrightData;

namespace BrightAPI.Models.DataTable
{
    public class ConvertDataTableColumnsRequest
    {
        public uint[] ColumnIndices { get; set; } = Array.Empty<uint>();
        public ColumnConversionType[] ColumnConversions { get; set; } = Array.Empty<ColumnConversionType>();
    }
}
