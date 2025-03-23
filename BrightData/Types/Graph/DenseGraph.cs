using System;
using System.Runtime.InteropServices;

namespace BrightData.Types.Graph
{
    public readonly record struct DenseGraph<T> : IGraph<T>, IHaveDataAsReadOnlyByteSpan
        where T: unmanaged
    {
        const int HeaderSize = 8;

        readonly T[] _nodes;
        readonly BitVector _edges;

        public DenseGraph(T[] nodes, BitVector edges)
        {
            _nodes = nodes;
            _edges = edges;
        }

        public DenseGraph(ReadOnlySpan<byte> data)
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
                var nodeBytes = MemoryMarshal.AsBytes(_nodes.AsSpan());
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

        public static uint GetEdgeIndex(uint from, uint to, uint numNodes) => from * numNodes + to;
    }
}
