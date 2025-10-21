using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BrightData.Types.Graph.Helper;

namespace BrightData.Types.Graph
{
    /// <summary>
    /// Directed graph implementation
    /// </summary>
    /// <typeparam name="NT">Node type</typeparam>
    /// <typeparam name="ET">Edge type</typeparam>
    public readonly struct DirectedGraph<NT, ET> : IDirectedGraph<NT, ET>, IDisposable
        where NT : unmanaged, IEquatable<NT>
        where ET : unmanaged
    {
        readonly record struct Edge(uint FromNodeIndex, uint ToNodeIndex, ET Data);
        readonly ArrayPoolBufferWriter<NT> _nodes = new();
        readonly ArrayPoolBufferWriter<Edge> _edges = new();

        /// <summary>
        /// Default constructor
        /// </summary>
        public DirectedGraph()
        {

        }

        /// <inheritdoc />
        public void Dispose()
        {
            _nodes.Dispose();
            _edges.Dispose();
        }

        /// <inheritdoc />
        public uint Size => (uint)_nodes.WrittenCount;

        /// <inheritdoc />
        public uint Add(NT node)
        {
            var ret = Size;
            _nodes.Write(node);
            return ret;
        }

        /// <inheritdoc />
        public uint AddEdge(uint fromNodeIndex, uint toNodeIndex, ET edge)
        {
            var ret = (uint)_edges.WrittenCount;
            _edges.Write(new(fromNodeIndex, toNodeIndex, edge));
            return ret;
        }

        /// <inheritdoc />
        public uint GetInDegree(uint nodeIndex)
        {
            uint ret = 0;
            foreach(ref readonly var edge in _edges.WrittenSpan)
                if (edge.ToNodeIndex == nodeIndex)
                    ++ret;
            return ret;
        }

        /// <inheritdoc />
        public uint GetOutDegree(uint nodeIndex)
        {
            uint ret = 0;
            foreach (ref readonly var edge in _edges.WrittenSpan)
                if (edge.FromNodeIndex == nodeIndex)
                    ++ret;
            return ret;
        }

        /// <inheritdoc />
        public IEnumerable<uint> TopologicalSort() => DirectedGraphHelper<DirectedGraph<NT, ET>, NT>.TopologicalSort(ref Unsafe.AsRef(in this));

        /// <inheritdoc />
        public void Clear()
        {
            _nodes.Clear();
            _edges.Clear();
        }

        public (ET Data, uint Index) GetEdge(uint fromNodeIndex, uint toNodeIndex)
        {
            var index = 0U;
            foreach (ref readonly var edge in _edges.WrittenSpan) {
                if (edge.FromNodeIndex == fromNodeIndex && edge.ToNodeIndex == toNodeIndex)
                    return (edge.Data, index);
                ++index;
            }
            throw new ArgumentException("Edge not found");
        }

        /// <inheritdoc />
        public IEnumerable<uint> GetConnectedNodes(uint nodeIndex)
        {
            if (nodeIndex >= Size)
                throw new ArgumentOutOfRangeException(nameof(nodeIndex));

            var list = new List<uint>();
            foreach (var edge in _edges.WrittenSpan)
            {
                if (edge.FromNodeIndex == nodeIndex)
                    list.Add(edge.ToNodeIndex);
            }

            return list;
        }

        /// <inheritdoc />
        public IEnumerable<uint> DepthFirstSearch(uint startNodeIndex) => GraphHelper<DirectedGraph<NT, ET>>.DepthFirstSearch(ref Unsafe.AsRef(in this), startNodeIndex);

        /// <inheritdoc />
        public IEnumerable<uint> BreadthFirstSearch(uint startNodeIndex) => GraphHelper<DirectedGraph<NT, ET>>.BreadthFirstSearch(ref Unsafe.AsRef(in this), startNodeIndex);

        /// <inheritdoc />
        public NT Get(uint nodeIndex) => _nodes.WrittenSpan[(int)nodeIndex];
    }
}
