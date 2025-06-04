using BrightData.LinearAlgebra.VectorIndexing.Storage;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace BrightData.LinearAlgebra.VectorIndexing.IndexStrategy
{
    internal class PreCalculatedEmbeddingVectorIndex<T>(IStoreVectors<T> storage, IMatrix<T> weights, IVector<T> bias, DistanceMetric distanceMetric, uint? capacity)
        : IVectorIndex<T>
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        readonly FlatVectorIndex<T> _embeddingIndex = new(new InMemoryVectorStorage<T>(weights.ColumnCount, capacity), distanceMetric);

        public void Dispose()
        {
            weights.Dispose();
            bias.Dispose();
        }

        public IStoreVectors<T> Storage { get; } = storage;

        public uint Add(ReadOnlySpan<T> vector)
        {
            using var vector2 = weights.LinearAlgebraProvider.CreateVector(vector);
            using var matrix = vector2.Reshape(weights.RowCount, 1);
            using var projection = weights.TransposeThisAndMultiply(matrix);
            projection.AddInPlace(bias);
            _embeddingIndex.Add(projection.ReadOnlySegment.Contiguous!.ReadOnlySpan);
            return Storage.Add(vector);
        }

        public IEnumerable<uint> Rank(ReadOnlySpan<T> vector)
        {
            using var vector2 = weights.LinearAlgebraProvider.CreateVector(vector);
            using var matrix = vector2.Reshape(weights.RowCount, 1);
            using var projection = weights.TransposeThisAndMultiply(matrix);
            projection.AddInPlace(bias);
            return _embeddingIndex.Rank(projection.ReadOnlySegment.Contiguous!.ReadOnlySpan);
        }

        public uint[] Closest(ReadOnlyMemory<T>[] vector)
        {
            throw new NotImplementedException();
        }
    }
}
