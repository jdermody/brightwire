using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.DataTable2
{
    internal class Row2 : IDataTableRow
    {
        readonly object[] _data;

        public Row2(BrightDataType[] types, object[] data, uint index)
        {
            Types = types;
            _data = data;
            RowIndex = index;
        }

        public void Dispose()
        {
            ArrayPool<object>.Shared.Return(_data);
        }

        public object this[uint index] => _data[index];

        public BrightDataType[] Types { get; }
        public uint Size => (uint)_data.Length;
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
            return (T)_data[index];
        }

        /// <summary>
        /// Row index
        /// </summary>
        public uint RowIndex { get; }
    }
}
