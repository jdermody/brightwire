using System;
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
    /// Typed data analyser
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDataAnalyser<T> : IAppendBlocks<T>, IDataAnalyser where T : notnull
    {
        /// <summary>
        /// Adds a typed object
        /// </summary>
        /// <param name="obj"></param>
        void Add(T obj);
    }

    /// <summary>
    /// Standard deviation analysis
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IStandardDeviationAnalysis<T> where T : unmanaged
    {
        /// <summary>
        /// Numerical mean
        /// </summary>
        T Mean { get; }

        /// <summary>
        /// Sample variance
        /// </summary>
        T? SampleVariance { get; }

        /// <summary>
        /// Population variance
        /// </summary>
        T? PopulationVariance { get; }

        /// <summary>
        /// Sample standard deviation
        /// </summary>
        T? SampleStdDev { get; }

        /// <summary>
        /// Population standard deviation
        /// </summary>
        T? PopulationStdDev { get; }

        /// <summary>
        /// Count of items that were analysed
        /// </summary>
        ulong Count { get; }
    }

    /// <summary>
    /// Numerical analysis
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface INumericAnalysis<T> : IStandardDeviationAnalysis<T>
        where T: unmanaged
    {
        /// <summary>
        /// L1 norm
        /// </summary>
        T L1Norm { get; }

        /// <summary>
        /// L2 norm
        /// </summary>
        T L2Norm { get; }

        /// <summary>
        /// Minimum number
        /// </summary>
        T Min { get; }

        /// <summary>
        /// Maximum number
        /// </summary>
        T Max { get; }

        /// <summary>
        /// Number of unique numbers
        /// </summary>
        uint NumDistinct { get; }

        /// <summary>
        /// Median
        /// </summary>
        T? Median { get; }

        /// <summary>
        /// Mode
        /// </summary>
        T? Mode { get; }
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
    /// Indicates that the type can convert string to string indices
    /// </summary>
    public interface IIndexStrings
    {
        /// <summary>
        /// Returns the index for a string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        uint GetIndex(string str);

        /// <summary>
        /// Gets the total number of possible string indices
        /// </summary>
        uint OutputSize { get; }
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
    /// Type of data specification
    /// </summary>
    public enum DataSpecificationType
    {
        /// <summary>
        /// Represents a field of data
        /// </summary>
        Field,

        /// <summary>
        /// Represents an item that holds a set of other items
        /// </summary>
        Composite,

        /// <summary>
        /// Represents a field that takes a value from one of a set of possibilities
        /// </summary>
        FieldSet
    }

    /// <summary>
    /// Data type specifications can validate a data source
    /// </summary>
    public interface IDataTypeSpecification
    {
        /// <summary>
        /// Name of this item
        /// </summary>
        string? Name { get; }

        /// <summary>
        /// Children of this item (optional)
        /// </summary>
        IDataTypeSpecification[]? Children { get; }

        /// <summary>
        /// Underlying .net type for this item
        /// </summary>
        Type UnderlyingType { get; }

        /// <summary>
        /// Item type
        /// </summary>
        DataSpecificationType SpecificationType { get; }

        /// <summary>
        /// True if the item can repeat
        /// </summary>
        bool CanRepeat { get; }
    }

    /// <summary>
    /// Typed data specification
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDataTypeSpecification<T> : IDataTypeSpecification where T: notnull
    {
        /// <summary>
        /// Checks if the typed instance is valid against the specification
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        bool IsValid(T instance);

        /// <summary>
        /// Adds a predicate that must match to be considered valid
        /// </summary>
        /// <param name="predicate"></param>
        void AddPredicate(Predicate<T> predicate);
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
    public record IndexListWithLabel<T>(T Label, IndexList Data);

    /// <summary>
    /// A weighted index list with a typed label
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="Label"></param>
    /// <param name="Data"></param>
    public record WeightedIndexListWithLabel<T>(T Label, WeightedIndexList Data);

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

    internal interface ISimpleNumericAnalysis : IOperation
    {
        public bool IsInteger { get; }
        public uint NanCount { get; }
        public uint InfinityCount { get; }
        public double MinValue { get; }
        public double MaxValue { get; }
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
    public interface IWeightedGraph<out T, W> : IHaveSize
        where T: IHaveSingleIndex
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
        where T: IHaveSingleIndex
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
