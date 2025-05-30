﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using BrightData.Types;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData
{
    /// <summary>
    /// Indicates that the type has a data context
    /// </summary>
    public interface IHaveBrightDataContext
    {
        /// <summary>
        /// Bright data context
        /// </summary>
        BrightDataContext Context { get; }
    }

    /// <summary>
    /// Indicates that the type has a size
    /// </summary>
    public interface IHaveSize
    {
        /// <summary>
        /// Number of items
        /// </summary>
        uint Size { get; }
    }

    /// <summary>
    /// Indicates that the type has a list of indices
    /// </summary>
    public interface IHaveIndices
    {
        /// <summary>
        /// Enumerates the indices
        /// </summary>
        IEnumerable<uint> Indices { get; }
    }

    /// <summary>
    /// Returns the types data as a read only byte span
    /// </summary>
    public interface IHaveDataAsReadOnlyByteSpan
    {
        /// <summary>
        /// The data of the type as bytes
        /// </summary>
        ReadOnlySpan<byte> DataAsBytes { get; }
    }

    /// <summary>
    /// Indicates that the type can create a readonly span of floats
    /// </summary>
    public interface IHaveSpanOf<T>
    {
        /// <summary>
        /// Returns a span of floats
        /// </summary>
        /// <param name="temp">Optional buffer that might be needed when creating the span</param>
        /// <param name="wasTempUsed">True if the buffer was used</param>
        /// <returns>Span of floats</returns>
        ReadOnlySpan<T> GetSpan(ref SpanOwner<T> temp, out bool wasTempUsed);
    }

    /// <summary>
    /// Indicates that the type exposes its data as typed memory
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IHaveMemory<T>
    {
        /// <summary>
        /// The typed memory of the data in the type
        /// </summary>
        ReadOnlyMemory<T> ReadOnlyMemory { get; }
    }

    /// <summary>
    /// Indicates that the type can serialize to a binary writer
    /// </summary>
    public interface ICanWriteToBinaryWriter
    {
        /// <summary>
        /// Serialize to binary writer
        /// </summary>
        /// <param name="writer"></param>
        void WriteTo(BinaryWriter writer);
    }

    /// <summary>
    /// Indicates that the type can initialize from a binary reader
    /// </summary>
    public interface ICanInitializeFromBinaryReader
    {
        /// <summary>
        /// Initialize from a binary reader
        /// </summary>
        /// <param name="context">Bright data context</param>
        /// <param name="reader">Reader to read from to initialize</param>
        void Initialize(BrightDataContext context, BinaryReader reader);
    }

    /// <summary>
    /// Supports both writing and reading from binary
    /// </summary>
    public interface IAmSerializable : ICanWriteToBinaryWriter, ICanInitializeFromBinaryReader;

    /// <summary>
    /// Indicates that the type has a metadata store
    /// </summary>
    public interface IHaveMetaData
    {
        /// <summary>
        /// Meta data store
        /// </summary>
        MetaData MetaData { get; }
    }

    /// <summary>
    /// Indicates that the type can write values to metadata
    /// </summary>
    public interface IWriteToMetaData
    {
        /// <summary>
        /// Writes values to meta data
        /// </summary>
        /// <param name="metadata">Meta data store</param>
        void WriteTo(MetaData metadata);
    }

    /// <summary>
    /// Base data analyzer type
    /// </summary>
    public interface IDataAnalyser : IWriteToMetaData, IAppendBlocks
    {
        /// <summary>
        /// Adds an object to analyze
        /// </summary>
        /// <param name="obj"></param>
        void AddObject(object obj);
    }

    /// <summary>
    /// Appends blocks of items
    /// </summary>
    public interface IAppendBlocks;

    /// <summary>
    /// Appends blocks of typed items
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAppendBlocks<T> : IAppendBlocks
    {
        /// <summary>
        /// Add a new block
        /// </summary>
        /// <param name="block"></param>
        void Append(ReadOnlySpan<T> block);
    }

    

    /// <summary>
    /// Types of data normalization
    /// </summary>
    public enum NormalizationType : byte
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,

        /// <summary>
        /// Standard deviation
        /// </summary>
        Standard,

        /// <summary>
        /// Euclidean norm
        /// </summary>
        Euclidean,

        /// <summary>
        /// Manhattan
        /// </summary>
        Manhattan,

        /// <summary>
        /// Between 0..1
        /// </summary>
        FeatureScale
    }

    /// <summary>
    /// Indicates that the type can convert different types
    /// </summary>
    public interface ICanConvert
    {
        /// <summary>
        /// Type that is converted from
        /// </summary>
        Type From { get; }

        /// <summary>
        /// Type that is converted to
        /// </summary>
        Type To { get; }
    }

    /// <summary>
    /// Typed converter interface
    /// </summary>
    /// <typeparam name="TF"></typeparam>
    /// <typeparam name="TT"></typeparam>
    public interface ICanConvert<in TF, out TT> : ICanConvert
        where TF : notnull
        where TT : notnull
    {
        /// <summary>
        /// Converts a type from one to another
        /// </summary>
        /// <param name="data">Object to convert</param>
        /// <returns></returns>
        TT Convert(TF data);
    }

    /// <summary>
    /// Data normalizer
    /// </summary>
    public interface INormalize
    {
        /// <summary>
        /// Type of data normalization
        /// </summary>
        NormalizationType NormalizationType { get; }

        /// <summary>
        /// Value to divide each value
        /// </summary>
        double Divide { get; }

        /// <summary>
        /// Value to subtract from each value
        /// </summary>
        double Subtract { get; }
    }

    /// <summary>
    /// Types of aggregations
    /// </summary>
    public enum AggregationType
    {
        /// <summary>
        /// Sums values to a final value
        /// </summary>
        Sum,

        /// <summary>
        /// Averages values
        /// </summary>
        Average,

        /// <summary>
        /// Finds the maximum value
        /// </summary>
        Max,

        /// <summary>
        /// Finds the minimum value
        /// </summary>
        Min
    }

    /// <summary>
    /// Data distribution
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDistribution<out T>
        where T : unmanaged, INumber<T>
    {
        /// <summary>
        /// Samples a value from the distribution
        /// </summary>
        /// <returns></returns>
        T Sample();
    }

    /// <summary>
    /// Discrete data distribution
    /// </summary>
    public interface IDiscreteDistribution : IDistribution<int>;

    /// <summary>
    /// Positive discrete data distribution
    /// </summary>
    public interface INonNegativeDiscreteDistribution : IDistribution<uint>;

    /// <summary>
    /// Continuous data distribution
    /// </summary>
    public interface IContinuousDistribution<out T> : IDistribution<T> where T : unmanaged, INumber<T>, IBinaryFloatingPointIeee754<T>;

    /// <summary>
    /// Indicates that the type can convert string to string indices
    /// </summary>
    public interface IIndexStrings : IHaveSize
    {
        /// <summary>
        /// Returns the index for a string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        uint GetIndex(ReadOnlySpan<char> str);

        /// <summary>
        /// Gets the total number of possible string indices
        /// </summary>
        [Obsolete("Use Size instead")]public uint OutputSize => Size;

        /// <summary>
        /// Returns the list of strings, ordered by string index
        /// </summary>
        IEnumerable<string> OrderedStrings { get; }
    }

    /// <summary>
    /// Indicates that the type has string indexer
    /// </summary>
    public interface IHaveStringIndexer
    {
        /// <summary>
        /// String indexer
        /// </summary>
        IIndexStrings? Indexer { get; }
    }

    /// <summary>
    /// In memory string table
    /// </summary>
    public interface IStringTableInMemory : IHaveSize
    {
        /// <summary>
        /// Gets a string as utf-8
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        ReadOnlySpan<byte> GetUtf8(uint index);

        /// <summary>
        /// Gets a string by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        string GetString(uint index);

        /// <summary>
        /// Gets all strings
        /// </summary>
        /// <param name="maxStringSize">Max string size</param>
        /// <returns></returns>
        string[] GetAll(int maxStringSize = 1024);
    }

    /// <summary>
    /// Notifies of operations and messages
    /// </summary>
    public interface INotifyOperationProgress
    {
        /// <summary>
        /// Called at the start of an operation
        /// </summary>
        /// <param name="operationId">Unique id for this operation</param>
        /// <param name="msg">Optional message associated with the operation</param>
        void OnStartOperation(Guid operationId, string? msg = null);

        /// <summary>
        /// Called when the operation has progressed
        /// </summary>
        /// <param name="operationId">Unique id for this operation</param>
        /// <param name="progressPercent">Progress percentage (between 0 and 1)</param>
        void OnOperationProgress(Guid operationId, float progressPercent);

        /// <summary>
        /// Called when the operation has completed
        /// </summary>
        /// <param name="operationId">Unique id for this operation</param>
        /// <param name="wasCancelled">True if the operation was cancelled</param>
        void OnCompleteOperation(Guid operationId, bool wasCancelled);

        /// <summary>
        /// Called to notify about a message
        /// </summary>
        /// <param name="msg">Message to user</param>
        void OnMessage(string msg);
    }

    /// <summary>
    /// Reference counter
    /// </summary>
    public interface ICountReferences
    {
        /// <summary>
        /// Adds a reference
        /// </summary>
        /// <returns></returns>
        int AddRef();

        /// <summary>
        /// Removes a reference (and might release the data)
        /// </summary>
        /// <returns></returns>
        int Release();

        /// <summary>
        /// Checks if there is still a valid reference count (and that the data has not been released)
        /// </summary>
        bool IsValid { get; }
    }

    /// <summary>
    /// Indicates that the type can randomly access untyped data
    /// </summary>
    public interface ICanRandomlyAccessData : IDisposable, IHaveSize
    {
        /// <summary>
        /// Returns the untyped item at this index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        object this[int index] { get; }

        /// <summary>
        /// Returns the untyped item at this index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        object this[uint index] { get; }
    }

    /// <summary>
    /// An index list with a typed label
    /// </summary>
    /// <param name="Label"></param>
    /// <param name="Data"></param>
    /// <typeparam name="T"></typeparam>
    public readonly record struct IndexListWithLabel<T>(T Label, IndexList Data);

    /// <summary>
    /// A weighted index list with a typed label
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="Label"></param>
    /// <param name="Data"></param>
    public readonly record struct WeightedIndexListWithLabel<T>(T Label, WeightedIndexList Data);

    /// <summary>
    /// A vector clustering strategy
    /// </summary>
    public interface IClusteringStrategy
    {
        /// <summary>
        /// Finds clusters from the array of vectors
        /// </summary>
        /// <param name="vectors"></param>
        /// <param name="numClusters"></param>
        /// <param name="metric"></param>
        /// <returns></returns>
        uint[][] Cluster(IReadOnlyVector<float>[] vectors, uint numClusters, DistanceMetric metric);
    }

    internal interface IMutableBufferBlock<T>
    {
        uint Size { get; }
        Task<uint> WriteTo(IByteBlockSource file);
        bool HasFreeCapacity { get; }
        ReadOnlySpan<T> WrittenSpan { get; }
        ReadOnlyMemory<T> WrittenMemory { get; }
        ref T GetNext();
    }

    /// <summary>
    /// Fixed size sorted array of values and weights
    /// </summary>
    /// <typeparam name="V">Type of value to store</typeparam>
    /// <typeparam name="W">Type of weight that will be used to sort</typeparam>
    public interface IFixedSizeSortedArray<V, W> : IHaveSize
    {
        /// <summary>
        /// Maximum number of elements that can fit in the array
        /// </summary>
        byte MaxSize { get; }

        /// <summary>
        /// Current number of elements
        /// </summary>
        new byte Size { get; }
        uint IHaveSize.Size => Size;

        /// <summary>
        /// Sorted list of values
        /// </summary>
        ReadOnlySpan<V> Values { get; }

        /// <summary>
        /// Sorted list of weights
        /// </summary>
        ReadOnlySpan<W> Weights { get; }

        /// <summary>
        /// The smallest weight
        /// </summary>
        W MinWeight { get; }

        /// <summary>
        /// The largest weight
        /// </summary>
        W MaxWeight { get; }

        /// <summary>
        /// The value with the smallest weight
        /// </summary>
        V? MinValue { get; }

        /// <summary>
        /// The value with the largest weight
        /// </summary>
        V? MaxValue { get; }

        /// <summary>
        /// Returns a value and weight
        /// </summary>
        /// <param name="index">Index to return</param>
        (V Value, W Weight) this[uint index] { get; }

        /// <summary>
        /// Enumerates the values and weights
        /// </summary>
        IEnumerable<(V Value, W Weight)> Elements { get; }

        /// <summary>
        /// Tries to add a new element - will succeed if there aren't already max elements with a smaller weight
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="weight">Weight to add</param>
        /// <param name="enforceUnique">True if values should be unique - will return false if the value already exists</param>
        /// <returns>True if the element was added</returns>
        bool TryAdd(V value, W weight, bool enforceUnique = true);

        /// <summary>
        /// Removes an element from the array
        /// </summary>
        /// <param name="index">Index of element to remove</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        V RemoveAt(byte index);
    }

    /// <summary>
    /// String index storage type
    /// </summary>
    public enum StringIndexType
    {
        /// <summary>
        /// Store strings in a dictionary
        /// </summary>
        Dictionary,

        /// <summary>
        /// Store strings in a trie
        /// </summary>
        Trie
    }

    /// <summary>
    /// Callback for each span in a collection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="span"></param>
    /// <param name="index"></param>
    public delegate void ForEachSpanCallback<T>(ReadOnlySpan<T> span, int index);

    /// <summary>
    /// A tuple of spans
    /// </summary>
    public interface IAmTupleOfSpans
    {
        /// <summary>
        /// Size of each of the spans in the tuple
        /// </summary>
        int[] Sizes { get; }

        /// <summary>
        /// Byte size of each of the spans in the tuple
        /// </summary>
        int[] ByteSizes { get; }

        /// <summary>
        /// Invokes a callback on each span in the tuple
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callback"></param>
        void ForEach<T>(ForEachSpanCallback<T> callback) where T : unmanaged;
    }

    /// <summary>
    /// Indicates that the type has an offset into a buffer
    /// </summary>
    public interface IHaveOffset
    {
        /// <summary>
        /// Offset into a buffer
        /// </summary>
        uint Offset { get; }
    }

    /// <summary>
    /// Array of bits
    /// </summary>
    public interface IBitVector : IHaveSize, IHaveDataAsReadOnlyByteSpan
    {
        /// <summary>
        /// Gets the value of a bit within the vector
        /// </summary>
        /// <param name="bitIndex">Index of the bit</param>
        /// <returns></returns>
        bool this[int bitIndex] { get; }

        /// <summary>
        /// Underlying data
        /// </summary>
        /// <returns></returns>
        ReadOnlySpan<ulong> AsSpan();
    }
}
