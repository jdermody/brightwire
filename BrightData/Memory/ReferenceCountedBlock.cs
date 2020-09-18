using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace BrightData.Memory
{
    class ReferenceCountedBlock<T> : IReferenceCountedMemory
        where T: struct
    {
        // ReSharper disable once StaticMemberInGenericType
        static long _allocationIndex = 0;

        protected readonly T[] _data;
        private int _refCount = 0;

        protected ReferenceCountedBlock(IBrightDataContext context, T[] data)
        {
            _data = data;
            Size = (uint) _data.Length;
            AllocationIndex = Interlocked.Increment(ref _allocationIndex);
            Context = context;
        }

        public uint Size { get; }
        public int AddRef() => Interlocked.Increment(ref _refCount);
        public IBrightDataContext Context { get; }
        public int Release()
        {
            var ret = Interlocked.Decrement(ref _refCount);
            if (ret == 0) {
                Context.TensorPool.Reuse(_data);
                IsValid = false;
            }

            return ret;
        }

        public long AllocationIndex { get; }
        public bool IsValid { get; private set; } = true;
    }
}
