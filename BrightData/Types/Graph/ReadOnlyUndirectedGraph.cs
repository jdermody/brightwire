using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BrightData.Types.Graph.Helper;

namespace BrightData.Types.Graph
{
    /// <summary>
    /// A dense graph implementation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly record struct ReadOnlyUndirectedGraph<T> : IReadOnlyGraph<T>, IHaveDataAsReadOnlyByteSpan
        where T : unmanaged, IEquatable<T>
    {
        const int HeaderSize = 8;

        readonly ReadOnlyMemory<T> _nodes;
        readonly BitVector _edges;

        /// <summary>
        /// Creates a graph from the given nodes and edges
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="edges"></param>
        public ReadOnlyUndirectedGraph(ReadOnlyMemory<T> nodes, BitVector edges)
        {
            _nodes = nodes;
            _edges = edges;
        }

        /// <summary>
        /// Creates a graph from a serialized byte array
        /// </summary>
        /// <param name="data"></param>
        public ReadOnlyUndirectedGraph(ReadOnlySpan<byte> data)
        {
            var header = MemoryMarshal.Cast<byte, uint>(data[..HeaderSize]);
            _nodes = MemoryMarshal.Cast<byte, T>(data[HeaderSize..(int)header[0]]).ToArray();
            _edges = new BitVector(MemoryMarshal.Cast<byte, ulong>(data[(int)header[0]..(int)header[1]]).ToArray().AsMemory(), (uint)_nodes.Length * (uint)_nodes.Length);
        }

        /// <inheritdoc />
        public ReadOnlySpan<byte> DataAsBytes
        {
            get
            {
                var nodeBytes = MemoryMarshal.AsBytes(_nodes.Span);
                var edgesBytes = _edges.DataAsBytes;
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

        /// <inheritdoc />
        public uint Size => (uint)_nodes.Length;

        /// <inheritdoc />
        public IEnumerable<uint> GetConnectedNodes(uint nodeIndex)
        {
            if (nodeIndex >= Size)
                throw new ArgumentOutOfRangeException(nameof(nodeIndex));

            var start = nodeIndex * Size;
            for (uint i = 0; i < Size; i++)
            {
                if (_edges[start + i])
                    yield return i;
            }
        }

        /// <summary>
        /// Finds the index of an edge in the adjacency matrix
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="numNodes"></param>
        /// <returns></returns>
        public static uint GetEdgeIndex(uint from, uint to, uint numNodes) => from * numNodes + to;

        /// <inheritdoc />
        public IEnumerable<uint> DepthFirstSearch(uint startNodeIndex) => GraphHelper<ReadOnlyUndirectedGraph<T>>.DepthFirstSearch(ref Unsafe.AsRef(in this), startNodeIndex);

        /// <inheritdoc />
        public IEnumerable<uint> BreadthFirstSearch(uint startNodeIndex) => GraphHelper<ReadOnlyUndirectedGraph<T>>.BreadthFirstSearch(ref Unsafe.AsRef(in this), startNodeIndex);

        /// <inheritdoc />
        public T Get(uint nodeIndex) => _nodes.Span[(int)nodeIndex];

        /// <inheritdoc />
        public uint GetNodeIndex(T node)
        {
            var ret = _nodes.Span.IndexOf(node);
            if (ret >= 0)
                return (uint)ret;
            throw new ArgumentException("Node not found", nameof(node));
        }
    }
}
