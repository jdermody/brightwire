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
        readonly RowGroupReaderProvider _rowGroupProvider = rowGroupProvider;
        readonly int _columnIndex                         = columnIndex;
        readonly string _defaultValue                     = defaultValue;

        protected override async ValueTask<string[]> GetData(uint blockIndex, CancellationToken ct = default)
        {
            var data = (string[])await _rowGroupProvider.GetColumn(typeof(string), blockIndex, _columnIndex, ct);
            var ret = new string[data.Length];
            for (int i = 0; i < data.Length; i++)
                ret[i] = data[i] ?? _defaultValue;
            return ret;
        }
    }
}
