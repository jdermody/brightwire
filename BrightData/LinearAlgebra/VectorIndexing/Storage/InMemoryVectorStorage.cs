using System;
using System.Buffers;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace BrightData.LinearAlgebra.VectorIndexing.Storage
{
    internal class InMemoryVectorStorage<T> : IStoreVectors<T>
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        readonly ArrayBufferWriter<T> _data;
        readonly Lock _writeLock = new();

        public InMemoryVectorStorage(uint vectorSize, uint? capacity)
        {
            VectorSize = vectorSize;
            if (capacity.HasValue)
                _data = new((int)(capacity.Value * vectorSize));
            else
                _data = new();
        }

        public void Dispose()
        {
            // nop
        }

        public VectorStorageType StorageType => VectorStorageType.InMemory;
        public uint VectorSize { get; }
        public uint Size => (uint)(_data.WrittenCount / VectorSize);
        public ReadOnlySpan<T> this[uint index] => _data.WrittenSpan.Slice((int)(index * VectorSize), (int)VectorSize);
        public ReadOnlyMemory<T> Get(uint index) => _data.WrittenMemory.Slice((int)(index * VectorSize), (int)VectorSize);

        public uint Add(ReadOnlySpan<T> vector)
        {
            if (vector.Length != VectorSize)
                throw new ArgumentException($"Expected vector to be size {VectorSize} but received {vector.Length}", nameof(vector));

            lock (_writeLock) {
                var index = Size;
                var span = _data.GetSpan((int)VectorSize);
                vector.CopyTo(span);
                _data.Advance((int)VectorSize);
                return index;
            }
        }

        public void ForEach(IndexedSpanCallbackWithVectorIndex<T> callback, CancellationToken ct)
        {
            if (Size < Consts.MinimumSizeForParallel) {
                for (uint i = 0U, size = Size; i < size && !ct.IsCancellationRequested; i++)
                    callback(this[i], i);
            }
            else {
                Parallel.For(0, Size, new ParallelOptions {
                    CancellationToken = ct
                }, i => callback(this[(uint)i], (uint)i));
            }
        }

        public unsafe void ForEach(ReadOnlySpan<uint> indices, IndexedSpanCallbackWithVectorIndexAndRelativeIndex<T> callback)
        {
            var len = indices.Length;
            if (len < Consts.MinimumSizeForParallel) {
                for (var i = 0U; i < len; i++) {
                    var vectorIndex = indices[(int)i];
                    callback(this[vectorIndex], vectorIndex, i);
                }
            }
            else {
                fixed (uint* indexPtr = indices) {
                    var index = indexPtr;
                    Parallel.For(0, len, i => {
                        var vectorIndex = index[i];
                        callback(this[vectorIndex], vectorIndex, (uint)i);
                    });
                }
            }
        }

        public ReadOnlyMemory<T>[] GetAll()
        {
            var size = Size;
            var ret = new ReadOnlyMemory<T>[size];
            for(var i = 0U; i < size; i++)
                ret[i] = Get(i);
            return ret;
        }
    }
}
