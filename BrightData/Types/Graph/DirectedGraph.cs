using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace BrightData.Types.Graph
{
    /// <summary>
    /// Directed graph
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly record struct DirectedGraph<T> : IHaveDataAsReadOnlyByteSpan, IHaveSize
        where T: unmanaged, IHaveSingleIndex
    {
        const int HeaderSize = 8;

        internal readonly record struct Node(T Value, int EdgeIndex, int EdgeCount)
        {
            
        }
        class NodeComparer(uint nodeIndex) : IComparable<Node>
        {
            public int CompareTo(Node other) => nodeIndex.CompareTo(other.Value.Index);
        }
        readonly Node[] _nodes;
        readonly int[] _edges;

        /// <summary>
        /// Creates a directed graph from graph data
        /// </summary>
        /// <param name="data"></param>
        public DirectedGraph(ReadOnlySpan<byte> data)
        {
            var header = MemoryMarshal.Cast<byte, uint>(data[..HeaderSize]);
            _nodes = MemoryMarshal.Cast<byte, Node>(data[HeaderSize..(int)header[0]]).ToArray();
            _edges = MemoryMarshal.Cast<byte, int>(data[(int)header[0]..(int)header[1]]).ToArray();
        }

        internal DirectedGraph(ReadOnlySpan<Node> nodes, ReadOnlySpan<int> edges)
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
        public bool TryGetValue(uint nodeIndex, [NotNullWhen(true)]out T? value)
        {
            var nodeSpan = _nodes.AsSpan();
            var index = nodeSpan.BinarySearch(new NodeComparer(nodeIndex));
            if (index < 0) {
                value = null;
                return false;
            }
            value = nodeSpan[index].Value;
            return true;
        }

        /// <summary>
        /// Enumerates the connected nodes from a single node
        /// </summary>
        /// <param name="nodeIndex"></param>
        /// <returns></returns>
        public IEnumerable<T> EnumerateConnectedNodes(uint nodeIndex)
        {
            var nodeSpan = _nodes.AsSpan();
            var index = nodeSpan.BinarySearch(new NodeComparer(nodeIndex));
            if (index >= 0) {
                var (_, edgeIndex, edgeCount) = nodeSpan[index];
                foreach (var connectedTo in _edges[edgeIndex..(edgeIndex+edgeCount)])
                    yield return _nodes[connectedTo].Value;
            }
        }
    }
}
