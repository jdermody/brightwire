using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.Helper;

namespace BrightData.DataTable2
{
    internal class Row2 : IDataTableRow
    {
        readonly BrightDataTable _dataTable;
        readonly object[] _data;

        public Row2(BrightDataTable dataTable, object[] data, uint index, uint columnCount)
        {
            _data = data;
            RowIndex = index;
            _dataTable = dataTable;
            Size = columnCount;
        }

        public void Dispose()
        {
            ArrayPool<object>.Shared.Return(_data);
        }

        public object this[uint index] => _data[index];
        public BrightDataType[] Types => _dataTable.ColumnTypes;
        public uint Size { get; }
        public IEnumerable<object> Data => _data;

        public override string ToString()
        {
            return string.Join(",", Types.Zip(_data, (t, d) => $"{Format(d, t)} [{t}]"));
        }

        static string Format(object obj, BrightDataType type)
        {
            if (type == BrightDataType.String)
                return $"\"{obj}\"";
            return obj.ToString() ?? "???";
        }

        /// <summary>
        /// Returns a value (dynamic conversion to type T)
        /// </summary>
        /// <typeparam name="T">Type to convert to</typeparam>
        /// <param name="index">Column index</param>
        /// <returns></returns>
        public T Get<T>(uint index) where T : notnull
        {
            return _dataTable.ConvertObjectTo<T>(index, _data[index]);
        }

        /// <summary>
        /// Row index
        /// </summary>
        public uint RowIndex { get; }
    }
}
