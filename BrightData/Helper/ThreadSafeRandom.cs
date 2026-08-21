using System;
using System.Threading;

namespace BrightData.Helper
{
    /// <summary>
    /// A thread-safe wrapper around <see cref="Random"/> that can be shared safely across multiple threads.
    /// When no seed is supplied the underlying generator is randomly initialized.
    /// </summary>
    /// <param name="seed">Initial value of random seed (or null to randomly initialize)</param>
    public sealed class ThreadSafeRandom(int? seed = null)
    {
        readonly Lock _lock = new();
        Random _random = seed.HasValue
            ? new Random(seed.Value)
            : new Random();

        /// <summary>
        /// Resets the underlying random number generator (optionally to a new seed)
        /// </summary>
        /// <param name="seed">New seed value (or null for a randomly initialized seed)</param>
        public void Reset(int? seed = null)
        {
            lock (_lock)
                _random = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        /// <summary>
        /// Returns a non-negative random integer (thread safe)
        /// </summary>
        /// <returns></returns>
        public int Next()
        {
            lock (_lock)
                return _random.Next();
        }

        /// <summary>
        /// Returns a non-negative random integer less than the specified maximum (thread safe)
        /// </summary>
        /// <param name="maxExclusive">Exclusive upper bound</param>
        /// <returns></returns>
        public int Next(int maxExclusive)
        {
            lock (_lock)
                return _random.Next(maxExclusive);
        }

        /// <summary>
        /// Returns a random integer between a minimum and maximum (thread safe)
        /// </summary>
        /// <param name="minInclusive">Inclusive lower bound</param>
        /// <param name="maxExclusive">Exclusive upper bound</param>
        /// <returns></returns>
        public int Next(int minInclusive, int maxExclusive)
        {
            lock (_lock)
                return _random.Next(minInclusive, maxExclusive);
        }

        /// <summary>
        /// Returns a random double between 0.0 and 1.0 (thread safe)
        /// </summary>
        /// <returns></returns>
        public double NextDouble()
        {
            lock (_lock)
                return _random.NextDouble();
        }

        /// <summary>
        /// Returns a random float between 0f and 1f (thread safe)
        /// </summary>
        /// <returns></returns>
        public float NextSingle()
        {
            lock (_lock)
                return _random.NextSingle();
        }
    }
}