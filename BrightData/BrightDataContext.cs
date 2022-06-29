using System;
using System.Collections.Concurrent;
using System.Threading;
using BrightData.Helper;
using BrightData.LinearAlegbra2;
using BrightData.LinearAlgebra;
using BrightData.LinearAlgebra.Memory;

namespace BrightData
{
    /// <summary>
    /// Bright data context
    /// </summary>
    public class BrightDataContext : ISetLinearAlgebraProvider, IDisposable
    {
        Lazy<LinearAlgebraProvider>                   _lap;
        readonly ConcurrentDictionary<string, object> _attachedProperties = new();
        readonly TensorPool                           _tensorPool;
        readonly DisposableLayers                     _memoryLayers = new();
        readonly DataEncoder                          _dataReader;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="randomSeed">Initial value of random seed (or null to randomly initialize)</param>
        public BrightDataContext(LinearAlgebraProvider? lap = null, int? randomSeed = null)
        {
            IsStochastic = !randomSeed.HasValue;
            Random = randomSeed.HasValue 
                ? new Random(randomSeed.Value) 
                : new Random()
            ;
            _tensorPool = new TensorPool();
            _dataReader = new DataEncoder(this);

            _memoryLayers.Push();
            if (lap is not null)
                _lap = new(lap);
            else
                _lap = new(() => new LinearAlgebraProvider(this));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _memoryLayers.Pop();
            _tensorPool.Dispose();
            if(_lap.IsValueCreated)
                _lap.Value.Dispose();
        }

        /// <inheritdoc />
        public Random Random { get; private set; }

        /// <inheritdoc />
        public ITensorPool TensorPool => _tensorPool;

        /// <inheritdoc />
        public IDisposableLayers MemoryLayer => _memoryLayers;

        /// <inheritdoc />
        public IDataReader DataReader => _dataReader;

        /// <summary>
        /// Linear algebra provider
        /// </summary>
        public LinearAlgebraProvider LinearAlgebraProvider2
        {
            get => _lap.Value;
            set
            {
                if(_lap.IsValueCreated)
                    _lap.Value.Dispose();
                _lap = new(value);
            }
        }

        public IProvideTempStreams CreateTempStreamProvider() => new TempStreamManager(Get<string>(Consts.BaseTempPath));

        /// <inheritdoc />
        public T Get<T>(string name, T defaultValue) where T : notnull => _attachedProperties.TryGetValue(name, out var obj) ? (T)obj : defaultValue;

        /// <inheritdoc />
        public T Get<T>(string name, Func<T> defaultValueCreator) where T : notnull => (T)_attachedProperties.GetOrAdd(name, _ => defaultValueCreator());

        /// <inheritdoc />
        public T? Get<T>(string name) where T : class => _attachedProperties.TryGetValue(name, out var obj) ? (T)obj : null;

        /// <inheritdoc />
        public T Set<T>(string name, T value) where T:notnull => (T)_attachedProperties.AddOrUpdate(name, value, (_, _) => value);

        /// <inheritdoc />
        public T Set<T>(string name, Func<T> valueCreator) where T : notnull => (T)_attachedProperties.AddOrUpdate(name, _ => valueCreator(), (_, o) => o);

        /// <inheritdoc />
        public bool IsStochastic { get; }

        /// <inheritdoc />
        public void ResetRandom(int? seed) => Random = seed.HasValue ? new Random(seed.Value) : new Random();

        /// <inheritdoc />
        public INotifyUser? UserNotifications { get; set; } = new ConsoleProgressNotification();

        /// <inheritdoc />
        public CancellationToken CancellationToken { get; set; } = default;
    }
}
