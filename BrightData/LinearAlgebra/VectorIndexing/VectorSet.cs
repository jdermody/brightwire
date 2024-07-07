using System;
using System.Collections.Generic;
using System.Numerics;
using BrightData.Helper;
using BrightData.LinearAlgebra.VectorIndexing.IndexStrategy;
using BrightData.LinearAlgebra.VectorIndexing.Storage;

namespace BrightData.LinearAlgebra.VectorIndexing
{
    /// <summary>
    /// Represents a set of vectors
    /// </summary>
    public class VectorSet<T>
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        readonly IVectorIndex<T> _index;

        /// <summary>
        /// Creates a vector set
        /// </summary>
        /// <param name="vectorSize">Size of each vector in the set</param>
        /// <param name="indexType">Indexing strategy</param>
        /// <exception cref="NotSupportedException"></exception>
        public VectorSet(uint vectorSize, VectorIndexStrategy indexType = VectorIndexStrategy.Flat)
        {
            var storage = new InMemoryVectorStorage<T>(vectorSize);
            if (indexType == VectorIndexStrategy.Flat)
                _index = new FlatVectorIndex<T>(storage);
            else
                throw new NotSupportedException();
        }

        /// <summary>
        /// Size of each vector
        /// </summary>
        public uint VectorSize => _index.Storage.VectorSize;

        /// <summary>
        /// Adds a vector to the set
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public uint Add(IReadOnlyVector<T> vector) => _index.Add(vector);

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
        /// Removes a vector
        /// </summary>
        /// <param name="index"></param>
        public void Remove(uint index) => _index.Remove(index);

        /// <summary>
        /// Returns a specified vector
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IReadOnlyNumericSegment<T> Get(uint index) => _index.Storage[index];

        /// <summary>
        /// Returns a list of vector indices ranked by the distance between that vector and a comparison vector
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="distanceMetric"></param>
        /// <returns></returns>
        public IEnumerable<uint> Rank(IReadOnlyVector<T> vector, DistanceMetric distanceMetric = DistanceMetric.Euclidean) => _index.Rank(vector, distanceMetric);

        /// <summary>
        /// Returns the index of the closest vector in the set to each of the supplied vectors
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="distanceMetric"></param>
        /// <returns></returns>
        public uint[] Closest(IReadOnlyVector<T>[] vector, DistanceMetric distanceMetric) => _index.Closest(vector, distanceMetric);

        /// <summary>
        /// Creates an average vector from the specified vectors
        /// </summary>
        /// <param name="vectorIndices">Indices of the vectors to combine</param>
        /// <returns></returns>
        public T[] GetAverage(IEnumerable<uint> vectorIndices)
        {
            using var aggregate = SpanAggregator<T>.GetOnlineAverage(VectorSize);
            foreach(var vectorIndex in vectorIndices) {
                var segment = _index.Storage[vectorIndex];
                var contiguous = segment.Contiguous;
                aggregate.Add(contiguous != null ? contiguous.ReadOnlySpan : segment.ToNewArray());
            }
            return aggregate.Span.ToArray();
        }
    }
}
