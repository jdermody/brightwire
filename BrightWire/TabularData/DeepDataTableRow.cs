using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.TabularData
{
    internal class DeepDataTableRow : IRow
    {
        readonly IReadOnlyList<ShallowDataTableRow> _data = new List<ShallowDataTableRow>();

        public DeepDataTableRow(IReadOnlyList<ShallowDataTableRow> data)
        {
            _data = data;
        }

        public bool IsSubItem { get { return false; } }

        public int Depth { get { return _data.Count; } }

        public IReadOnlyList<object> GetData(int depth = 0) { return _data[depth].GetData(); }

        public T GetField<T>(int index, int depth = 0)
        {
            return _data[depth].GetField<T>(index);
        }

        public IReadOnlyList<T> GetFields<T>(IReadOnlyList<int> indices, int depth = 0)
        {
            return _data[depth].GetFields<T>(indices);
        }

        public IReadOnlyList<IRow> SubItem { get { return _data; } }
    }
}
