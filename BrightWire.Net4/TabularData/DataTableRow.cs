using System.Collections.Generic;
using BrightWire.TabularData.Helper;

namespace BrightWire.TabularData
{
    /// <summary>
    /// A row within a data table
    /// </summary>
    internal class DataTableRow : IRow
    {
        readonly IHaveColumns _table;
        readonly IReadOnlyList<object> _data;
        readonly RowConverter _converter;

        public DataTableRow(IHaveColumns table, IReadOnlyList<object> data, RowConverter converter)
        {
            _converter = converter;
            _table = table;
            _data = data;
        }

        public IReadOnlyList<object> Data => _data;
        public int Index { get; set; }
        public IHaveColumns Table => _table;

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
