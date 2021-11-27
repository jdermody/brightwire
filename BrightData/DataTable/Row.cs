using System.Collections.Generic;
using System.Linq;

namespace BrightData.DataTable
{
    /// <summary>
    /// A row within a data table
    /// </summary>
    internal class Row : IDataTableSegment
    {
        readonly object[] _data;

        public Row(BrightDataType[] types, object[] data)
        {
            Types = types;
            _data = data;
        }

        public object this[uint index] => _data[index];

        public BrightDataType[] Types { get; }
        public uint Size => (uint)_data.Length;
        public IEnumerable<object> Data => _data;

        public override string ToString()
        {
            return string.Join(",", _data.Zip(Types, (d, t) => $"{Format(d, t)} [{t}]"));
        }

        static string Format(object obj, BrightDataType type)
        {
            if (type == BrightDataType.String)
                return $"\"{obj}\"";
            return obj.ToString() ?? "???";
        }
    }
}
