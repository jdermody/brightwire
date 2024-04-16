using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using BrightData.Helper;

namespace BrightData.Types
{
    /// <summary>
    /// Represents a set of vectors
    /// </summary>
    public class VectorSet<T>
        where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        /// <summary>
        /// Represents how the vectors should be stored
        /// </summary>
        public enum StorageType
        {
            /// <summary>
            /// Flat storage
            /// </summary>
            Flat
        }
        interface IVectorSetData
        {
            public uint VectorSize { get; }
            uint Add(IReadOnlyVector<T> vector);
            void Remove(uint index);
            IEnumerable<uint> Rank(IReadOnlyVector<T> vector, DistanceMetric distanceMetric);
            uint[] Closest(IReadOnlyVector<T>[] vector, DistanceMetric distanceMetric);
            IReadOnlyVector<T> Get(uint index);
            void Aggregate(SpanAggregator<T> aggregator, IEnumerable<uint> indices);
        }
        class FlatList(uint vectorSize) : IVectorSetData
        {
            readonly List<IReadOnlyVector<T>> _data = [];

            public uint VectorSize { get; } = vectorSize;

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

            public IEnumerable<uint> Rank(IReadOnlyVector<T> vector, DistanceMetric distanceMetric)
            {
                var results = new ConcurrentDictionary<uint, T>();
                Parallel.ForEach(_data, (x, _, i) => results[(uint)i] = x.FindDistance(vector, distanceMetric));
                return results.OrderBy(x => x.Value).Select(x => x.Key);
            }

            public uint[] Closest(IReadOnlyVector<T>[] vector, DistanceMetric distanceMetric)
            {
                var ret = new uint[_data.Count];
                var distance = new T[_data.Count];
                for (var i = 0; i < _data.Count; i++)
                    distance[i] = T.MaxValue;

                var parallelOptions = new ParallelOptions {
                    MaxDegreeOfParallelism = Debugger.IsAttached ? 1 : -1
                };
                Parallel.For(0, _data.Count * vector.Length, parallelOptions, i => {
                    var dataIndex = i % _data.Count;
                    var vectorIndex = i / _data.Count;
                    var d = _data[dataIndex].FindDistance(vector[vectorIndex], distanceMetric);
                    if (d < distance[dataIndex]) {
                        distance[dataIndex] = d;
                        ret[dataIndex] = (uint)vectorIndex;
                    }
                });
                return ret;
            }

            public IReadOnlyVector<T> Get(uint index) => _data[(int)index];

            public void Aggregate(SpanAggregator<T> aggregator, IEnumerable<uint> indices)
            {
                foreach (var index in indices)
                {
                    var segment = _data[(int)index].ReadOnlySegment;
                    var contiguous = segment.Contiguous;
                    aggregator.Add(contiguous != null ? contiguous.ReadOnlySpan : segment.ToNewArray());
                }
            }
        }

        readonly IVectorSetData _data;

        /// <summary>
        /// Creates a vector set
        /// </summary>
        /// <param name="vectorSize"></param>
        /// <param name="type"></param>
        /// <exception cref="NotSupportedException"></exception>
        public VectorSet(uint vectorSize, StorageType type = StorageType.Flat)
        {
            VectorSize = vectorSize;
            if (type == StorageType.Flat)
                _data = new FlatList(vectorSize);
            else
                throw new NotSupportedException();
        }

        /// <summary>
        /// Size of each vector
        /// </summary>
        public uint VectorSize { get; }

        /// <summary>
        /// Adds a vector to the set
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public uint Add(IReadOnlyVector<T> vector) => _data.Add(vector);

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
        public void Remove(uint index) => _data.Remove(index);

        /// <summary>
        /// Returns a specified vector
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IReadOnlyVector<T> Get(uint index) => _data.Get(index);

        /// <summary>
        /// Returns the ranking (based on distance) between a vector and every vector in this set
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="distanceMetric"></param>
        /// <returns></returns>
        public IEnumerable<uint> Rank(IReadOnlyVector<T> vector, DistanceMetric distanceMetric = DistanceMetric.Euclidean) => _data.Rank(vector, distanceMetric);

        /// <summary>
        /// Returns the index of the closest vector in the set to each of the supplied vectors
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="distanceMetric"></param>
        /// <returns></returns>
        public uint[] Closest(IReadOnlyVector<T>[] vector, DistanceMetric distanceMetric) => _data.Closest(vector, distanceMetric);

        /// <summary>
        /// Creates an average vector from the specified vectors
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public T[] GetAverage(IEnumerable<uint> keys)
        {
            using var aggregate = SpanAggregator<T>.GetOnlineAverage(VectorSize);
            _data.Aggregate(aggregate, keys);
            return aggregate.Span.ToArray();
        }
    }
}
