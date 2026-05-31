using BrightData.Types;
using Parquet.Data;

namespace BrightData.Parquet.BufferAdaptors
{
    internal class ParquetMappedBufferAdaptor<DT, T>(
        RowGroupReaderProvider rowGroupProvider,
        int columnIndex,
        MetaData metaData,
        Func<DT, T> mappingFunction
    ) : ParquetBufferAdaptor<T>(rowGroupProvider, columnIndex, metaData)
        where DT : notnull
        where T : notnull
    {
        protected override async ValueTask<T[]> GetData(uint blockIndex, CancellationToken ct = default)
        {
            var data = await rowGroupProvider.GetColumn(typeof(DT), blockIndex, columnIndex, ct);
            var ret = new T[data.Length];
            var index = 0;

            foreach (var item in data)
                ret[index++] = mappingFunction((DT)item!);
            return ret;
        }
    }
}