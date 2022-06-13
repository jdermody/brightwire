using System;
using System.Collections.Concurrent;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.Memory
{
    /// <summary>
    /// Wraps memory owner array pool
    /// </summary>
    internal class TensorPool : ITensorPool, IDisposable
    {
#if DEBUG
        readonly ConcurrentDictionary<long, uint> _registeredBlocks = new();
        #endif

        public TensorPool()
        {
        }

        public MemoryOwner<T> Get<T>(uint size) where T : struct
        {
            return MemoryOwner<T>.Allocate((int)size);
        }

#if DEBUG
        public void Register(IReferenceCountedMemory block, uint size)
        {
            _registeredBlocks[block.AllocationIndex] = size;
        }

        public void UnRegister(IReferenceCountedMemory block)
        {
            _registeredBlocks.TryRemove(block.AllocationIndex, out var _);
        }
#endif

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
