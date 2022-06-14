using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.DataTable2.Bindings
{
    internal delegate T ColumnTypeCallback<CT, out T>(ref CT item) where CT: struct where T: notnull;
    internal class ColumnReaderBinding<CT, T> : IReadColumnTypes<CT>
        where CT: struct
        where T : notnull
    {
        readonly Action<T, uint> _reader;
        readonly ColumnTypeCallback<CT, T> _converter;

        public ColumnReaderBinding(Action<T, uint> reader)
        {
            _reader = reader;
        }

        public void OnItem(ref CT item, uint index)
        {
            _reader(_converter(ref item), index);
        }
    }
}
