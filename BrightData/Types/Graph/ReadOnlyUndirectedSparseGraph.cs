using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BrightData.Types.Graph.Helper;

namespace BrightData.Types.Graph
{
    /// <summary>
    /// Directed graph
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly record struct ReadOnlyUndirectedSparseGraph<T> : IReadOnlyGraph<T>, IHaveDataAsReadOnlyByteSpan
        where T : unmanaged, IEquatable<T>
    {
        const int HeaderSize = 8;

        internal readonly record struct Node(T Value, int EdgeIndex, int EdgeCount);
        readonly Node[] _nodes;
        readonly uint[] _edges;

        /// <summary>
        /// Creates a directed graph from graph data
        /// </summary>
        /// <param name="data"></param>
        public ReadOnlyUndirectedSparseGraph(ReadOnlySpan<byte> data)
        {
            var header = MemoryMarshal.Cast<byte, uint>(data[..HeaderSize]);
            _nodes = MemoryMarshal.Cast<byte, Node>(data[HeaderSize..(int)header[0]]).ToArray();
            _edges = MemoryMarshal.Cast<byte, uint>(data[(int)header[0]..(int)header[1]]).ToArray();
        }

        internal ReadOnlyUndirectedSparseGraph(ReadOnlySpan<Node> nodes, ReadOnlySpan<uint> edges)
        {
            _nodes = nodes.ToArray();
            _edges = edges.ToArray();
        }

        /// <inheritdoc />
        public uint Size => (uint)_nodes.Length;

        ReadOnlySpan<byte> IHaveDataAsReadOnlyByteSpan.DataAsBytes
        {
            get
            {
                var nodeBytes = MemoryMarshal.AsBytes(_nodes.AsSpan());
                var edgesBytes = MemoryMarshal.AsBytes(_edges.AsSpan());
                Span<uint> header = [
                    (uint)(HeaderSize + nodeBytes.Length),
                    (uint)(HeaderSize + nodeBytes.Length + edgesBytes.Length)
                ];
                var headerBytes = MemoryMarshal.AsBytes(header);
                var ret = new byte[headerBytes.Length + nodeBytes.Length + edgesBytes.Length];
                var retSpan = ret.AsSpan();
                headerBytes.CopyTo(retSpan[..headerBytes.Length]);
                nodeBytes.CopyTo(retSpan[headerBytes.Length..]);
                edgesBytes.CopyTo(retSpan[(headerBytes.Length + nodeBytes.Length)..]);
                return retSpan;
            }
        }

        /// <summary>
        /// Returns the value associated with the node
        /// </summary>
        /// <param name="nodeIndex"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(uint nodeIndex, [NotNullWhen(true)] out T? value)
        {
            if (nodeIndex < _nodes.Length) {
                value = _nodes[nodeIndex].Value;
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Enumerates the connected nodes from a single node
        /// </summary>
        /// <param name="nodeIndex"></param>
        /// <returns></returns>
        public IEnumerable<T> EnumerateDirectlyConnectedNodes(uint nodeIndex)
        {
            if (nodeIndex < _nodes.Length) {
                var (_, edgeIndex, edgeCount) = _nodes[nodeIndex];
                foreach (var connectedTo in _edges[edgeIndex..(edgeIndex + edgeCount)])
                    yield return _nodes[connectedTo].Value;
            }
        }

        /// <inheritdoc />
        public IEnumerable<uint> GetConnectedNodes(uint nodeIndex)
        {
            if (nodeIndex < _nodes.Length) {
                var (_, edgeIndex, edgeCount) = _nodes[nodeIndex];
                foreach (var connectedTo in _edges[edgeIndex..(edgeIndex + edgeCount)])
                    yield return connectedTo;
            }
        }

        /// <inheritdoc />
        public IEnumerable<uint> DepthFirstSearch(uint startNodeIndex) => GraphHelper<ReadOnlyUndirectedSparseGraph<T>>.DepthFirstSearch(ref Unsafe.AsRef(in this), startNodeIndex);

        /// <inheritdoc />
        public IEnumerable<uint> BreadthFirstSearch(uint startNodeIndex) => GraphHelper<ReadOnlyUndirectedSparseGraph<T>>.BreadthFirstSearch(ref Unsafe.AsRef(in this), startNodeIndex);

        /// <inheritdoc />
        public T Get(uint nodeIndex) => _nodes[nodeIndex].Value;
    }
}
