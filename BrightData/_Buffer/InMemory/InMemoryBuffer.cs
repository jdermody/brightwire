using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightData.Buffer.InMemory
{
    class InMemoryBuffer<T> : ICanEnumerateWithSize<T>, IAcceptSequentialTypedData<T> where T : notnull
    {
        readonly List<T> _data = new();

        IEnumerable<object> ICanEnumerate.Values => Values.Cast<object>();
        public IEnumerable<T> Values => _data;
        public void Add(T obj) => _data.Add(obj);
        public void Add(Span<T> data)
        {
            foreach (var item in data)
                _data.Add(item);
        }
        public uint Size => (uint)_data.Count;
    }
}
