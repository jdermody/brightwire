using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BrightData.Helper;

namespace BrightData.Types
{
    /// <summary>
    /// Represents a set of vectors
    /// </summary>
    public class VectorSet
    {
        public enum StorageType
        {
            Flat
        }
        interface IVectorSetData
        {
            public uint VectorSize { get; }
            uint Add(IReadOnlyVector vector);
            void Remove(uint index);
            IEnumerable<uint> Rank(IReadOnlyVector vector, DistanceMetric distanceMetric);
            uint[] Closest(IReadOnlyVector[] vector, DistanceMetric distanceMetric);
            IReadOnlyVector Get(uint index);
            void Aggregate(SpanAggregator<float> aggregator, IEnumerable<uint> indices);
        }
        class FlatList : IVectorSetData
        {
            readonly List<IReadOnlyVector> _data = new();

            public FlatList(uint vectorSize) => VectorSize = vectorSize;
            public uint VectorSize { get; }

            public uint Add(IReadOnlyVector vector)
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

            public IEnumerable<uint> Rank(IReadOnlyVector vector, DistanceMetric distanceMetric)
            {
                var results = new ConcurrentDictionary<uint, float>();
                Parallel.ForEach(_data, (x, _, i) => results[(uint)i] = x.FindDistance(vector, distanceMetric));
                return results.OrderBy(x => x.Value).Select(x => x.Key);
            }

            public uint[] Closest(IReadOnlyVector[] vector, DistanceMetric distanceMetric)
            {
                var ret = new uint[_data.Count];
                var distance = new float[_data.Count];
                for (var i = 0; i < _data.Count; i++)
                    distance[i] = float.MaxValue;

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

            public IReadOnlyVector Get(uint index) => _data[(int)index];

            public void Aggregate(SpanAggregator<float> aggregator, IEnumerable<uint> indices)
            {
                foreach (var index in indices)
                {
                    var segment = _data[(int)index].ReadOnlySegment;
                    var contiguous = segment.Contiguous;
                    if (contiguous != null)
                        aggregator.Add(contiguous.ReadOnlySpan);
                    else
                        throw new NotImplementedException("Can only aggregate contiguous vectors");
                }
            }
        }

        readonly IVectorSetData _data;

        public VectorSet(uint vectorSize, StorageType type = StorageType.Flat)
        {
            VectorSize = vectorSize;
            if (type == StorageType.Flat)
                _data = new FlatList(vectorSize);
            else
                throw new NotSupportedException();
        }

        public uint VectorSize { get; }
        public uint Add(IReadOnlyVector vector) => _data.Add(vector);
        public uint[] Add(IReadOnlyList<IReadOnlyVector> vectors)
        {
            var ret = new uint[vectors.Count];
            for (var i = 0; i < ret.Length; i++)
                ret[i] = Add(vectors[i]);
            return ret;
        }
        public void Remove(uint index) => _data.Remove(index);
        public IReadOnlyVector Get(uint index) => _data.Get(index);
        public IEnumerable<uint> Rank(IReadOnlyVector vector, DistanceMetric distanceMetric = DistanceMetric.Euclidean) => _data.Rank(vector, distanceMetric);
        public uint[] Closest(IReadOnlyVector[] vector, DistanceMetric distanceMetric) => _data.Closest(vector, distanceMetric);
        public float[] GetAverage(IEnumerable<uint> keys)
        {
            using var aggregate = SpanAggregator<float>.GetOnlineAverage(VectorSize);
            _data.Aggregate(aggregate, keys);
            return aggregate.Span.ToArray();
        }
    }
}
