using BrightData.LinearAlgebra.VectorIndexing.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.LinearAlgebra.VectorIndexing.IndexStrategy
{
    internal class PreCalculatedEmbeddingVectorIndex<T> : IVectorIndex<T>
        where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        readonly LinearAlgebraProvider<T> _lap;
        readonly IMatrix<T> _weights;
        readonly IVector<T> _bias;
        readonly FlatVectorIndex<T> _embeddingIndex;

        public PreCalculatedEmbeddingVectorIndex(IStoreVectors<T> storage, IMatrix<T> weights, IVector<T> bias, DistanceMetric distanceMetric, uint? capacity)
        {
            _lap = weights.LinearAlgebraProvider;
            _weights = weights;
            _bias = bias;
            Storage = storage;
            _embeddingIndex = new(new InMemoryVectorStorage<T>(weights.ColumnCount, capacity), distanceMetric);
        }

        public void Dispose()
        {
            _weights.Dispose();
            _bias.Dispose();
        }

        public IStoreVectors<T> Storage { get; }

        public uint Add(ReadOnlySpan<T> vector)
        {
            using var vector2 = _weights.LinearAlgebraProvider.CreateVector(vector);
            using var matrix = vector2.Reshape(_weights.RowCount, 1);
            using var projection = _weights.TransposeThisAndMultiply(matrix);
            projection.AddInPlace(_bias);
            _embeddingIndex.Add(projection.ReadOnlySegment.Contiguous!.ReadOnlySpan);
            return Storage.Add(vector);
        }

        public IEnumerable<uint> Rank(ReadOnlySpan<T> vector)
        {
            using var vector2 = _weights.LinearAlgebraProvider.CreateVector(vector);
            using var matrix = vector2.Reshape(_weights.RowCount, 1);
            using var projection = _weights.TransposeThisAndMultiply(matrix);
            projection.AddInPlace(_bias);
            return _embeddingIndex.Rank(projection.ReadOnlySegment.Contiguous!.ReadOnlySpan);
        }

        public uint[] Closest(ReadOnlyMemory<T>[] vector)
        {
            throw new NotImplementedException();
        }
    }
}
