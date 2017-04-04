using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Helper
{
    class PropertySet : IPropertySet
    {
        readonly Dictionary<string, object> _data = new Dictionary<string, object>();
        readonly ILinearAlgebraProvider _lap;

        public PropertySet(ILinearAlgebraProvider lap)
        {
            _lap = lap;
        }

        public ILinearAlgebraProvider LinearAlgebraProvider { get { return _lap; } }

        public T Get<T>(string name)
        {
            object obj;
            if(_data.TryGetValue(name, out obj))
                return (T)obj;
            return default(T);
        }

        public void Set<T>(string name, T obj)
        {
            _data[name] = obj;
        }

        public void Set(string name, object obj)
        {
            _data[name] = obj;
        }

        public void Clear(string name)
        {
            _data.Remove(name);
        }
    }
}
