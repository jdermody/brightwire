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
            GC.SuppressFinalize(this);
            if(_lap.IsValueCreated)
                _lap.Value.Dispose();
        }

        /// <summary>
        /// Default random number generator
        /// </summary>
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

        /// <summary>
        /// Creates a new temp stream provider
        /// </summary>
        /// <returns></returns>
        public IProvideTempStreams CreateTempStreamProvider() => new TempStreamManager(Get<string>(Consts.BaseTempPath));

        /// <summary>
        /// Returns a typed property from the context
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">Property name</param>
        /// <param name="defaultValue">Value to return if the property was not set</param>
        /// <returns></returns>
        public T Get<T>(string name, T defaultValue) where T : notnull => _attachedProperties.TryGetValue(name, out var obj) ? (T)obj : defaultValue;

        /// <summary>
        /// Returns a typed property from the context
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">Property name</param>
        /// <param name="defaultValueCreator">Callback to return a value if the property was not set</param>
        /// <returns></returns>
        public T Get<T>(string name, Func<T> defaultValueCreator) where T : notnull => (T)_attachedProperties.GetOrAdd(name, _ => defaultValueCreator());

        /// <summary>
        /// Returns a typed property from the context (or null if the property was not set)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">Property name</param>
        /// <returns></returns>
        public T? Get<T>(string name) where T : class => _attachedProperties.TryGetValue(name, out var obj) ? (T)obj : null;

        /// <summary>
        /// Tries to get a typed property from the context
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">Property name</param>
        /// <param name="ret">The property value (if set)</param>
        /// <returns>True if the property was set</returns>
        public bool TryGet<T>(string name, [NotNullWhen(true)]out T? ret)
        {
            if (_attachedProperties.TryGetValue(name, out var obj)) {
                ret = (T)obj;
                return true;
            }

            ret = default;
            return false;
        }

        /// <summary>
        /// Sets a typed property in this context
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">Property name</param>
        /// <param name="value">Property value</param>
        /// <returns></returns>
        public T Set<T>(string name, T value) where T:notnull => (T)_attachedProperties.AddOrUpdate(name, value, (_, _) => value);

        /// <summary>
        /// Removes a typed property from the context
        /// </summary>
        /// <param name="name">Property name</param>
        public void Clear(string name) => _attachedProperties.TryRemove(name, out var _);

        /// <summary>
        /// True if the context does not use a predefined random seed
        /// </summary>
        public bool IsStochastic { get; }

        /// <summary>
        /// Resets the random seed
        /// </summary>
        /// <param name="seed"></param>
        public void ResetRandom(int? seed) => Random = seed.HasValue ? new Random(seed.Value) : new Random();

        /// <summary>
        /// Optional interface to provide user notification of long running operations
        /// </summary>
        public INotifyUser? UserNotifications { get; set; } = new ConsoleProgressNotification();
    }
}
