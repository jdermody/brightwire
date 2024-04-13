using BrightData.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parquet.Data;

namespace BrightData.Parquet.BufferAdaptors
{
    internal class ParquetNullableBufferAdaptor<T>(RowGroupReaderProvider rowGroupProvider, int columnIndex, MetaData metaData, T defaultValue) : ParquetBufferAdaptor<T>(rowGroupProvider, columnIndex, metaData)
        where T : struct
    {
        protected override T[] GetArray(DataColumn column)
        {
            var data = (T?[])column.Data;
            var ret = new T[data.Length];
            var index = 0;

            foreach (var item in data)
                ret[index++] = item ?? defaultValue;
            return ret;
        }
    }
}
