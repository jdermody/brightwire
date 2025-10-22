using BrightData.Types.Graph.Helper;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BrightData.Types.Graph
{
    /// <summary>
    /// Mutable directed graph
    /// </summary>
    /// <typeparam name="NT">Node type</typeparam>
    /// <typeparam name="ET">Edge type</typeparam>
    public readonly struct MultiDirectedGraph<NT, ET> : IMutableGraphWithEdgeData<NT, ET>, IReadOnlyMultiDirectedGraph<NT, ET>, IDisposable
        where NT : unmanaged, IEquatable<NT>
        where ET : unmanaged
    {
        readonly record struct Edge(uint FromNodeIndex, uint ToNodeIndex, ET Data);
        readonly ArrayPoolBufferWriter<NT> _nodes = new();
        readonly ArrayPoolBufferWriter<Edge> _edges = new();

        /// <summary>
        /// Default constructor
        /// </summary>
        public MultiDirectedGraph()
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
        public bool AddEdge(uint fromNodeIndex, uint toNodeIndex) => AddEdge(fromNodeIndex, toNodeIndex, default(ET));

        /// <inheritdoc />
        public bool AddEdge(uint fromNodeIndex, uint toNodeIndex, ET edge)
        {
            _edges.Write(new(fromNodeIndex, toNodeIndex, edge));
            return true;
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
        public IEnumerable<uint> TopologicalSort() => DirectedGraphHelper<MultiDirectedGraph<NT, ET>>.TopologicalSort(ref Unsafe.AsRef(in this));

        /// <inheritdoc />
        public void Clear()
        {
            _nodes.Clear();
            _edges.Clear();
        }

        /// <inheritdoc />
        public List<(ET Data, uint Index)> GetEdges(uint fromNodeIndex, uint toNodeIndex)
        {
            if (fromNodeIndex >= Size)
                throw new ArgumentOutOfRangeException(nameof(fromNodeIndex));
            if (toNodeIndex >= Size)
                throw new ArgumentOutOfRangeException(nameof(toNodeIndex));

            var index = 0U;
            var ret = new List<(ET Data, uint Index)>();
            foreach (var edge in _edges.WrittenSpan) {
                if (edge.FromNodeIndex == fromNodeIndex && edge.ToNodeIndex == toNodeIndex)
                    ret.Add((edge.Data, index));
                ++index;
            }
            return ret;
        }

        /// <inheritdoc />
        public IEnumerable<uint> GetConnectedNodes(uint nodeIndex)
        {
            if (nodeIndex >= Size)
                throw new ArgumentOutOfRangeException(nameof(nodeIndex));

            var ret = new List<uint>();
            var visited = new HashSet<uint>();
            foreach (var edge in _edges.WrittenSpan)
            {
                if (edge.FromNodeIndex == nodeIndex && visited.Add(edge.ToNodeIndex))
                    ret.Add(edge.ToNodeIndex);
            }
            return ret;
        }

        /// <inheritdoc />
        public IEnumerable<uint> DepthFirstSearch(uint startNodeIndex) => GraphHelper<MultiDirectedGraph<NT, ET>>.DepthFirstSearch(ref Unsafe.AsRef(in this), startNodeIndex);

        /// <inheritdoc />
        public IEnumerable<uint> BreadthFirstSearch(uint startNodeIndex) => GraphHelper<MultiDirectedGraph<NT, ET>>.BreadthFirstSearch(ref Unsafe.AsRef(in this), startNodeIndex);

        /// <inheritdoc />
        public NT Get(uint nodeIndex) => _nodes.WrittenSpan[(int)nodeIndex];

        /// <inheritdoc />
        public uint GetNodeIndex(NT node)
        {
            var ret = _nodes.WrittenSpan.IndexOf(node);
            if (ret >= 0)
                return (uint)ret;
            throw new ArgumentException("Node not found", nameof(node));
        }
    }
}
