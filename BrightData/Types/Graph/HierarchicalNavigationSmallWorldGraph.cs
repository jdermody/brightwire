using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace BrightData.Types.Graph
{
    /// <summary>
    /// A hierarchical navigation small world (HNSW) graph that uses multiple sub-graphs as a skip list
    /// for efficient approximate nearest neighbour search.
    /// </summary>
    /// <param name="context">Data context for creating distributions.</param>
    /// <param name="maxLayers">Maximum number of layers in the graph (upper bound on node levels).</param>
    /// <param name="ml">ML multiplier controlling the level distribution. Standard HNSW uses 1/M where M is the max connections per layer.</param>
    /// <typeparam name="T">Type of node value; must be unmanaged and have a single index.</typeparam>
    /// <typeparam name="W">Type of weight; must be an IEEE 754 binary floating-point number.</typeparam>
    /// <typeparam name="AT">Array type used for upper layers (fixed-size sorted array of neighbours).</typeparam>
    /// <typeparam name="BLAT">Array type used for the base layer (fixed-size sorted array of neighbours).</typeparam>
    public class HierarchicalNavigationSmallWorldGraph<T, W, AT, BLAT>(BrightDataContext context, int maxLayers, W ml)
        where T : unmanaged, IHaveSingleIndex
        where W : unmanaged, INumber<W>, IMinMaxValue<W>, IBinaryFloatingPointIeee754<W>
        where AT : unmanaged, IFixedSizeSortedArray<uint, W>
        where BLAT : unmanaged, IFixedSizeSortedArray<uint, W>
    {
        readonly record struct NodeIndex(T Value, uint LayerIndex) : IHaveSingleIndex
        {
            public uint Index => Value.Index;
        }

        readonly IContinuousDistribution<W> _distribution = context.CreateExponentialDistribution<W>(ml);
        readonly IWeightedGraph<NodeIndex, W>[] _layers = Enumerable.Range(0, maxLayers)
            .Select(i => (IWeightedGraph<NodeIndex, W>)(i == 0 ? new FixedSizeWeightedGraph<NodeIndex, W, BLAT>() : new FixedSizeWeightedGraph<NodeIndex, W, AT>()))
            .ToArray()
        ;
        NodeIndex? _entryPoint = null;

        /// <summary>
        /// Adds a single new node to the graph.
        /// </summary>
        /// <param name="distanceCalculator">Calculator for computing distances (weights) between nodes.</param>
        /// <param name="value">The node value to add.</param>
        [SkipLocalsInit]
        public void Add(ICalculateNodeWeights<W> distanceCalculator, T value)
        {
            Span<(uint, W)> newNodeNeighbours = stackalloc (uint, W)[32];
            AddNode(distanceCalculator, value, newNodeNeighbours);
        }

        /// <summary>
        /// Adds multiple new nodes to the graph from a span (zero-copy).
        /// </summary>
        /// <param name="distanceCalculator">Calculator for computing distances (weights) between nodes.</param>
        /// <param name="values">Span of node values to add.</param>
        public void Add(ICalculateNodeWeights<W> distanceCalculator, ReadOnlySpan<T> values)
        {
            Span<(uint, W)> newNodeNeighbours = stackalloc (uint, W)[32];
            for (var i = 0; i < values.Length; i++)
                AddNode(distanceCalculator, values[i], newNodeNeighbours);
        }

        [SkipLocalsInit]
        void AddNode(ICalculateNodeWeights<W> distanceCalculator, T value, Span<(uint, W)> newNodeNeighbours)
        {
            var entryPoint = _entryPoint;
            var level = GetRandomLevel();

            // zoom in from the current entry point toward the target level
            uint? entryPointLevel = null;
            if (entryPoint is not null) {
                entryPointLevel = entryPoint.Value.LayerIndex;
                for (var i = entryPointLevel.Value; i > level; i--) {
                    var layer = _layers[i];
                    var w = layer.ProbabilisticSearch<FixedSizeSortedAscending1Array<uint, W>, AT>(value.Index, entryPoint.Value.Index, distanceCalculator);
                    entryPoint = layer.Get(w.MinValue);
                }
            }

            // add node to layers: upper layers (no neighbours), then lower layers (with neighbours)
            var from = Math.Min(level, entryPoint?.LayerIndex ?? int.MaxValue);
            for (var i = level; i > from; i--)
                _layers[i].Add(new NodeIndex(value, (uint)i));
            for (var i = from; i >= 0; i--) {
                var layer = _layers[i];
                var newNodeIndex = 0;
                if (entryPoint is not null) {
                    var w = layer.ProbabilisticSearch<AT, BLAT>(value.Index, entryPoint.Value.Index, distanceCalculator);
                    foreach (var (ni, nw) in w.Elements) {
                        // only record edge if bidirectional connection succeeds
                        if (layer.AddNeighbour(ni, value.Index, nw))
                            newNodeNeighbours[newNodeIndex++] = (ni, nw);
                    }
                }
                layer.Add(new NodeIndex(value, (uint)i), newNodeNeighbours[..newNodeIndex]);
            }

            if (!entryPointLevel.HasValue || level > entryPointLevel.Value)
                _entryPoint = _layers[level].Get(value.Index);
        }

        /// <summary>
        /// Performs a K nearest neighbour search (probabilistic) for the given query index.
        /// </summary>
        /// <param name="q">The query node index.</param>
        /// <param name="distanceCalculator">Calculator for computing distances (weights) between nodes.</param>
        /// <returns>A fixed-size sorted array of nearest neighbours and their weights.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the graph contains no nodes.</exception>
        public AT KnnSearch(uint q, ICalculateNodeWeights<W> distanceCalculator)
        {
            var entryPoint = _entryPoint ?? throw new InvalidOperationException("Cannot perform KNN search on an empty graph. Add nodes first.");
            for (var i = (int)entryPoint.LayerIndex; i > 0; i--) {
                var layer = _layers[i];
                var w = layer.ProbabilisticSearch<AT, BLAT>(q, entryPoint.Index, distanceCalculator);
                entryPoint = layer.Get(w.MinValue);
            }
            return _layers[0].ProbabilisticSearch<AT, BLAT>(q, entryPoint.Index, distanceCalculator);
        }

        /// <summary>
        /// Performs a breadth-first search on the base layer (layer 0) starting from the given node index.
        /// Uses pooled memory for the queue and a <see cref="BitArray"/> for visited tracking to minimize allocations.
        /// </summary>
        /// <param name="index">The start node index.</param>
        /// <returns>An enumerable of neighbour index and weight pairs discovered by the BFS.</returns>
        public IEnumerable<(uint NeighbourIndex, W Weight)> BreadthFirstSearch(uint index)
        {
            var queue = ArrayPool<uint>.Shared.Rent(32);
            var visited = new BitArray((int)_layers[0].Size + 1);
            int head = 0, tail = 0;

            try {
                queue[tail++] = index;
                visited.Set((int)index, true);
                var layer = _layers[0];

                while (head < tail) {
                    var current = queue[head++];

                    // expand queue if near capacity
                    if (tail >= queue.Length) {
                        var expanded = ArrayPool<uint>.Shared.Rent(queue.Length * 2);
                        Array.Copy(queue, expanded, queue.Length);
                        ArrayPool<uint>.Shared.Return(queue);
                        queue = expanded;
                    }

                    foreach (var neighbour in layer.EnumerateNeighbours(current)) {
                        if (visited.Get((int)neighbour.NeighbourIndex))
                            continue;
                        visited.Set((int)neighbour.NeighbourIndex, true);

                        if (tail >= queue.Length) {
                            var expanded = ArrayPool<uint>.Shared.Rent(queue.Length * 2);
                            Array.Copy(queue, expanded, queue.Length);
                            ArrayPool<uint>.Shared.Return(queue);
                            queue = expanded;
                        }
                        queue[tail++] = neighbour.NeighbourIndex;
                        yield return neighbour;
                    }
                }
            }
            finally {
                ArrayPool<uint>.Shared.Return(queue);
            }
        }

        /// <summary>
        /// Generates a random layer level using the exponential distribution, clamped to the valid range.
        /// </summary>
        int GetRandomLevel()
        {
            var sample = int.CreateTruncating(_distribution.Sample());
            return Math.Min(sample, maxLayers - 1);
        }
    }
}
