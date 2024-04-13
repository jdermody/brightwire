﻿using BrightData.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parquet.Data;

namespace BrightData.Parquet.BufferAdaptors
{
    internal class ParquetMappedBufferAdaptor<DT, T>(RowGroupReaderProvider rowGroupProvider, int columnIndex, MetaData metaData, Func<DT, T> mappingFunction) : ParquetBufferAdaptor<T>(rowGroupProvider, columnIndex, metaData)
        where T : notnull
    {
        protected override T[] GetArray(DataColumn column)
        {
            var data = (DT[])column.Data;
            var ret = new T[data.Length];
            var index = 0;

            foreach (var item in data)
                ret[index++] = mappingFunction(item);
            return ret;
        }
    }
}
