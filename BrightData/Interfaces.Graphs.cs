using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace BrightData
{
    public interface IGraph : IHaveSize
    {

    }

    public interface IGraph<out T> : IGraph
        where T: unmanaged
    {
        /// <summary>
        /// Returns the value associated with the node
        /// </summary>
        /// <param name="nodeIndex"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        //bool TryGetValue(uint nodeIndex, [NotNullWhen(true)] out T? value);
    }

    public interface IBuildGraphs<T>
        where T: unmanaged
    {
        /// <summary>
        /// Add a new node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        uint Add(T node);

        /// <summary>
        /// Adds an edge between two nodes
        /// </summary>
        /// <param name="fromNodeIndex"></param>
        /// <param name="toNodeIndex"></param>
        /// <returns></returns>
        bool AddEdge(uint fromNodeIndex, uint toNodeIndex);


    }

    /// <summary>
    /// A graph node
    /// </summary>
    public interface IGraphNode : IHaveSingleIndex
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
    public interface IWeightedGraphNode<out T, W> : IGraphNode
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
    public interface IWeightedGraph<out T, W> : IGraph<T>
        where T: unmanaged, IHaveSingleIndex
        where W : unmanaged, INumber<W>, IMinMaxValue<W>
    {
        /// <summary>
        /// Finds the value of a node based on index
        /// </summary>
        /// <param name="nodeIndex"></param>
        /// <returns></returns>
        T Find(uint nodeIndex);

        /// <summary>
        /// Finds the value based on node position within the graph
        /// </summary>
        /// <param name="nodePosition"></param>
        /// <returns></returns>
        T this[uint nodePosition] { get; }

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
    public interface IWeightedDynamicGraph<T, W> : IWeightedGraph<T, W>
        where T : unmanaged, IHaveSingleIndex
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
