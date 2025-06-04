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
    public readonly record struct SparseGraph<T> : IGraph<T>, IHaveDataAsReadOnlyByteSpan
        where T : unmanaged
    {
        const int HeaderSize = 8;

        internal readonly record struct Node(T Value, int EdgeIndex, int EdgeCount);
        readonly Node[] _nodes;
        readonly uint[] _edges;

        /// <summary>
        /// Creates a directed graph from graph data
        /// </summary>
        /// <param name="data"></param>
        public SparseGraph(ReadOnlySpan<byte> data)
        {
            var header = MemoryMarshal.Cast<byte, uint>(data[..HeaderSize]);
            _nodes = MemoryMarshal.Cast<byte, Node>(data[HeaderSize..(int)header[0]]).ToArray();
            _edges = MemoryMarshal.Cast<byte, uint>(data[(int)header[0]..(int)header[1]]).ToArray();
        }

        internal SparseGraph(ReadOnlySpan<Node> nodes, ReadOnlySpan<uint> edges)
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

        public IEnumerable<T> DepthFirstSearch(T start)
        {
            var startIndex = Array.FindIndex(_nodes, n => EqualityComparer<T>.Default.Equals(n.Value, start));
            if (startIndex == -1) 
                yield break;

            var stack = new Stack<int>();
            stack.Push(startIndex);

            var visited = new HashSet<int>();
            while (stack.Count > 0) {
                var index = stack.Pop();
                if (!visited.Add(index)) 
                    continue;
                yield return _nodes[index].Value;

                for (int edgeIndex = _nodes[index].EdgeIndex, i = edgeIndex + _nodes[index].EdgeCount - 1; i >= edgeIndex; i--) {
                    stack.Push((int)_edges[i]);
                }
            }
        }

        public IEnumerable<T> BreadthFirstSearch(T start)
        {
            var queue = new Queue<int>();
            var visited = new HashSet<int>();
            var startIndex = Array.FindIndex(_nodes, n => EqualityComparer<T>.Default.Equals(n.Value, start));
            if (startIndex == -1) yield break;

            queue.Enqueue(startIndex);
            visited.Add(startIndex);

            while (queue.Count > 0) {
                var index = queue.Dequeue();
                yield return _nodes[index].Value;

                for (var i = _nodes[index].EdgeIndex; i < _nodes[index].EdgeIndex + _nodes[index].EdgeCount; i++) {
                    var neighbor = (int)_edges[i];
                    if (visited.Add(neighbor)) {
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }
    }
}
