using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace BrightData.Memory
{
    /// <summary>
    /// Tensor memory pool
    /// </summary>
    internal class TensorPool : ITensorPool, IDisposable
    {
        readonly ConcurrentDictionary<string, ConcurrentBag<Array>> _cache = new ConcurrentDictionary<string, ConcurrentBag<Array>>();
        readonly ConcurrentDictionary<string, long> _requestHistory = new ConcurrentDictionary<string, long>();
        long _requestIndex = 0;

        public TensorPool(long maxCacheSize)
        {
            MaxCacheSize = maxCacheSize;
        }

        public T[] Get<T>(uint size) where T: struct
        {
            var key = _GetKey<T>(size);
            _requestHistory[key] = Interlocked.Increment(ref _requestIndex);

            if (_cache.TryGetValue(key, out var bag) && bag.TryTake(out var ptr))
                return (T[])ptr;

            var ret = new T[size];
            return ret;
        }

        public void Reuse<T>(T[] block) where T: struct
        {
            _cache.AddOrUpdate(
                _GetKey<T>((uint)block.Length),
                key => new ConcurrentBag<Array> { block },
                (key, bag) => {
                    bag.Add(block);
                    return bag;
                });

            // check if we should drop items from the cache
            if (CacheSize > MaxCacheSize)
            {
                lock (_cache)
                {
                    while (CacheSize > MaxCacheSize)
                    {
                        var lru = _cache
                            .OrderBy(d => _requestHistory[d.Key])
                            .FirstOrDefault();

                        if (lru.Value.TryTake(out var stale))
                            CacheSize -= stale.Length;
                        else
                            break;
                    }
                }
            }
        }

        public long MaxCacheSize { get; }

        public long CacheSize { get; private set; } = 0;

        static string _GetKey<T>(uint size)
        {
            return $"({size})" + typeof(T).AssemblyQualifiedName;
        }

        public void Dispose()
        {
        }
    }
}
