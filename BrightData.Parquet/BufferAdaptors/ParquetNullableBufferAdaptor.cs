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
        readonly RowGroupReaderProvider _rowGroupProvider = rowGroupProvider;
        readonly int _columnIndex                         = columnIndex;
        readonly T _defaultValue                          = defaultValue;

        protected override async ValueTask<T[]> GetData(uint blockIndex, CancellationToken ct = default)
        {
            var data = await _rowGroupProvider.GetNullableColumn<T>(blockIndex, _columnIndex, ct);
            var ret = new T[data.Length];
            for (int i = 0; i < data.Length; i++)
                ret[i] = data[i] ?? _defaultValue;
            return ret;
        }
    }
}
