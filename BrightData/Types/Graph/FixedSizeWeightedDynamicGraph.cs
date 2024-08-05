using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using BrightData.Types.Helper;

namespace BrightData.Types.Graph
{
    /// <summary>
    /// A fixed size weighted graph
    /// </summary>
    /// <typeparam name="T">Type to store in each node</typeparam>
    /// <typeparam name="W">Type to describe weights</typeparam>
    /// <typeparam name="AT">Array type (fixed size)</typeparam>
    public class FixedSizeWeightedDynamicGraph<T, W, AT> : IWeightedDynamicGraph<T, W>
        where T : unmanaged, IHaveSingleIndex
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
        where AT : unmanaged, IFixedSizeSortedArray<uint, W>
    {
        readonly IndexedSortedArray<FixedSizeWeightedGraphNode<T, W, AT>> _nodes = new();

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
        public T Find(uint nodeIndex)
        {
            ref var node = ref _nodes.Find(nodeIndex);
            if (!Unsafe.IsNullRef(ref node))
                return node.Value;
            throw new ArgumentException($"Node with index {nodeIndex} was not found");
        }

        /// <inheritdoc />
        public T this[uint nodePosition] => _nodes[nodePosition].Value;

        /// <inheritdoc />
        public ReadOnlySpan<uint> GetNeighbours(uint nodeIndex)
        {
            ref var node = ref _nodes.Find(nodeIndex);
            if (!Unsafe.IsNullRef(ref node))
                return node.NeighbourSpan;
            return ReadOnlySpan<uint>.Empty;
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
    }
}
