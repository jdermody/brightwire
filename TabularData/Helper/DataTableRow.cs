using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.TabularData.Helper
{
    public class DataTableRow : IRow
    {
        readonly IDataTable _table;
        readonly List<object> _data;

        public DataTableRow(IDataTable table, IEnumerable<object> data)
        {
            _table = table;
            _data = new List<object>(data);
        }

        public IReadOnlyList<object> Data { get { return _data; } }

        public static T GetField<T>(IReadOnlyList<object> data, int index)
        {
            var ret = data[index];
            var targetType = typeof(T);
            if (ret.GetType() == targetType)
                return (T)ret;
            return (T)Convert.ChangeType(ret, targetType);
        }

        public T GetField<T>(int index)
        {
            return GetField<T>(_data, index);
        }
        public IEnumerable<float> GetNumericFields(IEnumerable<int> fields)
        {
            return fields.Select(i => Convert.ToSingle(_data[i]));
        }
        public IColumn GetColumn(int index)
        {
            if (index >= 0 && index < _table.Columns.Count)
                return _table.Columns[index];
            return null;
        }
    }
}
