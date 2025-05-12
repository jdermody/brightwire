using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace BrightData.Types.Graph
{
    /// <summary>
    /// A dense graph implementation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly record struct DenseGraph<T> : IGraph<T>, IHaveDataAsReadOnlyByteSpan
        where T : unmanaged
    {
        const int HeaderSize = 8;

        readonly T[] _nodes;
        readonly BitVector _edges;

        /// <summary>
        /// Creates a graph from the given nodes and edges
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="edges"></param>
        public DenseGraph(T[] nodes, BitVector edges)
        {
            _nodes = nodes;
            _edges = edges;
        }

        /// <summary>
        /// Creates a graph from a serialized byte array
        /// </summary>
        /// <param name="data"></param>
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

        /// <summary>
        /// Finds the index of an edge in the adjacency matrix
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="numNodes"></param>
        /// <returns></returns>
        public static uint GetEdgeIndex(uint from, uint to, uint numNodes) => from * numNodes + to;

        public IEnumerable<T> DepthFirstSearch(T start)
        {
            var startIndex = Array.IndexOf(_nodes, start);
            if (startIndex == -1)
                yield break;

            var stack = new Stack<int>();
            stack.Push(startIndex);
            var visited = new HashSet<int>();
            while (stack.Count > 0) {
                var index = stack.Pop();
                if (!visited.Add(index))
                    continue;
                yield return _nodes[index];

                for (var i = _nodes.Length - 1; i >= 0; i--) {
                    if (_edges[index * _nodes.Length + i]) // Check adjacency matrix
                        stack.Push(i);
                }
            }
        }

        public IEnumerable<T> BreadthFirstSearch(T start)
        {
            var startIndex = Array.IndexOf(_nodes, start);
            if (startIndex == -1)
                yield break;

            var queue = new Queue<int>();
            queue.Enqueue(startIndex);
            var visited = new HashSet<int> { startIndex };

            while (queue.Count > 0) {
                var index = queue.Dequeue();
                yield return _nodes[index];

                for (var i = 0; i < _nodes.Length; i++) {
                    if (_edges[index * _nodes.Length + i] && visited.Add(i)) {
                        queue.Enqueue(i);
                    }
                }
            }
        }
    }
}
