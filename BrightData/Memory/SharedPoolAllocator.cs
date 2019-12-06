using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace BrightData.Memory
{
    class SharedPoolAllocator : ITensorAllocator
    {
        long _allocationIndex;

        public ITensorBlock<T> Create<T>(IBrightDataContext context, uint size) where T: struct
        {
            var data = MemoryPool<T>.Shared.Rent((int) size);
            return new TensorBlock<T>(context, data, size, Interlocked.Increment(ref _allocationIndex));
        }
    }
}
