using BrightData.Types.Graph.Helper;
using CommunityToolkit.HighPerformance.Buffers;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance;

namespace BrightData.Types.Graph
{

    /// <summary>
    /// Mutable undirected graph
    /// </summary>
    /// <typeparam name="T">Node type</typeparam>
    public readonly struct UndirectedGraph<T> : IMutableGraph<T>, IReadOnlyGraph<T>, IDisposable
        where T: unmanaged, IEquatable<T>
    {
        readonly record struct NodeIndexPair(uint FirstNodeIndex, uint SecondNodeIndex);

        readonly ArrayPoolBufferWriter<T> _nodes = new();
        readonly HashSet<NodeIndexPair> _edges = [];

        /// <summary>
        /// Default constructor
        /// </summary>
        public UndirectedGraph()
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _nodes.Dispose();
        }

        /// <inheritdoc />
        public uint Add(T node)
        {
            var nodeIndex = (uint)_nodes.WrittenCount;
            _nodes.Write(node);
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

        /// <inheritdoc />
        public void Clear()
        {
            _nodes.Clear();
            _edges.Clear();
        }

        /// <summary>
        /// Converts to a read only graph
        /// </summary>
        /// <returns></returns>
        public ReadOnlyUndirectedGraph<T> ToReadOnly()
        {
            var nodeCount = (uint)_nodes.WrittenCount;
            var edges = new BitVector(nodeCount * nodeCount);
            foreach (var (from, to) in _edges) {
                edges[ReadOnlyUndirectedGraph<T>.GetEdgeIndex(from, to, nodeCount)] = true;
                edges[ReadOnlyUndirectedGraph<T>.GetEdgeIndex(to, from, nodeCount)] = true;
            }
            return new ReadOnlyUndirectedGraph<T>(_nodes.WrittenMemory, edges);
        }

        /// <inheritdoc />
        public uint Size => (uint)_nodes.WrittenCount;

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
        public T Get(uint nodeIndex) => _nodes.WrittenSpan[(int)nodeIndex];

        /// <inheritdoc />
        public uint GetNodeIndex(T node)
        {
            var ret = _nodes.WrittenSpan.IndexOf(node);
            if (ret >= 0)
                return (uint)ret;
            throw new ArgumentException("Node not found", nameof(node));
        }
    }
}
