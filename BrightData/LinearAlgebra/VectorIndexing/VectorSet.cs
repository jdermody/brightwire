﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics;
using BrightData.Helper;
using BrightData.LinearAlgebra.VectorIndexing.IndexStrategy;
using BrightData.LinearAlgebra.VectorIndexing.Storage;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.VectorIndexing
{
    /// <summary>
    /// Represents a set of vectors
    /// </summary>
    public class VectorSet<T> : IHaveSize
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        readonly IVectorIndex<T> _index;

        /// <summary>
        /// Creates a vector set
        /// </summary>
        /// <param name="vectorSize">Size of each vector in the set</param>
        /// <param name="indexType">Indexing strategy</param>
        /// <param name="storageType">Storage type</param>
        /// <param name="capacity">The expected number of vectors (optional)</param>
        /// <exception cref="NotSupportedException"></exception>
        public VectorSet(uint vectorSize, VectorIndexStrategy indexType = VectorIndexStrategy.Flat, VectorStorageType storageType = VectorStorageType.InMemory, uint? capacity = null)
        {
            var storage = new InMemoryVectorStorage<T>(vectorSize, capacity);
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
        /// <param name="distanceMetric"></param>
        /// <returns></returns>
        public IEnumerable<uint> Rank(IReadOnlyVector<T> vector, DistanceMetric distanceMetric = DistanceMetric.Euclidean)
        {
            var temp = SpanOwner<T>.Empty;
            var wasTempUsed = false;
            try {
                var span = vector.GetSpan(ref temp, out wasTempUsed);
                return _index.Rank(span, distanceMetric);
            }
            finally {
                if (wasTempUsed)
                    temp.Dispose();
            }
        }

        public IEnumerable<uint> Rank(ReadOnlySpan<T> span, DistanceMetric distanceMetric = DistanceMetric.Euclidean) => _index.Rank(span, distanceMetric);

        /// <summary>
        /// Returns the index of the closest vector in the set to each of the supplied vectors
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="distanceMetric"></param>
        /// <returns></returns>
        public uint[] Closest(IReadOnlyVector<T>[] vector, DistanceMetric distanceMetric)
        {
            var memoryArray = vector.Select(x => x.ReadOnlySegment.GetMemory()).ToArray();
            return _index.Closest(memoryArray, distanceMetric);
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
