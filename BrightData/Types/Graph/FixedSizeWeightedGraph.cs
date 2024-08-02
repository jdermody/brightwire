using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Types.Graph
{
    /// <summary>
    /// A fixed size graph weighted graph
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="W"></typeparam>
    /// <typeparam name="AT"></typeparam>
    public class FixedSizeWeightedGraph<T, W, AT> : IWeightedGraph<T, W>
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
                node.AddNeighbour(index, weight);
            _nodes.Add(node);
        }

        /// <inheritdoc />
        public T Get(uint nodeIndex)
        {
            ref var node = ref _nodes.Get(nodeIndex);
            if (!Unsafe.IsNullRef(ref node))
                return node.Value;
            throw new ArgumentException($"Node with index {nodeIndex} was not found");
        }

        /// <inheritdoc />
        public ReadOnlySpan<uint> GetNeighbours(uint nodeIndex)
        {
            ref var node = ref _nodes.Get(nodeIndex);
            if (!Unsafe.IsNullRef(ref node))
                return node.NeighbourSpan;
            return ReadOnlySpan<uint>.Empty;
        }

        /// <inheritdoc />
        public bool AddNeighbour(uint nodeIndex, uint neighbourIndex, W weight)
        {
            ref var node = ref _nodes.Get(nodeIndex);
            if (!Unsafe.IsNullRef(ref node)) {
                return node.AddNeighbour(neighbourIndex, weight);
            }

            return false;
        }

        /// <inheritdoc />
        public uint Size => _nodes.Size;

        /// <inheritdoc />
        public RAT Search<RAT, CAT>(uint q, uint entryPoint, ICalculateNodeWeights<W> distanceCalculator)
            where RAT : struct, IFixedSizeSortedArray<uint, W>
            where CAT : struct, IFixedSizeSortedArray<uint, W>
        {
            var visited = new HashSet<uint> { entryPoint };
            var candidates = new CAT();
            var distanceEQ = distanceCalculator.GetWeight(q, entryPoint);
            candidates.TryAdd(entryPoint, distanceEQ);
            var ret = new RAT();
            ret.TryAdd(entryPoint, distanceEQ);

            while (candidates.Size > 0) {
                var c = candidates.RemoveAt(0);
                var f = ret.MaxValue;
                if (distanceCalculator.GetWeight(c, q) > distanceCalculator.GetWeight(f, q))
                    break;

                foreach (var neighbour in GetNeighbours(c)) {
                    if(!visited.Add(neighbour))
                        continue;

                    f = ret.MaxValue;
                    if ((distanceEQ = distanceCalculator.GetWeight(neighbour, q)) < distanceCalculator.GetWeight(f, q)) {
                        candidates.TryAdd(neighbour, distanceEQ);
                        ret.TryAdd(neighbour, distanceEQ);
                    }
                }
            }
            return ret;
        }
    }
}
