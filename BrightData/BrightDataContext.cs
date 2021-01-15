using System;
using System.Collections.Concurrent;
using BrightData.Computation;
using BrightData.FloatTensors;
using BrightData.Helper;
using BrightData.Memory;

namespace BrightData
{
    /// <summary>
    /// Bright data context
    /// </summary>
    public class BrightDataContext : IBrightDataContext, ISetLinearAlgebraProvider
    {
        private ILinearAlgebraProvider _lap;
        readonly ConcurrentDictionary<string, object> _attachedProperties = new ConcurrentDictionary<string, object>();
        readonly TensorPool _tensorPool;
        readonly DisposableLayers _memoryLayers = new DisposableLayers();
        readonly FloatComputation _floatComputation;
        readonly DoubleComputation _doubleComputation;
        readonly DecimalComputation _decimalComputation;
        readonly UIntComputation _uintComputation;
        readonly DataEncoder _dataReader;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="randomSeed">Initial value of random seed (or null to randomly initialize)</param>
        /// <param name="maxCacheSize">Max size of cache to store in memory</param>
        public BrightDataContext(
            int? randomSeed = null,
            long maxCacheSize = Consts.DefaultMemoryCacheSize)
        {
            IsStochastic = !randomSeed.HasValue;
            Random = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random();
            _tensorPool = new TensorPool(maxCacheSize);
            _dataReader = new DataEncoder(this);

            _floatComputation = new FloatComputation(this);
            _doubleComputation = new DoubleComputation(this);
            _decimalComputation = new DecimalComputation(this);
            _uintComputation = new UIntComputation(this);

            _memoryLayers.Push();
            _lap = new SimpleLinearAlgebraProvider(this, true);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _memoryLayers.Pop();
            _tensorPool.Dispose();
            TempStreamProvider.Dispose();
            LinearAlgebraProvider?.Dispose();
        }

        /// <inheritdoc />
        public Random Random { get; }

        /// <inheritdoc />
        public ITensorPool TensorPool => _tensorPool;

        /// <inheritdoc />
        public IDisposableLayers MemoryLayer => _memoryLayers;

        /// <inheritdoc />
        public IDataReader DataReader => _dataReader;

        /// <inheritdoc />
        public INumericComputation<T> GetComputation<T>() where T : struct
        {
            var typeCode = Type.GetTypeCode(typeof(T));
            return typeCode switch {
                TypeCode.Single => (INumericComputation<T>) _floatComputation,
                TypeCode.Double => (INumericComputation<T>) _doubleComputation,
                TypeCode.Decimal => (INumericComputation<T>) _decimalComputation,
                TypeCode.UInt32 => (INumericComputation<T>) _uintComputation,
                _ => throw new NotImplementedException()
            };
        }

        /// <inheritdoc />
        public ILinearAlgebraProvider LinearAlgebraProvider
        {
            get => _lap;
            set
            {
                _lap.Dispose();
                _lap = value;
            }
        }

        /// <inheritdoc />
        public IProvideTempStreams TempStreamProvider { get; } = new TempStreamManager();

        /// <inheritdoc />
        public T Get<T>(string name) => _attachedProperties.TryGetValue(name, out var obj) ? (T)obj : default;

        /// <inheritdoc />
        public T Set<T>(string name, T value) where T:notnull => (T)_attachedProperties.AddOrUpdate(name, value, (n, o) => value);

        /// <inheritdoc />
        public T Set<T>(string name, Func<T> valueCreator) where T : notnull => (T)_attachedProperties.AddOrUpdate(name, n => valueCreator(), (n, o) => o);

        /// <inheritdoc />
        public bool IsStochastic { get; }
    }
}
