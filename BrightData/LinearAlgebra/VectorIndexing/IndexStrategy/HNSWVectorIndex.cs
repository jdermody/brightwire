using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using BrightData.Helper.Vectors;
using BrightData.LinearAlgebra.VectorIndexing.Storage;
using BrightData.Types;
using BrightData.Types.Graph;

namespace BrightData.LinearAlgebra.VectorIndexing.IndexStrategy
{
    internal class HNSWVectorIndex<T>(BrightDataContext context, IStoreVectors<T> storage, int numLayers = 5, DistanceMetric distanceMetric = DistanceMetric.Cosine) : IVectorIndex<T>
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        readonly HierarchicalNavigationSmallWorldGraph<GraphNodeIndex, T, FixedSizeSortedAscending16Array<uint, T>, FixedSizeSortedAscending32Array<uint, T>> _graph = new(context, numLayers);
        readonly VectorDistanceCache<T> _distanceCache = new(storage, distanceMetric);

        public void Dispose()
        {
            // nop
        }

        public IStoreVectors<T> Storage { get; } = storage;

        public uint Add(ReadOnlySpan<T> vector)
        {
            var index = Storage.Add(vector);
            _graph.Add(_distanceCache, new GraphNodeIndex(index));
            return index;
        }

        public IEnumerable<uint> Rank(ReadOnlySpan<T> vector)
        {
            using var inMemoryStorage = new InMemoryVectorStorage<T>(_distanceCache.VectorSize, 1);
            var distanceCache = new VectorDistanceCache<T>(inMemoryStorage, distanceMetric, _distanceCache);
            var q = distanceCache.Add(vector);
            var w = _graph.KnnSearch(q, distanceCache);
            var ret = new FixedSizeSortedAscending32Array<uint, T>();

            for (var i = 0U; i < w.Size; i++) {
                var (index, weight) = w[i];
                ret.TryAdd(index, weight);
                foreach (var (index2, weight2) in _graph.BreadthFirstSearch(index).Take(4))
                    ret.TryAdd(index2, weight2);
            }
            return ret.Values.ToArray();
        }

        public uint[] Closest(ReadOnlyMemory<T>[] vector)
        {
            var ret = new uint[vector.Length];
            var vectorIndices = new uint[vector.Length];
            using var inMemoryStorage = new InMemoryVectorStorage<T>(_distanceCache.VectorSize, (uint)vector.Length);
            var distanceCache = new VectorDistanceCache<T>(inMemoryStorage, distanceMetric, _distanceCache);
            var index = 0U;
            foreach (var item in vector)
                vectorIndices[index++] = distanceCache.Add(item.Span);
            Parallel.For(0, ret.Length, i => {
                var w = _graph.KnnSearch(vectorIndices[i], distanceCache);
                ret[i] = w[0].Value;
            });
            return ret;
        }
    }
}
