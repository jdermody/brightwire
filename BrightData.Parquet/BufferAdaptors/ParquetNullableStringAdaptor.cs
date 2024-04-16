using BrightData.Types;
using Parquet.Data;

namespace BrightData.Parquet.BufferAdaptors
{
    internal class ParquetNullableStringAdaptor(RowGroupReaderProvider rowGroupProvider, int columnIndex, MetaData metaData, string defaultValue) : ParquetBufferAdaptor<string>(rowGroupProvider, columnIndex, metaData)
    {
        protected override string[] GetArray(DataColumn column)
        {
            var data = (string?[])column.Data;
            var ret = new string[data.Length];
            var index = 0;

            foreach (var item in data)
                ret[index++] = item ?? defaultValue;
            return ret;
        }
    }
}
