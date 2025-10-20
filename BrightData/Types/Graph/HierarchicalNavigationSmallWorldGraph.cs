using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace BrightData.Types.Graph
{
    /// <summary>
    /// A graph that uses multiple sub graphs as a skip list
    /// </summary>
    /// <param name="context"></param>
    /// <param name="maxLayers"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="W"></typeparam>
    /// <typeparam name="AT"></typeparam>
    /// <typeparam name="BLAT"></typeparam>
    public class HierarchicalNavigationSmallWorldGraph<T, W, AT, BLAT>(BrightDataContext context, int maxLayers)
        where T : unmanaged, IHaveSingleIndex
        where W : unmanaged, INumber<W>, IMinMaxValue<W>, IBinaryFloatingPointIeee754<W>
        where AT : unmanaged, IFixedSizeSortedArray<uint, W>
        where BLAT : unmanaged, IFixedSizeSortedArray<uint, W>
    {
        readonly record struct NodeIndex(T Value, uint LayerIndex) : IHaveSingleIndex
        {
            public uint Index => Value.Index;
        }
        readonly IContinuousDistribution<W> _distribution = context.CreateExponentialDistribution<W>(W.CreateSaturating(maxLayers));
        readonly IWeightedGraph<NodeIndex, W>[] _layers = Enumerable.Range(0, maxLayers)
            .Select(i => (IWeightedGraph<NodeIndex, W>)(i == 0 ? new FixedSizeWeightedDynamicGraph<NodeIndex, W, BLAT>() : new FixedSizeWeightedDynamicGraph<NodeIndex, W, AT>()))
            .ToArray()
        ;
        NodeIndex? _entryPoint = null;

        /// <summary>
        /// Adds new nodes to the graph
        /// </summary>
        /// <param name="distanceCalculator"></param>
        /// <param name="values">New nodes</param>
        [SkipLocalsInit]
        public void Add(ICalculateNodeWeights<W> distanceCalculator, params T[] values)
        {
            Span<(uint, W)> newNodeNeighbours = stackalloc (uint, W)[32];

            foreach (var value in values) {
                var entryPoint = _entryPoint;
                var level = GetRandomLevel();

                // zoom in
                uint? entryPointLevel = null;
                if (entryPoint is not null) {
                    entryPointLevel = entryPoint.Value.LayerIndex;
                    for (var i = entryPointLevel.Value; i > level; i--) {
                        var layer = _layers[i];
                        var w = layer.ProbabilisticSearch<FixedSizeSortedAscending1Array<uint, W>, AT>(value.Index, entryPoint.Value.Index, distanceCalculator);
                        entryPoint = layer.Get(w.MinValue);
                    }
                }

                // add to levels
                var from = Math.Min(level, entryPoint?.LayerIndex ?? int.MaxValue);
                for(var i = level; i > from; i--)
                    _layers[i].Add(new NodeIndex(value, (uint)i));
                for (var i = from; i >= 0; i--) {
                    var layer = _layers[i];
                    var newNodeIndex = 0;
                    if (entryPoint is not null) {
                        var w = layer.ProbabilisticSearch<AT, BLAT>(value.Index, entryPoint.Value.Index, distanceCalculator);
                        foreach (var (ni, nw) in w.Elements) {
                            layer.AddNeighbour(ni, value.Index, nw);
                            newNodeNeighbours[newNodeIndex++] = (ni, nw);
                        }
                    }
                    layer.Add(new NodeIndex(value, (uint)i), newNodeNeighbours[..newNodeIndex]);
                }

                if(!entryPointLevel.HasValue || level > entryPointLevel.Value)
                    _entryPoint = _layers[level].Get(value.Index);
            }
        }

        /// <summary>
        /// K nearest neighbour search (probabilistic)
        /// </summary>
        /// <param name="q">Node to query</param>
        /// <param name="distanceCalculator"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public AT KnnSearch(uint q, ICalculateNodeWeights<W> distanceCalculator)
        {
            var entryPoint = _entryPoint ?? throw new InvalidOperationException("No nodes in graph");
            for (var i = (int)entryPoint.LayerIndex; i > 0; i--) {
                var layer = _layers[i];
                var w = layer.ProbabilisticSearch<AT, BLAT>(q, entryPoint.Index, distanceCalculator);
                entryPoint = layer.Get(w.MinValue);
            }
            return _layers[0].ProbabilisticSearch<AT, BLAT>(q, entryPoint.Index, distanceCalculator);
        }

        /// <summary>
        /// Breadth first search
        /// </summary>
        /// <param name="index">Start node index</param>
        /// <returns></returns>
        public IEnumerable<(uint NeighbourIndex, W Weight)> BreadthFirstSearch(uint index)
        {
            var queue = new Queue<uint>();
            var visited = new HashSet<uint> { index};
            var layer = _layers[0];
            queue.Enqueue(index);

            while (queue.Count > 0) {
                foreach (var neighbour in layer.EnumerateNeighbours(queue.Dequeue())) {
                    if(!visited.Add(neighbour.NeighbourIndex))
                        continue;
                    yield return neighbour;
                }
            }
        }

        int GetRandomLevel()
        {
            int ret;
            do {
                ret = int.CreateTruncating(_distribution.Sample());
            } while (ret >= maxLayers);
            return ret;
        }
    }
}
