using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using BrightData.Helper;
using BrightData.LinearAlgebra;

namespace BrightData
{
    /// <summary>
    /// Bright data context
    /// </summary>
    public class BrightDataContext : ISetLinearAlgebraProvider, IDisposable
    {
        Lazy<LinearAlgebraProvider>                   _lap;
        readonly ConcurrentDictionary<string, object> _attachedProperties = new();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="lap">Linear algebra provider to use (optional)</param>
        /// <param name="randomSeed">Initial value of random seed (or null to randomly initialize)</param>
        public BrightDataContext(LinearAlgebraProvider? lap = null, int? randomSeed = null)
        {
            IsStochastic = !randomSeed.HasValue;
            Random = randomSeed.HasValue 
                ? new Random(randomSeed.Value) 
                : new Random()
            ;
            if (lap is not null)
                _lap = new(lap);
            else
                _lap = new(() => new LinearAlgebraProvider(this));

            Set(Consts.DateTimeCreated, DateTime.Now);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if(_lap.IsValueCreated)
                _lap.Value.Dispose();
        }

        public Random Random { get; private set; }

        /// <summary>
        /// Linear algebra provider
        /// </summary>
        public LinearAlgebraProvider LinearAlgebraProvider
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

        public T Get<T>(string name, T defaultValue) where T : notnull => _attachedProperties.TryGetValue(name, out var obj) ? (T)obj : defaultValue;

        public T Get<T>(string name, Func<T> defaultValueCreator) where T : notnull => (T)_attachedProperties.GetOrAdd(name, _ => defaultValueCreator());

        public T? Get<T>(string name) where T : class => _attachedProperties.TryGetValue(name, out var obj) ? (T)obj : null;

        public bool TryGet<T>(string name, [NotNullWhen(true)]out T? ret)
        {
            if (_attachedProperties.TryGetValue(name, out var obj)) {
                ret = (T)obj;
                return true;
            }

            ret = default;
            return false;
        }

        public T Set<T>(string name, T value) where T:notnull => (T)_attachedProperties.AddOrUpdate(name, value, (_, _) => value);

        public T Set<T>(string name, Func<T> valueCreator) where T : notnull => (T)_attachedProperties.AddOrUpdate(name, _ => valueCreator(), (_, o) => o);

        public void Clear(string name) => _attachedProperties.TryRemove(name, out var _);

        public bool IsStochastic { get; }

        public void ResetRandom(int? seed) => Random = seed.HasValue ? new Random(seed.Value) : new Random();

        public INotifyUser? UserNotifications { get; set; } = new ConsoleProgressNotification();
    }
}
