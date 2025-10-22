using System;
using System.Collections.Generic;
using System.Numerics;

namespace BrightData
{
    /// <summary>
    /// Read only graph
    /// </summary>
    public interface IReadOnlyGraph : IHaveSize
    {
        /// <summary>
        /// Gets the set of connected nodes from the specified node index
        /// </summary>
        /// <param name="nodeIndex"></param>
        /// <returns></returns>
        IEnumerable<uint> GetConnectedNodes(uint nodeIndex);

        /// <summary>
        /// Returns connected node indices in depth first order
        /// </summary>
        /// <param name="startNodeIndex">Node index at which to start search</param>
        /// <returns></returns>
        IEnumerable<uint> DepthFirstSearch(uint startNodeIndex);

        /// <summary>
        /// Returns connected node indices in breadth first order
        /// </summary>
        /// <param name="startNodeIndex">Node index at which to start search</param>
        /// <returns></returns>
        IEnumerable<uint> BreadthFirstSearch(uint startNodeIndex);
    }

    /// <summary>
    /// Strongly typed read only graph
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReadOnlyGraph<T> : IReadOnlyGraph
        where T: unmanaged, IEquatable<T>
    {
        /// <summary>
        /// Gets the value of a node based on index
        /// </summary>
        /// <param name="nodeIndex"></param>
        /// <returns></returns>
        T Get(uint nodeIndex);

        /// <summary>
        /// Gets the node index of the specified node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        uint GetNodeIndex(T node);
    }

    /// <summary>
    /// Read only directed graph
    /// </summary>
    public interface IReadOnlyDirectedGraph : IReadOnlyGraph
    {
        /// <summary>
        /// Gets the number of vertices that connect to the specified node index
        /// </summary>
        /// <param name="nodeIndex">Node index</param>
        /// <returns></returns>
        uint GetInDegree(uint nodeIndex);

        /// <summary>
        /// Gets the number of vertices from the specified node index
        /// </summary>
        /// <param name="nodeIndex">Node index</param>
        /// <returns></returns>
        uint GetOutDegree(uint nodeIndex);

        /// <summary>
        /// Returns a valid ordering of the nodes in a DAG (or empty if there is a cycle in the graph)
        /// </summary>
        /// <returns></returns>
        IEnumerable<uint> TopologicalSort();
    }

    /// <summary>
    /// A directed graph that allows multiple edges between nodes
    /// </summary>
    public interface IReadOnlyMultiDirectedGraph<NT, ET> : IReadOnlyDirectedGraph, IReadOnlyGraph<NT>
        where NT : unmanaged, IEquatable<NT>
        where ET : unmanaged
    {
        /// <summary>
        /// Return the edges between two nodes
        /// </summary>
        /// <param name="fromNodeIndex"></param>
        /// <param name="toNodeIndex"></param>
        /// <returns></returns>
        List<(ET Data, uint Index)> GetEdges(uint fromNodeIndex, uint toNodeIndex);
    }

    /// <summary>
    /// Mutable graph
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMutableGraph<in T>
        where T : unmanaged, IEquatable<T>
    {
        /// <summary>
        /// Adds a node to the graph
        /// </summary>
        /// <param name="node">Node data to add</param>
        /// <returns>Node index</returns>
        uint Add(T node);

        /// <summary>
        /// Adds a vertex between two nodes
        /// </summary>
        /// <param name="fromNodeIndex"></param>
        /// <param name="toNodeIndex"></param>
        /// <returns></returns>
        bool AddEdge(uint fromNodeIndex, uint toNodeIndex);

        /// <summary>
        /// Clears all nodes and vertices from the graph
        /// </summary>
        void Clear();
    }

    /// <summary>
    /// Mutable graph with edges with an associated data type
    /// </summary>
    /// <typeparam name="NT">Node type</typeparam>
    /// <typeparam name="ET">Edge type</typeparam>
    public interface IMutableGraphWithEdgeData<in NT, in ET> : IMutableGraph<NT>
        where NT : unmanaged, IEquatable<NT>
        where ET : unmanaged
    {
        /// <summary>
        /// Adds a vertex between two nodes
        /// </summary>
        /// <param name="fromNodeIndex"></param>
        /// <param name="toNodeIndex"></param>
        /// <param name="edgeData"></param>
        /// <returns></returns>
        bool AddEdge(uint fromNodeIndex, uint toNodeIndex, ET edgeData);
    }

    /// <summary>
    /// A graph node
    /// </summary>
    public interface IUndirectedWeightedGraphNode : IHaveSingleIndex
    {
        /// <summary>
        /// Span of neighbour indices
        /// </summary>
        ReadOnlySpan<uint> NeighbourSpan { get; }

        /// <summary>
        /// Enumerable of neighbour indices
        /// </summary>
        IEnumerable<uint> Neighbours { get; }
    }

    /// <summary>
    /// A graph node with weighted neighbours
    /// </summary>
    /// <typeparam name="T">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight</typeparam>
    public interface IWeightedGraphNode<out T, W> : IUndirectedWeightedGraphNode
        where T: IHaveSingleIndex
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Graph node value
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// Tries to add a neighbour to this node
        /// </summary>
        /// <param name="index"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        bool TryAddNeighbour(uint index, W weight);

        /// <summary>
        /// Enumerates the neighbour indices and their weights
        /// </summary>
        IEnumerable<(uint Index, W Weight)> WeightedNeighbours { get; }
    }

    /// <summary>
    /// Calculates the weights between two indexed graph nodes
    /// </summary>
    /// <typeparam name="W"></typeparam>
    public interface ICalculateNodeWeights<out W>
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Returns the weight between any two graph nodes
        /// </summary>
        /// <param name="fromIndex">First node</param>
        /// <param name="toIndex">Second index</param>
        /// <returns></returns>
        W GetWeight(uint fromIndex, uint toIndex);
    }

    /// <summary>
    /// A graph with weighted connections
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="W"></typeparam>
    public interface IReadOnlyWeightedGraph<T, W> : IReadOnlyGraph<T>
        where T: unmanaged, IEquatable<T>, IHaveSingleIndex
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Performs a probabilistic search between two nodes
        /// </summary>
        /// <typeparam name="RAT">Return array type</typeparam>
        /// <typeparam name="CAT">Candidate array type</typeparam>
        /// <param name="q">Node index to query</param>
        /// <param name="entryPoint">Node index from which to start the search</param>
        /// <param name="distanceCalculator">Weigh calculator</param>
        /// <returns></returns>
        RAT ProbabilisticSearch<RAT, CAT>(uint q, uint entryPoint, ICalculateNodeWeights<W> distanceCalculator)
            where RAT : struct, IFixedSizeSortedArray<uint, W>
            where CAT : struct, IFixedSizeSortedArray<uint, W>
        ;

        /// <summary>
        /// Returns the neighbour indices of a node based on index
        /// </summary>
        /// <param name="nodeIndex"></param>
        /// <returns></returns>
        ReadOnlySpan<uint> GetNeighbours(uint nodeIndex);

        /// <summary>
        /// Enumerates the neighbours and their weights of a node based on index
        /// </summary>
        /// <param name="nodeIndex"></param>
        /// <returns></returns>
        IEnumerable<(uint NeighbourIndex, W Weight)> EnumerateNeighbours(uint nodeIndex);

        /// <summary>
        /// Adds a new neighbour to a node
        /// </summary>
        /// <param name="toNodeIndex">Node index to add to</param>
        /// <param name="neighbourIndex">Index of the neighbour node</param>
        /// <param name="weight">Weight of the connection</param>
        /// <returns></returns>
        bool AddNeighbour(uint toNodeIndex, uint neighbourIndex, W weight);
    }

    /// <summary>
    /// A graph with weighted connections that can be dynamically extended
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="W"></typeparam>
    public interface IWeightedGraph<T, W> : IReadOnlyWeightedGraph<T, W>
        where T : unmanaged, IEquatable<T>, IHaveSingleIndex
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Adds a new node
        /// </summary>
        /// <param name="value"></param>
        void Add(T value);

        /// <summary>
        /// Adds a new node and its neighbours
        /// </summary>
        /// <param name="value"></param>
        /// <param name="neighbours"></param>
        void Add(T value, ReadOnlySpan<(uint Index, W Weight)> neighbours);
    }
}
