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
        readonly RowGroupReaderProvider _rowGroupProvider = rowGroupProvider;
        readonly int _columnIndex                         = columnIndex;
        readonly Func<DT, T> _mappingFunction             = mappingFunction;

        protected override async ValueTask<T[]> GetData(uint blockIndex, CancellationToken ct = default)
        {
            var data = (DT[])await _rowGroupProvider.GetColumn(typeof(DT), blockIndex, _columnIndex, ct);
            var ret = new T[data.Length];
            for (int i = 0; i < data.Length; i++)
                ret[i] = _mappingFunction(data[i]!);
            return ret;
        }
    }
}