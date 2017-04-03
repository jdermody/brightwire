using System;
using System.Collections.Generic;
using System.Text;
using BrightWire.TabularData.Helper;

namespace BrightWire.TabularData
{
    internal class ShallowDataTableRow : IRow
    {
        readonly IHaveColumns _table;
        readonly IReadOnlyList<object> _data;
        readonly RowConverter _converter;
        readonly bool _isSubItem;

        public ShallowDataTableRow(IHaveColumns table, IReadOnlyList<object> data, RowConverter converter, bool isSubItem)
        {
            _converter = converter;
            _table = table;
            _data = data;
            _isSubItem = isSubItem;
        }

        public bool IsSubItem { get { return _isSubItem; } }

        public int Depth { get { return 1; } }

        public IReadOnlyList<object> GetData(int depth = 0) { return _data; }

        public T GetField<T>(int index, int depth = 0)
        {
            return _converter.GetField<T>(_data, index);
        }

        public IReadOnlyList<T> GetFields<T>(IReadOnlyList<int> indices, int depth = 0)
        {
            var ret = new T[indices.Count];
            for (int i = 0, len = indices.Count; i < len; i++)
                ret[i] = _converter.GetField<T>(_data, i);
            return ret;
        }
    }
}
