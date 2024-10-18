using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;

namespace BrightData
{
    /// <summary>
    /// Represents how vectors should be indexed
    /// </summary>
    public enum VectorIndexStrategy
    {
        /// <summary>
        /// Flat indexing (naive, brute force approach)
        /// </summary>
        Flat,

        /// <summary>
        /// Vectors are randomly projected into a random lower dimensional space
        /// </summary>
        RandomProjection,

        /// <summary>
        /// A nearest neighbour graph is created to improve index performance
        /// </summary>
        HierarchicalNavigableSmallWorld 
    }

    /// <summary>
    /// Determines how vectors are stored
    /// </summary>
    public enum VectorStorageType
    {
        /// <summary>
        /// Vectors are stored in memory
        /// </summary>
        InMemory
    }

    /// <summary>
    /// Responsible for storing vectors
    /// </summary>
    public interface IStoreVectors : IHaveSize
    {
        /// <summary>
        /// Storage type
        /// </summary>
        VectorStorageType StorageType { get; }

        /// <summary>
        /// Size of each vector (fixed)
        /// </summary>
        uint VectorSize { get; }
    }

    /// <summary>
    /// Callback for indexed spans
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public delegate void IndexedSpanCallback<T>(ReadOnlySpan<T> span) where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>; 

    /// <summary>
    /// Callback for indexed spans
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public delegate void IndexedSpanCallbackWithVectorIndex<T>(ReadOnlySpan<T> span, uint vectorIndex) where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>; 

    /// <summary>
    /// Callback for indexed spans
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public delegate void IndexedSpanCallbackWithVectorIndexAndRelativeIndex<T>(ReadOnlySpan<T> span, uint vectorIndex, uint relativeIndex) where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>; 

    /// <summary>
    /// Stores typed vectors
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReadOnlyVectorStore<T> : IStoreVectors, IDisposable
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        /// <summary>
        /// Returns a segment at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        ReadOnlySpan<T> this[uint index] { get; }

        /// <summary>
        /// Passes each vector to the callback, possible in parallel
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="ct"></param>
        void ForEach(IndexedSpanCallbackWithVectorIndex<T> callback, CancellationToken ct = default);

        /// <summary>
        /// Passes each vector to the callback, possible in parallel
        /// </summary>
        /// <param name="indices"></param>
        /// <param name="callback"></param>
        void ForEach(ReadOnlySpan<uint> indices, IndexedSpanCallbackWithVectorIndexAndRelativeIndex<T> callback);

        /// <summary>
        /// Returns all vectors
        /// </summary>
        /// <returns></returns>
        ReadOnlyMemory<T>[] GetAll();
    }

    /// <summary>
    /// Stores typed vectors
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IStoreVectors<T> : IReadOnlyVectorStore<T>
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        /// <summary>
        /// Adds a vector
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        uint Add(ReadOnlySpan<T> vector);
    }

    /// <summary>
    /// A vector set index
    /// </summary>
    public interface IVectorIndex<T> : IDisposable 
        where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        /// <summary>
        /// The vector storage for the index
        /// </summary>
        IStoreVectors<T> Storage { get; } 

        /// <summary>
        /// Adds a vector to the index
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        uint Add(ReadOnlySpan<T> vector);

        /// <summary>
        /// Returns a list of vector indices ranked by the distance between that vector and a comparison vector
        /// </summary>
        /// <param name="vector">Vector to compare</param>
        /// <returns></returns>
        IEnumerable<uint> Rank(ReadOnlySpan<T> vector);

        /// <summary>
        /// Returns the index of the closest vector in the set to each of the supplied vectors
        /// </summary>
        /// <param name="vector">Vectors to compare</param>
        /// <returns></returns>
        uint[] Closest(ReadOnlyMemory<T>[] vector);
    }

    /// <summary>
    /// Indicates that the type supports K Nearest Neighbour Search
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISupportKnnSearch<T>
    {
        /// <summary>
        /// Searches for N best results relative to a query vector
        /// </summary>
        /// <param name="query">Query vector</param>
        /// <typeparam name="AT"></typeparam>
        /// <returns></returns>
        public AT KnnSearch<AT>(ReadOnlySpan<T> query) where AT : IFixedSizeSortedArray<uint, T>, new();
    }
}
