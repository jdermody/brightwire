using System;
using System.Diagnostics;
using System.Threading;

namespace BrightData.Memory
{
    /// <summary>
    /// Reference counted memory block
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ReferenceCountedBlock<T> : IReferenceCountedMemory, IHaveDataContext
        where T: struct
    {
        // ReSharper disable once StaticMemberInGenericType
        static long _allocationIndex = 0;
#if DEBUG
        public static int _badAlloc = -1;
#endif

        protected readonly T[] _data;
        int _refCount = 0;

        protected ReferenceCountedBlock(IBrightDataContext context, T[] data)
        {
            _data = data;
            Size = (uint) _data.Length;
            AllocationIndex = Interlocked.Increment(ref _allocationIndex);
            Context = context;
#if DEBUG
            if (AllocationIndex == _badAlloc)
                Debugger.Break();
            Context.TensorPool.Register(this, Size);
#endif
        }

        public uint Size { get; }

        public int AddRef()
        {
#if DEBUG
            if (!IsValid)
                Debugger.Break();
#endif
            return Interlocked.Increment(ref _refCount);
        } 
        public IBrightDataContext Context { get; }
        public int Release()
        {
            var ret = Interlocked.Decrement(ref _refCount);
            if (ret == 0) {
#if DEBUG
                Context.TensorPool.Unregister(this);
#endif
                Context.TensorPool.Reuse(_data);
                IsValid = false;
            }

            return ret;
        }

        public long AllocationIndex { get; }
        public bool IsValid { get; private set; } = true;
    }
}
