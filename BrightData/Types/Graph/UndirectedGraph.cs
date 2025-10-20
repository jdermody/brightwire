using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BrightData.Types.Graph.Helper;

namespace BrightData.Types.Graph
{
    /// <inheritdoc />
    public readonly struct UndirectedGraph<T> : IUndirectedGraphs<T>
        where T: unmanaged, IEquatable<T>
    {
        readonly record struct NodeIndexPair(uint FirstNodeIndex, uint SecondNodeIndex);

        readonly List<T> _nodes = [];
        readonly HashSet<NodeIndexPair> _edges = [];

        /// <summary>
        /// Default constructor
        /// </summary>
        public UndirectedGraph()
        {
        }

        /// <inheritdoc />
        public uint Add(T node)
        {
            var nodeIndex = (uint)_nodes.Count;
            _nodes.Add(node);
            return nodeIndex;
        }

        /// <inheritdoc />
        public bool AddEdge(uint fromNodeIndex, uint toNodeIndex)
        {
            var nodeCount = Size;
            if (fromNodeIndex < nodeCount && toNodeIndex < nodeCount) {
                if(fromNodeIndex < toNodeIndex)
                    return _edges.Add(new(fromNodeIndex, toNodeIndex));
                return _edges.Add(new(toNodeIndex, fromNodeIndex));
            }

            return false;
        }

        /// <summary>
        /// Converts to a read only graph
        /// </summary>
        /// <returns></returns>
        public ReadOnlyUndirectedGraph<T> ToReadOnly()
        {
            var nodeCount = (uint)_nodes.Count;
            var edges = new BitVector(nodeCount * nodeCount);
            foreach (var (from, to) in _edges) {
                edges[ReadOnlyUndirectedGraph<T>.GetEdgeIndex(from, to, nodeCount)] = true;
            }
            return new ReadOnlyUndirectedGraph<T>(_nodes.ToArray(), edges);
        }

        /// <inheritdoc />
        public uint Size => (uint)_nodes.Count;

        /// <inheritdoc />
        public IEnumerable<uint> GetConnectedNodes(uint nodeIndex)
        {
            var visited = new HashSet<uint>();

            foreach (var (first, second) in _edges) {
                if (first == nodeIndex) {
                    if (visited.Add(second))
                        yield return second;
                } else if (second == nodeIndex) {
                    if(visited.Add(first))
                        yield return first;
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<uint> DepthFirstSearch(uint startNodeIndex) => GraphHelper<UndirectedGraph<T>>.DepthFirstSearch(ref Unsafe.AsRef(in this), startNodeIndex);

        /// <inheritdoc />
        public IEnumerable<uint> BreadthFirstSearch(uint startNodeIndex) => GraphHelper<UndirectedGraph<T>>.BreadthFirstSearch(ref Unsafe.AsRef(in this), startNodeIndex);

        /// <inheritdoc />
        public T Get(uint nodeIndex) => _nodes[(int)nodeIndex];
    }
}
