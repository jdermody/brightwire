using System;
using System.Collections.Generic;
using System.Text;
using BrightData.Computation;
using BrightData.Helper;
using BrightData.Memory;

namespace BrightData
{
    public class BrightDataContext : IBrightDataContext
    {
        readonly TensorPool _tensorPool;
        readonly DisposableLayers _memoryLayers = new DisposableLayers();
        readonly FloatComputation _floatComputation;
        readonly DoubleComputation _doubleComputation;
        readonly DecimalComputation _decimalComputation;
        readonly UIntComputation _uintComputation;
        readonly DataEncoder _dataReader;

        public BrightDataContext(long maxCacheSize = Consts.DefaultMemoryCacheSize)
        {
            _floatComputation = new FloatComputation(_tensorPool);
            _doubleComputation = new DoubleComputation(_tensorPool);
            _decimalComputation = new DecimalComputation(_tensorPool);
            _uintComputation = new UIntComputation(_tensorPool);
            _tensorPool = new TensorPool(this, new SharedPoolAllocator(), maxCacheSize);
            _dataReader = new DataEncoder(this);
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

        public ITensorPool TensorPool => _tensorPool;
        public IDisposableLayers MemoryLayer => _memoryLayers;
        public IDataReader DataReader => _dataReader;

        public INumericComputation<T> GetComputation<T>()
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
    }
}
