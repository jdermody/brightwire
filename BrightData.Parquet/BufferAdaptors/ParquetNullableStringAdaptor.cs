using BrightData.Types;
using Parquet.Data;

namespace BrightData.Parquet.BufferAdaptors
{
    internal class ParquetNullableStringAdaptor(
        RowGroupReaderProvider rowGroupProvider, 
        int columnIndex, 
        MetaData metaData, 
        string defaultValue
    ) : ParquetBufferAdaptor<string>(rowGroupProvider, columnIndex, metaData)
    {
        protected override async ValueTask<string[]> GetData(uint blockIndex, CancellationToken ct = default)
        {
            var data = (string[])await rowGroupProvider.GetColumn(typeof(string), blockIndex, columnIndex, ct);
            var ret = new string[data.Length];
            var index = 0;

            foreach (var item in data)
                ret[index++] = item ?? defaultValue;
            return ret;
        }
    }
}
