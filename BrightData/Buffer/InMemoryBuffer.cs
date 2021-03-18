using System;
using System.Collections.Generic;
using System.Text;

namespace BrightData.Buffer
{
    class InMemoryBuffer<T> : ICanEnumerate<T>, IAppendableBuffer<T> where T : notnull
    {
        readonly List<T> _data = new List<T>();

        public IEnumerable<T> EnumerateTyped() => _data;
        public void Add(T obj) => _data.Add(obj);

        public uint Size => (uint)_data.Count;
    }
}
