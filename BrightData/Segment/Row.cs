using System.Collections.Generic;
using System.Linq;

namespace BrightData.Segment
{
    internal class Row : IDataTableSegment
    {
        readonly object[] _data;

        public Row(ColumnType[] types, object[] data)
        {
            Types = types;
            _data = data;
        }

        public object this[uint index] => _data[index];

        public ColumnType[] Types { get; }
        public uint Size => (uint)_data.Length;
        public IEnumerable<object> Data => _data;

        public override string ToString()
        {
            return string.Join(",", _data.Zip(Types, (d, t) => $"{Format(d, t)} [{t}]"));
        }

        string Format(object obj, ColumnType type)
        {
            if (type == ColumnType.String)
                return $"\"{obj}\"";
            return obj.ToString() ?? "???";
        }
    }
}
