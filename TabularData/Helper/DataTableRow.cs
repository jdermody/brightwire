using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.TabularData.Helper
{
    internal class DataTableRow : IRow
    {
        readonly IHaveColumns _table;
        readonly object[] _data;
        readonly RowConverter _converter;

        public DataTableRow(IHaveColumns table, object[] data, RowConverter converter)
        {
            _converter = converter;
            _table = table;
            _data = data;
        }

        public IReadOnlyList<object> Data { get { return _data; } }

        public T GetField<T>(int index)
        {
            return _converter.GetField<T>(_data, index);
        }

        public IReadOnlyList<T> GetFields<T>(IReadOnlyList<int> indices)
        {
            var ret = new T[indices.Count];
            for (int i = 0, len = indices.Count; i < len; i++)
                ret[i] = _converter.GetField<T>(_data, i);
            return ret;
        }
    }
}
