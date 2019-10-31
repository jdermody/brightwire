using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace BrightData.Memory
{
    class TensorPool : ITensorPool, IDisposable
    {
        readonly ITensorAllocator _allocator;
        readonly IBrightDataContext _context;
        readonly ConcurrentDictionary<string, ConcurrentBag<IReferenceCountedMemory>> _cache = new ConcurrentDictionary<string, ConcurrentBag<IReferenceCountedMemory>>();
        readonly HashSet<IReferenceCountedMemory> _allocated = new HashSet<IReferenceCountedMemory>();
        readonly ConcurrentDictionary<string, long> _requestHistory = new ConcurrentDictionary<string, long>();
        long requestIndex = 0;

        public TensorPool(IBrightDataContext context, ITensorAllocator allocator, long maxCacheSize)
        {
            MaxCacheSize = maxCacheSize;
            _allocator = allocator;
            _context = context;
        }

        public ITensorBlock<T> Get<T>(uint size)
        {
            var key = _GetKey<T>(size);
            _requestHistory[key] = Interlocked.Increment(ref requestIndex);

            if (_cache.TryGetValue(key, out var bag) && bag.TryTake(out var ptr))
                return (ITensorBlock<T>)ptr;

            var ret = _allocator.Create<T>(_context, size);
            lock (_allocated) {
                _allocated.Add(ret);
            }
            return ret;
        }

        public void Add<T>(ITensorBlock<T> block)
        {
            _cache.AddOrUpdate(
                _GetKey<T>(block.Size),
                key => new ConcurrentBag<IReferenceCountedMemory> { block },
                (key, bag) => {
                    bag.Add(block);
                    return bag;
                });

            while (CacheSize > MaxCacheSize) {
                var lru = _cache
                    .OrderBy(d => _requestHistory[d.Key])
                    .FirstOrDefault();
                if (lru.Value.TryTake(out var ptr)) {
                    lock (_allocated) {
                        if (_allocated.Remove(ptr)) {
                            var deallocator = (IMemoryDeallocator)ptr;
                            deallocator.Free();
                        }
                    }
                }
            }
        }

        public long MaxCacheSize { get; }

        public long AllocationSize
        {
            get
            {
                lock (_allocated) {
                    return _allocated.Sum(b => b.Size);
                }
            }
        }
        public long CacheSize => _cache.Sum(kv => kv.Value.Sum(b => b.Size));

        static string _GetKey<T>(uint size)
        {
            return $"({size})" + typeof(T).AssemblyQualifiedName;
        }

        public void Dispose()
        {
            lock (_allocated) {
                foreach (var item in _allocated) {
                    var deallocator = (IMemoryDeallocator)item;
                    deallocator.Free();
                }
            }
        }

        public void LogAllocations(Action<string> callback)
        {
            var cached = new HashSet<IReferenceCountedMemory>(_cache.SelectMany(b => b.Value));
            lock (_allocated) {
                foreach (var item in _allocated) {
                    if (!cached.Contains(item))
                        callback($"Allocation {item.AllocationIndex} was not released");
                }
            }
        }
    }
}
