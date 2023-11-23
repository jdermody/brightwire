using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
            IEnumerable<uint> Rank(IReadOnlyVector vector, DistanceMetric distanceMetric);
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

            public IEnumerable<uint> Rank(IReadOnlyVector vector, DistanceMetric distanceMetric)
            {
                var results = new ConcurrentDictionary<uint, float>();
                Parallel.ForEach(_data, (x, _, i) => results[(uint)i] = x.FindDistance(vector, distanceMetric));
                return results.OrderByDescending(x => x.Value).Select(x => x.Key);
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
        public IEnumerable<uint> Rank(IReadOnlyVector vector, DistanceMetric distanceMetric = DistanceMetric.Euclidean) => _data.Rank(vector, distanceMetric);
        public ReadOnlySpan<float> GetAverage(IEnumerable<uint> keys)
        {
            var aggregate = SpanAggregator<float>.GetOnlineAverage(VectorSize);
            _data.Aggregate(aggregate, keys);
            return aggregate.Span;
        }
    }
}
