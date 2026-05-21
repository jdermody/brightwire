using BrightData.Types;
using Parquet.Data;

namespace BrightData.Parquet.BufferAdaptors
{
    internal class ParquetNullableBufferAdaptor<T>(
        RowGroupReaderProvider rowGroupProvider, 
        int columnIndex, 
        MetaData metaData, 
        T defaultValue
    ) : ParquetBufferAdaptor<T>(rowGroupProvider, columnIndex, metaData)
        where T : struct
    {
        protected override async ValueTask<T[]> GetData(uint blockIndex, CancellationToken ct = default)
        {
            var data = await rowGroupProvider.GetNullableColumn<T>(blockIndex, columnIndex, ct);
            var ret = new T[data.Length];
            var index = 0;

            foreach (var item in data)
                ret[index++] = item ?? defaultValue;
            return ret;
        }
    }
}
