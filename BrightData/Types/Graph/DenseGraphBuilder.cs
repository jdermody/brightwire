using System.Collections.Generic;

namespace BrightData.Types.Graph
{
    public class DenseGraphBuilder<T> : IBuildGraphs<T>
        where T: unmanaged
    {
        readonly record struct NodeIndexPair(uint FirstNodeIndex, uint SecondNodeIndex);

        readonly List<T> _nodes = [];
        readonly HashSet<NodeIndexPair> _edges = [];
        readonly HashSet<uint> _validIndices = [];

        /// <inheritdoc />
        public uint Add(T node)
        {
            var nodeIndex = (uint)_nodes.Count;
            _nodes.Add(node);
            _validIndices.Add(nodeIndex);
            return nodeIndex;
        }

        /// <inheritdoc />
        public bool AddEdge(uint fromNodeIndex, uint toNodeIndex)
        {
            if (_validIndices.Contains(fromNodeIndex) && _validIndices.Contains(toNodeIndex)) {
                _edges.Add(new(fromNodeIndex, toNodeIndex));
                return true;
            }

            return false;
        }

        public DenseGraph<T> Build()
        {
            var nodeCount = (uint)_nodes.Count;
            var edges = new BitVector(nodeCount * nodeCount);
            foreach (var (from, to) in _edges) {
                edges[DenseGraph<T>.GetEdgeIndex(from, to, nodeCount)] = true;
            }
            return new DenseGraph<T>(_nodes.ToArray(), edges);
        }
    }
}
