using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace BrightData.Memory
{
    /// <summary>
    /// Block of contiguous tensor memory
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class TensorBlock<T> : ITensorBlock<T>, IMemoryDeallocator
        where T: struct
    {
        readonly IMemoryOwner<T> _data;
        int _refCount = 0;

        public TensorBlock(IBrightDataContext context, IMemoryOwner<T> data, uint size, long allocationIndex)
        {
            _data = data;
            Context = context;
            Size = size;
            AllocationIndex = allocationIndex;
        }

        public int AddRef() => Interlocked.Increment(ref _refCount);

        public int Release()
        {
            var ret = Interlocked.Decrement(ref _refCount);
            if (ret == 0)
                Context.TensorPool.Add(this);
            return ret;
        }

        public long AllocationIndex { get; }
        public bool IsValid { get; private set; } = true;
        public uint Size { get; }

        public string TensorType => "Standard";
        public ITensorSegment<T> GetSegment() => new TensorSegment<T>(this);
        public IBrightDataContext Context { get; }

        public T[] ToArray() => _data.Memory.Span.Slice(0, (int)Size).ToArray();
        public Span<T> Data => _data.Memory.Span;
        public IEnumerable<T> Values
        {
            get {
                var memory = _data.Memory;
                for (int i = 0; i < Size; i++)
                    yield return memory.Span[i];

            }
        }

        public T this[uint index] {
            get => _data.Memory.Span[(int)index];
            set {
                var span = _data.Memory.Span;
                span[(int) index] = value;
            }
        }

        public T this[long index] {
            get => _data.Memory.Span[(int)index];
            set {
                var span = _data.Memory.Span;
                span[(int) index] = value;
            }
        }

        public void WriteTo(Stream stream)
        {
            stream.Write(MemoryMarshal.Cast<T, byte>(_data.Memory.Span));
        }

        public void InitializeFrom(Stream stream)
        {
            stream.Read(MemoryMarshal.Cast<T, byte>(_data.Memory.Span));
        }

        public void InitializeFrom(ITensorBlock<T> tensor)
        {
            tensor.Data.CopyTo(Data);
        }

        public void InitializeFrom(T[] array)
        {
            var ptr = new Span<T>(array);
            ptr.CopyTo(Data);
        }

        public void Initialize(Func<uint, T> initializer)
        {
            var span = _data.Memory.Span;
            for (uint i = 0; i < Size; i++)
                span[(int) i] = initializer(i);
        }

        public void Initialize(T initializer)
        {
            var span = _data.Memory.Span;
            for (uint i = 0; i < Size; i++)
                span[(int) i] = initializer;
        }

        public void Free()
        {
            _data.Dispose();
            IsValid = false;
        }
    }
}
