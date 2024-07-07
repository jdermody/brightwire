using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace BrightData.LinearAlgebra.VectorIndexing.Storage
{
    internal class InMemoryVectorStorage<T>(uint vectorSize) : IStoreVectors<T>
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        readonly List<IReadOnlyVector<T>> _data = [];

        public VectorStorageType StorageType => VectorStorageType.InMemory;
        public uint VectorSize { get; } = vectorSize;
        public uint Size => (uint)_data.Count;
        public IReadOnlyNumericSegment<T> this[uint index] => _data[(int)index].ReadOnlySegment;

        public uint Add(IReadOnlyVector<T> vector)
        {
            if (vector.Size != VectorSize)
                throw new ArgumentException($"Expected vector to be size {VectorSize} but received {vector.Size}", nameof(vector));
            var index = (uint)_data.Count;
            _data.Add(vector);
            return index;
        }

        public void Remove(uint index)
        {
            _data.RemoveAt((int)index);
        }

        public void ForEach(Action<IReadOnlyVector<T>, uint> callback)
        {
            Parallel.ForEach(_data, (x, _, i) => callback(x, (uint)i));
        }
    }
}
