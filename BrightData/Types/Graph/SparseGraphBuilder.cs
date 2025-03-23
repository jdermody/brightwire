using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.Types.Graph
{
    /// <summary>
    /// Builds a directed graph
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SparseGraphBuilder<T> : IBuildGraphs<T>
        where T: unmanaged
    {
        class Node(T value, uint nodeIndex)
        {
            HashSet<uint>? _edges = null;
            public T Value { get; } = value;
            public uint NodeIndex { get; } = nodeIndex;

            public void AddEdge(uint toNodeOffset)
            {
                (_edges??= []).Add(toNodeOffset);
            }
            public IEnumerable<uint> ConnectedToNodeOffsets => _edges ?? Enumerable.Empty<uint>();
            public int NumEdges => _edges?.Count ?? 0;
        }
        readonly List<Node> _nodes = new();

        /// <inheritdoc />
        public uint Add(T node)
        {
            var nodeIndex = (uint)_nodes.Count;
            _nodes.Add(new Node(node, nodeIndex));
            return nodeIndex;
        }

        /// <inheritdoc />
        public bool AddEdge(uint fromNodeIndex, uint toNodeIndex)
        {
            if (fromNodeIndex < _nodes.Count && toNodeIndex < _nodes.Count) {
                _nodes[(int)fromNodeIndex].AddEdge(toNodeIndex);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Builds the graph
        /// </summary>
        /// <returns></returns>
        public SparseGraph<T> Build()
        {
            using var nodes = new ArrayPoolBufferWriter<SparseGraph<T>.Node>(_nodes.Count);
            using var edges = new ArrayPoolBufferWriter<uint>();

            foreach (var node in _nodes) {
                var edgeIndex = edges.WrittenCount;
                var edgeCount = node.NumEdges;
                if (edgeCount > 0) {
                    var span = edges.GetSpan(edgeCount);
                    var index = 0;
                    foreach (var edge in node.ConnectedToNodeOffsets.Order())
                        span[index++] = edge;
                    edges.Advance(edgeCount);
                }
                nodes.Write(new SparseGraph<T>.Node(node.Value, edgeIndex, edgeCount));
            }

            return new SparseGraph<T>(nodes.WrittenSpan, edges.WrittenSpan);
        }
    }
}
