using System;
using System.Buffers;
using System.Numerics;
using System.Threading.Tasks;

namespace BrightData.LinearAlgebra.VectorIndexing.Storage
{
    internal class InMemoryVectorStorage<T> : IStoreVectors<T>
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        readonly ArrayBufferWriter<T> _data;

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

        public uint Add(ReadOnlySpan<T> vector)
        {
            if (vector.Length != VectorSize)
                throw new ArgumentException($"Expected vector to be size {VectorSize} but received {vector.Length}", nameof(vector));
            var index = Size;
            var span = _data.GetSpan((int)VectorSize);
            vector.CopyTo(span);
            _data.Advance((int)VectorSize);
            return index;
        }

        public void ForEach(IndexedSpanCallback<T> callback)
        {
            Parallel.For(0, Size, i => callback(this[(uint)i], (uint)i));
        }
    }
}
