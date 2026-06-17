using BrightData.Types.Graph.Helper;
using BrightData.Types.Helper;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace BrightData.Types.Graph
{
    /// <summary>
    /// A fixed size weighted graph
    /// </summary>
    /// <typeparam name="T">Type to store in each node</typeparam>
    /// <typeparam name="W">Type to describe weights</typeparam>
    /// <typeparam name="AT">Array type (fixed size)</typeparam>
    public readonly struct FixedSizeWeightedGraph<T, W, AT> : IWeightedGraph<T, W>
        where T : unmanaged, IEquatable<T>, IHaveSingleIndex
        where W : unmanaged, IBinaryFloatingPointIeee754<W>, IMinMaxValue<W>
        where AT : unmanaged, IFixedSizeSortedArray<uint, W>
    {
        readonly IndexedSortedArray<FixedSizeWeightedGraphNode<T, W, AT>> _nodes = new();

        /// <summary>
        /// Default constructor
        /// </summary>
        public FixedSizeWeightedGraph()
        {

        }

        /// <inheritdoc />
        public void Add(T value)
        {
            _nodes.Add(new FixedSizeWeightedGraphNode<T, W, AT>(value));
        }

        /// <inheritdoc />
        public void Add(T value, ReadOnlySpan<(uint Index, W Weight)> neighbours)
        {
            var node = new FixedSizeWeightedGraphNode<T, W, AT>(value);
            foreach (var (index, weight) in neighbours)
                node.TryAddNeighbour(index, weight);
            _nodes.Add(node);
        }

        /// <inheritdoc />
        public IEnumerable<uint> DepthFirstSearch(uint startNodeIndex) => GraphHelper<FixedSizeWeightedGraph<T, W, AT>>.DepthFirstSearch(ref Unsafe.AsRef(in this), startNodeIndex);

        /// <inheritdoc />
        public IEnumerable<uint> BreadthFirstSearch(uint startNodeIndex) => GraphHelper<FixedSizeWeightedGraph<T, W, AT>>.BreadthFirstSearch(ref Unsafe.AsRef(in this), startNodeIndex);

        /// <inheritdoc />
        public T Get(uint nodeIndex)
        {
            if (TryGet(nodeIndex, out var value))
                return value;
            throw new IndexOutOfRangeException($"Node with index {nodeIndex} was not found");
        }

        /// <summary>
        /// Returns true if a node with the specified index exists in the graph.
        /// </summary>
        /// <param name="nodeIndex">The node index to check.</param>
        /// <returns>True if the node exists, false otherwise.</returns>
        public bool Contains(uint nodeIndex) => _nodes.TryFind(nodeIndex, out _);

        /// <summary>
        /// Tries to retrieve the value stored at the specified node index.
        /// </summary>
        /// <param name="nodeIndex">The node index to look up.</param>
        /// <param name="value">When this method returns, contains the value at the specified node index, or default(T) if the node was not found.</param>
        /// <returns>True if the node was found, false otherwise.</returns>
        public bool TryGet(uint nodeIndex, out T value)
        {
            ref var node = ref _nodes.Find(nodeIndex);
            if (!Unsafe.IsNullRef(ref node))
            {
                value = node.Value;
                return true;
            }
            value = default!;
            return false;
        }

        /// <inheritdoc />
        public uint GetNodeIndex(T node) => node.Index;

        /// <inheritdoc />
        public ReadOnlySpan<uint> GetNeighbours(uint nodeIndex)
        {
            ref var node = ref _nodes.Find(nodeIndex);
            if (!Unsafe.IsNullRef(ref node))
                return node.NeighbourSpan;
            return [];
        }

        /// <inheritdoc />
        public IEnumerable<(uint NeighbourIndex, W Weight)> EnumerateNeighbours(uint nodeIndex)
        {
            ref var node = ref _nodes.Find(nodeIndex);
            if (!Unsafe.IsNullRef(ref node))
                return node.WeightedNeighbours;
            return [];
        }

        /// <inheritdoc />
        public bool AddNeighbour(uint nodeIndex, uint neighbourIndex, W weight)
        {
            ref var node = ref _nodes.Find(nodeIndex);
            if (!Unsafe.IsNullRef(ref node)) {
                return node.TryAddNeighbour(neighbourIndex, weight);
            }

            return false;
        }

        /// <inheritdoc />
        public uint Size => _nodes.Size;

        /// <inheritdoc />
        public RAT ProbabilisticSearch<RAT, CAT>(uint q, uint entryPoint, ICalculateNodeWeights<W> distanceCalculator)
            where RAT : struct, IFixedSizeSortedArray<uint, W>
            where CAT : struct, IFixedSizeSortedArray<uint, W>
        {
            return WeightedGraphHelper.SearchFixedSize<W, RAT, CAT>(q, entryPoint, distanceCalculator, GetNeighbours);
        }

        /// <inheritdoc />
        public IEnumerable<uint> GetConnectedNodes(uint nodeIndex)
        {
            ref var node = ref _nodes.Find(nodeIndex);
            return !Unsafe.IsNullRef(ref node) ? node.Neighbours : [];
        }

        /// <summary>
        /// Converts to a read only graph
        /// </summary>
        /// <returns></returns>
        public ReadOnlyFixedSizeWeightedGraph<T, W, AT> ToReadOnly() => new(_nodes.Values.ToArray());
    }
}
