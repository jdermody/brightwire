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
        readonly List<object> _data;

        public DataTableRow(IHaveColumns table, IEnumerable<object> data)
        {
            _table = table;
            _data = new List<object>(data);
        }

        public IReadOnlyList<object> Data { get { return _data; } }

        public static T GetField<T>(IReadOnlyList<object> data, int index)
        {
            if (index < 0 || index > data.Count)
                throw new IndexOutOfRangeException();

            var ret = data[index];
            if (ret == null)
                return default(T);

            var targetType = typeof(T);
            var retType = ret.GetType();

            if (retType == targetType)
                return (T)ret;
            else if (retType == typeof(DateTime)) {
                if (targetType == typeof(string))
                    ret = ((DateTime)ret).ToString();
                else
                    ret = ((DateTime)ret).Ticks;
            }
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
