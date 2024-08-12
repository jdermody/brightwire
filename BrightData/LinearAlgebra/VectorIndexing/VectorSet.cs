using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BrightData.Helper;
using BrightData.LinearAlgebra.VectorIndexing.IndexStrategy;
using BrightData.LinearAlgebra.VectorIndexing.Storage;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.VectorIndexing
{
    /// <summary>
    /// Represents a set of vectors
    /// </summary>
    public class VectorSet<T> : IHaveSize, IDisposable
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        readonly IVectorIndex<T> _index;

        private VectorSet(IVectorIndex<T> index) => _index = index;

        /// <summary>
        /// Creates a vector set with flat indexing (simplest approach for small data sets)
        /// </summary>
        /// <param name="vectorSize">Size of each vector in the set</param>
        /// <param name="distanceMetric">Distance metric for vector comparison</param>
        /// <param name="storageType">Storage type</param>
        /// <param name="capacity">The expected number of vectors (optional)</param>
        /// <exception cref="NotSupportedException"></exception>
        public static VectorSet<T> CreateFlat(uint vectorSize, DistanceMetric distanceMetric = DistanceMetric.Cosine, VectorStorageType storageType = VectorStorageType.InMemory, uint? capacity = null)
        {
            var storage = GetStorage(storageType, vectorSize, capacity);
            return new(new FlatVectorIndex<T>(storage, distanceMetric));
        }

        /// <summary>
        /// Creates a vector set with random projection indexing (reduced dimensionality)
        /// </summary>
        /// <param name="lap">Linear algebra provider</param>
        /// <param name="vectorSize">Input vector size</param>
        /// <param name="distanceMetric">Distance metric for vector comparison</param>
        /// <param name="projectionSize">Projection vector size</param>
        /// <param name="storageType">Vector storage type</param>
        /// <param name="capacity">The expected number of vectors (optional)</param>
        /// <param name="s">Density parameter (random projection)</param>
        /// <returns></returns>
        public static VectorSet<T> CreateRandomProjection(LinearAlgebraProvider<T> lap, uint vectorSize, uint projectionSize, DistanceMetric distanceMetric = DistanceMetric.Cosine, VectorStorageType storageType = VectorStorageType.InMemory, uint? capacity = null, int s = 3)
        {
            var storage = GetStorage(storageType, vectorSize, capacity);
            return new(new RandomProjectionVectorIndex<T>(lap, storage, projectionSize, distanceMetric, capacity, s));
        }

        /// <summary>
        /// Creates a vector index that uses a Hierarchical Navigation Small World Graph
        /// </summary>
        /// <param name="context">Bright data context</param>
        /// <param name="vectorSize">Size of each input vector</param>
        /// <param name="storageType">Vector storage type</param>
        /// <param name="capacity">The expected number of vectors (optional)</param>
        /// <param name="layers">Number of layers in the HNSW graph</param>
        /// <param name="distanceMetric">Distance metric</param>
        /// <returns></returns>
        public static VectorSet<T> CreateHNSW(BrightDataContext context, uint vectorSize, DistanceMetric distanceMetric = DistanceMetric.Cosine, VectorStorageType storageType = VectorStorageType.InMemory, uint? capacity = null, int layers = 5)
        {
            var storage = GetStorage(storageType, vectorSize, capacity);
            return new(new HNSWVectorIndex<T>(context, storage, layers, distanceMetric));
        }

        /// <summary>
        /// Creates a vector set that uses a KNN search provider
        /// </summary>
        /// <param name="creator"></param>
        /// <param name="vectorSize"></param>
        /// <param name="storageType"></param>
        /// <param name="capacity"></param>
        /// <returns></returns>
        public static VectorSet<T> CreateKnnSearch(uint vectorSize, Func<IReadOnlyVectorStore<T>, ISupportKnnSearch<T>> creator, VectorStorageType storageType = VectorStorageType.InMemory, uint? capacity = null)
        {
            var storage = GetStorage(storageType, vectorSize, capacity);
            return new(new KnnSearchVectorIndex<T>(storage, creator));
        }

        public static VectorSet<T> CreateFromPreCalculatedEmbedding(IMatrix<T> weight, IVector<T> bias, DistanceMetric distanceMetric = DistanceMetric.Cosine, VectorStorageType storageType = VectorStorageType.InMemory, uint? capacity = null)
        {
            var storage = GetStorage(storageType, weight.RowCount, capacity);
            return new(new PreCalculatedEmbeddingVectorIndex<T>(storage, weight, bias, distanceMetric, capacity));
        }

        /// <summary>
        /// Creates vector storage
        /// </summary>
        /// <param name="storageType">Storage type</param>
        /// <param name="vectorSize">Size of each input vector</param>
        /// <param name="capacity">The expected number of vectors (optional)</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static IStoreVectors<T> GetStorage(VectorStorageType storageType, uint vectorSize, uint? capacity) => storageType switch {
            VectorStorageType.InMemory => new InMemoryVectorStorage<T>(vectorSize, capacity),
            _ => throw new NotSupportedException()
        };

        /// <inheritdoc />
        public void Dispose()
        {
            _index.Dispose();
        }

        /// <summary>
        /// Size of each vector
        /// </summary>
        public uint VectorSize => _index.Storage.VectorSize;

        /// <summary>
        /// Vector storage
        /// </summary>
        public IStoreVectors<T> Storage => _index.Storage;

        /// <summary>
        /// Adds a vector to the set
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public uint Add(IReadOnlyVector<T> vector)
        {
            var temp = SpanOwner<T>.Empty;
            var wasTempUsed = false;
            try {
                var span = vector.GetSpan(ref temp, out wasTempUsed);
                return _index.Add(span);
            }
            finally {
                if (wasTempUsed)
                    temp.Dispose();
            }
        }

        /// <summary>
        /// Adds a segment to the set
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public uint Add(IReadOnlyNumericSegment<T> segment)
        {
            var temp = SpanOwner<T>.Empty;
            var wasTempUsed = false;
            try {
                var span = segment.GetSpan(ref temp, out wasTempUsed);
                return _index.Add(span);
            }
            finally {
                if (wasTempUsed)
                    temp.Dispose();
            }
        }

        /// <summary>
        /// Adds a vector span to the set
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public uint Add(ReadOnlySpan<T> span) => _index.Add(span);

        /// <summary>
        /// Adds a vector as memory to the set
        /// </summary>
        /// <param name="memory"></param>
        /// <returns></returns>
        public uint Add(ReadOnlyMemory<T> memory) => _index.Add(memory.Span);

        /// <summary>
        /// Adds a collection of vectors to the set
        /// </summary>
        /// <param name="vectors"></param>
        /// <returns></returns>
        public uint[] Add(IReadOnlyList<IReadOnlyVector<T>> vectors)
        {
            var ret = new uint[vectors.Count];
            for (var i = 0; i < ret.Length; i++)
                ret[i] = Add(vectors[i]);
            return ret;
        }

        /// <summary>
        /// Returns a specified vector
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ReadOnlySpan<T> Get(uint index) => _index.Storage[index];

        /// <summary>
        /// Returns a list of vector indices ranked by the distance between that vector and a comparison vector
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public IEnumerable<uint> Rank(IReadOnlyVector<T> vector)
        {
            var temp = SpanOwner<T>.Empty;
            var wasTempUsed = false;
            try {
                var span = vector.GetSpan(ref temp, out wasTempUsed);
                return _index.Rank(span);
            }
            finally {
                if (wasTempUsed)
                    temp.Dispose();
            }
        }

        /// <summary>
        /// Ranks the input vector and returns the closest vectors in the set
        /// </summary>
        /// <param name="span">Input vector</param>
        /// <returns></returns>
        public IEnumerable<uint> Rank(ReadOnlySpan<T> span) => _index.Rank(span);

        /// <summary>
        /// Returns the index of the closest vector in the set to each of the supplied vectors
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="distanceMetric"></param>
        /// <returns></returns>
        public uint[] Closest(IReadOnlyVector<T>[] vector)
        {
            var memoryArray = vector.Select(x => x.ReadOnlySegment.GetMemory()).ToArray();
            return _index.Closest(memoryArray);
        }

        /// <summary>
        /// Creates an average vector from the specified vectors
        /// </summary>
        /// <param name="vectorIndices">Indices of the vectors to combine</param>
        /// <returns></returns>
        public T[] GetAverage(IEnumerable<uint> vectorIndices)
        {
            using var aggregate = SpanAggregator<T>.GetOnlineAverage(VectorSize);
            foreach(var vectorIndex in vectorIndices)
                aggregate.Add(_index.Storage[vectorIndex]);
            return aggregate.Span.ToArray();
        }

        /// <inheritdoc />
        public uint Size => _index.Storage.Size;
    }
}
