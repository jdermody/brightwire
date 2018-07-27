using System.Collections.Generic;
using BrightWire.TabularData.Helper;

namespace BrightWire.TabularData
{
    /// <summary>
    /// A row within a data table
    /// </summary>
    class DataTableRow : IRow
    {
	    readonly RowConverter _converter;

        public DataTableRow(IHaveColumns table, IReadOnlyList<object> data, RowConverter converter)
        {
            _converter = converter;
            Table = table;
            Data = data;
        }

        public IReadOnlyList<object> Data { get; }
	    public int Index { get; set; }
        public IHaveColumns Table { get; }

	    public T GetField<T>(int index) => _converter.GetField<T>(Data, index);

        public IReadOnlyList<T> GetFields<T>(IReadOnlyList<int> indices)
        {
            var ret = new T[indices.Count];
            for (int i = 0, len = indices.Count; i < len; i++)
                ret[i] = _converter.GetField<T>(Data, indices[i]);
            return ret;
        }
    }
}
