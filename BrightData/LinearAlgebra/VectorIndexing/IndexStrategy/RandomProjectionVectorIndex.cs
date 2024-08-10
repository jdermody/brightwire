using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BrightData.LinearAlgebra.VectorIndexing.Storage;

namespace BrightData.LinearAlgebra.VectorIndexing.IndexStrategy
{
    internal class RandomProjectionVectorIndex<T> : IVectorIndex<T>
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        readonly LinearAlgebraProvider<T> _lap;
        readonly IMatrix<T> _randomProjection;
        readonly FlatVectorIndex<T> _projectionIndex;

        public RandomProjectionVectorIndex(LinearAlgebraProvider<T> lap, IStoreVectors<T> storage, uint projectionSize, DistanceMetric distanceMetric, uint? capacity, int s)
        {
            _lap = lap;
            Storage = storage;
            _projectionIndex = new(new InMemoryVectorStorage<T>(projectionSize, capacity), distanceMetric);

            var c1 = MathF.Sqrt(3);
            var distribution = lap.Context.CreateCategoricalDistribution([1.0f / (2f * s), 1f - (1.0f / s), 1.0f / (2f * s)]);
            _randomProjection = lap.CreateMatrix(projectionSize, storage.VectorSize, (_, _) => T.CreateSaturating((distribution.Sample() - 1f) * c1));
        }

        public void Dispose()
        {
            Storage.Dispose();
        }

        public IStoreVectors<T> Storage { get; }

        public uint Add(ReadOnlySpan<T> vector)
        {
            using var vector2 = _lap.CreateVector(vector);
            using var projection = _randomProjection.Multiply(vector2);
            _projectionIndex.Add(projection.ReadOnlySegment.Contiguous!.ReadOnlySpan);
            return Storage.Add(vector);
        }

        public IEnumerable<uint> Rank(ReadOnlySpan<T> vector)
        {
            using var vector2 = _lap.CreateVector(vector);
            using var projection = _randomProjection.Multiply(vector2);
            return _projectionIndex.Rank(projection.ReadOnlySegment.Contiguous!.ReadOnlySpan);
        }

        public uint[] Closest(ReadOnlyMemory<T>[] vector)
        {
            return _projectionIndex.Closest(Project(vector));
        }

        ReadOnlyMemory<T>[] Project(ReadOnlyMemory<T>[] vectors) =>
            vectors.Select(x => {
                using var vector2 = _lap.CreateVector(x);
                using var projection = _randomProjection.Multiply(vector2);
                return new ReadOnlyMemory<T>(projection.ReadOnlySegment.ToNewArray());
            }).ToArray()
        ;
    }
}
