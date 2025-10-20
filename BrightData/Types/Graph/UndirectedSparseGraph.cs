using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BrightData.Types.Graph.Helper;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.Types.Graph
{
    /// <summary>
    /// Builds a directed graph
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct UndirectedSparseGraph<T> : IUndirectedGraphs<T>
        where T: unmanaged, IEquatable<T>
    {
        class Node(T value, uint nodeIndex)
        {
            HashSet<uint>? _edges = null;
            public T Value { get; } = value;
            public uint NodeIndex { get; } = nodeIndex;

            public void AddEdge(uint toNodeIndex)
            {
                (_edges??= []).Add(toNodeIndex);
            }
            public IEnumerable<uint> ConnectedToNodeIndices => _edges ?? Enumerable.Empty<uint>();
            public int NumEdges => _edges?.Count ?? 0;
        }
        readonly List<Node> _nodes = [];

        /// <summary>
        /// Default constructor
        /// </summary>
        public UndirectedSparseGraph()
        {
        }

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
        public ReadOnlyUndirectedSparseGraph<T> Build()
        {
            using var nodes = new ArrayPoolBufferWriter<ReadOnlyUndirectedSparseGraph<T>.Node>(_nodes.Count);
            using var edges = new ArrayPoolBufferWriter<uint>();

            foreach (var node in _nodes) {
                var edgeIndex = edges.WrittenCount;
                var edgeCount = node.NumEdges;
                if (edgeCount > 0) {
                    var span = edges.GetSpan(edgeCount);
                    var index = 0;
                    foreach (var edge in node.ConnectedToNodeIndices.Order())
                        span[index++] = edge;
                    edges.Advance(edgeCount);
                }
                nodes.Write(new ReadOnlyUndirectedSparseGraph<T>.Node(node.Value, edgeIndex, edgeCount));
            }

            return new ReadOnlyUndirectedSparseGraph<T>(nodes.WrittenSpan, edges.WrittenSpan);
        }

        /// <inheritdoc />
        public uint Size => (uint) _nodes.Count;

        /// <inheritdoc />
        public IEnumerable<uint> GetConnectedNodes(uint nodeIndex)
        {
            if (nodeIndex < Size)
                return _nodes[(int)nodeIndex].ConnectedToNodeIndices;
            return [];
        }

        /// <inheritdoc />
        public IEnumerable<uint> DepthFirstSearch(uint startNodeIndex) => GraphHelper<UndirectedSparseGraph<T>>.DepthFirstSearch(ref Unsafe.AsRef(in this), startNodeIndex);

        /// <inheritdoc />
        public IEnumerable<uint> BreadthFirstSearch(uint startNodeIndex) => GraphHelper<UndirectedSparseGraph<T>>.BreadthFirstSearch(ref Unsafe.AsRef(in this), startNodeIndex);

        /// <inheritdoc />
        public T Get(uint nodeIndex) => _nodes[(int)nodeIndex].Value;
    }
}
