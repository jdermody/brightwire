using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
        where T : IHaveSingleIndex
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
        where AT : struct, IFixedSizeSortedArray<uint, W>

    {
        readonly Dictionary<uint, FixedSizeWeightedGraphNode<T, W, AT>> _nodes = [];

        /// <inheritdoc />
        public IWeightedGraphNode<T, W> Create(T value, bool addToGraph = true)
        {
            var ret = new FixedSizeWeightedGraphNode<T, W, AT>(value);
            if(addToGraph)
                _nodes.Add(value.Index, ret);
            return ret;
        }

        /// <inheritdoc />
        public void Add(IWeightedGraphNode<T, W> node)
        {
            if(node is FixedSizeWeightedGraphNode<T, W, AT> same)
                _nodes.Add(same.Index, same);
            else {
                var ret = new FixedSizeWeightedGraphNode<T, W, AT>(node.Value);
                foreach (var (neighbour, weight) in node.WeightedNeighbours)
                    ret.AddNeighbour(neighbour, weight);
                _nodes.Add(ret.Index, ret);
            }
        }

        /// <inheritdoc />
        public uint Size => (uint)_nodes.Count;

        /// <inheritdoc />
        public IWeightedGraphNode<T, W> Get(uint index) => _nodes[index];

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

                foreach (var neighbour in _nodes[c].NeighbourSpan) {
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
