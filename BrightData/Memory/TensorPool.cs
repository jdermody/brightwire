using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.Memory
{
    /// <summary>
    /// Tensor memory pool
    /// </summary>
    internal class TensorPool : ITensorPool, IDisposable
    {
        //readonly Dictionary<string, Stack<Array>> _cache = new();
        //readonly Dictionary<string, long> _requestHistory = new();
        //long _requestIndex = 0;
        #if DEBUG
        readonly ConcurrentDictionary<long, uint> _registeredBlocks = new();
        #endif

        public TensorPool(/*long maxCacheSize*/)
        {
            //MaxCacheSize = maxCacheSize;
        }

        public MemoryOwner<T> Get<T>(uint size) where T : struct
        {
            return MemoryOwner<T>.Allocate((int)size);
            //var key = GetKey<T>(size);
            //lock (_cache) {
            //    _requestHistory[key] = _requestIndex++;
            //    if (_cache.TryGetValue(key, out var stack) && stack.TryPop(out var ptr))
            //        return (T[])ptr;
            //}

            //var ret = new T[size];
            //return ret;
        }

#if DEBUG
        public void Register(IReferenceCountedMemory block, uint size)
        {
            _registeredBlocks.AddOrUpdate(block.AllocationIndex, 
                _ => size, 
                (_, _) => throw new Exception("Unexpected")
            );
        }

        public void Unregister(IReferenceCountedMemory block)
        {
            _registeredBlocks.TryRemove(block.AllocationIndex, out var _);
        }
#endif

        //public void Reuse<T>(T[] block) where T: struct
        //{
        //    var key = GetKey<T>((uint)block.Length);
        //    lock (_cache) {
        //        if (!_cache.TryGetValue(key, out var ret))
        //            _cache.Add(key, ret = new Stack<Array>());
        //        ret.Push(block);
        //        CacheSize += block.Length;

        //        // check if we should drop items from the cache
        //        while (CacheSize > MaxCacheSize) {
        //            var lru = _cache
        //                .OrderBy(d => _requestHistory[d.Key])
        //                .FirstOrDefault();

        //            if (lru.Value.TryPop(out var stale))
        //                CacheSize -= stale.Length;
        //            else
        //                break;
        //        }
        //    }
        //}

        //public long MaxCacheSize { get; }

        //public long CacheSize { get; private set; } = 0;

        //static string GetKey<T>(uint size) => $"({size})" + typeof(T).AssemblyQualifiedName;

        public void Dispose()
        {
#if DEBUG
            foreach (var item in _registeredBlocks) {
                Console.WriteLine($"Block {item.Key} ({item.Value}) was not released");
            }
#endif
        }
    }
}
