using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using BrightData.Computation;
using BrightData.Helper;
using BrightData.Memory;

namespace BrightData
{
    public class BrightDataContext : IBrightDataContext
    {
        readonly ConcurrentDictionary<string, object> _attachedProperties = new ConcurrentDictionary<string, object>();
        readonly TensorPool _tensorPool;
        readonly DisposableLayers _memoryLayers = new DisposableLayers();
        readonly FloatComputation _floatComputation;
        readonly DoubleComputation _doubleComputation;
        readonly DecimalComputation _decimalComputation;
        readonly UIntComputation _uintComputation;
        readonly DataEncoder _dataReader;

        public BrightDataContext(
            int? randomSeed = null,
            long maxCacheSize = Consts.DefaultMemoryCacheSize)
        {
            Random = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random();
            _tensorPool = new TensorPool(this, new SharedPoolAllocator(), maxCacheSize);
            _dataReader = new DataEncoder(this);

            _floatComputation = new FloatComputation(this);
            _doubleComputation = new DoubleComputation(this);
            _decimalComputation = new DecimalComputation(this);
            _uintComputation = new UIntComputation(this);

            _memoryLayers.Push();
        }

        public void Dispose()
        {
            _memoryLayers.Pop();
#if DEBUG
            _tensorPool.LogAllocations(str => Debug.WriteLine(str));
#endif
            _tensorPool.Dispose();
        }

        public Random Random { get; }
        public ITensorPool TensorPool => _tensorPool;
        public IDisposableLayers MemoryLayer => _memoryLayers;
        public IDataReader DataReader => _dataReader;
        public IComputableFactory ComputableFactory { get; set; }

        public INumericComputation<T> GetComputation<T>() where T : struct
        {
            var typeCode = Type.GetTypeCode(typeof(T));
            switch (typeCode) {
                case TypeCode.Single:
                    return (INumericComputation<T>)_floatComputation;
                case TypeCode.Double:
                    return (INumericComputation<T>)_doubleComputation;
                case TypeCode.Decimal:
                    return (INumericComputation<T>)_decimalComputation;
                case TypeCode.UInt32:
                    return (INumericComputation<T>)_uintComputation;
            }
            throw new NotImplementedException();
        }

        public T Get<T>(string name) => _attachedProperties.TryGetValue(name, out var obj) ? (T)obj : default(T);
        public void Set<T>(string name, T value) => _attachedProperties.AddOrUpdate(name, value, (n, o) => value);
    }
}
