using System;
using System.Collections.Generic;

namespace BrightData.Buffer
{
    class InMemoryBuffer<T> : ICanEnumerate<T>, IAcceptSequentialTypedData<T> where T : notnull
    {
        readonly List<T> _data = new();

        public IEnumerable<T> EnumerateTyped() => _data;
        public void Add(T obj) => _data.Add(obj);
        public void Add(Span<T> data)
        {
            foreach(var item in data)
                _data.Add(item);
        }

        public uint Size => (uint)_data.Count;
    }
}
