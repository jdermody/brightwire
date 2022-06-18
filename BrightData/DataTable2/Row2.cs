using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.DataTable2
{
    internal class Row2 : IDisposableDataTableSegment
    {
        readonly object[] _data;

        public Row2(BrightDataType[] types, object[] data, uint size)
        {
            Types = types;
            _data = data;
            Size = size;
        }

        public void Dispose()
        {
            ArrayPool<object>.Shared.Return(_data);
        }

        public object this[uint index] => _data[index];

        public BrightDataType[] Types { get; }
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
    }
}
