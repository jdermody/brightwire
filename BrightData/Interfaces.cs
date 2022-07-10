using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using BrightData.LinearAlgebra;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData
{
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

    public interface IHaveSpan
    {
        ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed);
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
    public interface ISerializable : ICanWriteToBinaryWriter, ICanInitializeFromBinaryReader
    {
    }

    /// <summary>
    /// Unstructured meta data store
    /// </summary>
    public interface IMetaData : ICanWriteToBinaryWriter
    {
        /// <summary>
        /// Returns a value
        /// </summary>
        /// <param name="name">Name of the value</param>
        /// <returns></returns>
        object? Get(string name);

        /// <summary>
        /// Returns a typed nullable value
        /// </summary>
        /// <param name="name">Name of the value</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T? GetNullable<T>(string name) where T : struct;

        /// <summary>
        /// Returns a typed value
        /// </summary>
        /// <param name="name">Name of the value</param>
        /// <param name="valueIfMissing">Value to return if the value has not been set</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Get<T>(string name, T valueIfMissing) where T : IConvertible;

        /// <summary>
        /// Returns an existing value (throws if not found)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">Name of the value</param>
        /// <returns></returns>
        T Get<T>(string name) where T : IConvertible;

        /// <summary>
        /// Sets a named value
        /// </summary>
        /// <param name="name">Name of the value</param>
        /// <param name="value">Value</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Set<T>(string name, T value) where T : IConvertible;

        /// <summary>
        /// XML representation of the meta data
        /// </summary>
        string AsXml { get; }

        /// <summary>
        /// Copies this to another meta data store
        /// </summary>
        /// <param name="metadata">Other meta data store</param>
        void CopyTo(IMetaData metadata);

        /// <summary>
        /// Copies the specified values to another meta data store
        /// </summary>
        /// <param name="metadata">Other meta data store</param>
        /// <param name="keys">Values to copy</param>
        void CopyTo(IMetaData metadata, params string[] keys);

        /// <summary>
        /// Copies all except for the specified values to another meta data store
        /// </summary>
        /// <param name="metadata">Other meta data store</param>
        /// <param name="keys">Values NOT to copy (i.e. skip)</param>
        void CopyAllExcept(IMetaData metadata, params string[] keys);

        /// <summary>
        /// Reads values from a binary reader
        /// </summary>
        /// <param name="reader"></param>
        void ReadFrom(BinaryReader reader);

        /// <summary>
        /// Returns all value names with the specified prefix
        /// </summary>
        /// <param name="prefix">Prefix to query</param>
        /// <returns></returns>
        IEnumerable<string> GetStringsWithPrefix(string prefix);

        /// <summary>
        /// Checks if a value has been set
        /// </summary>
        /// <param name="key">Name of the value</param>
        /// <returns></returns>
        bool Has(string key);

        /// <summary>
        /// Removes a value
        /// </summary>
        /// <param name="key">Name of the value</param>
        void Remove(string key);

        /// <summary>
        /// Returns all keys that have been set
        /// </summary>
        IEnumerable<string> AllKeys { get; }

        /// <summary>
        /// Creates a clone of the current metadata
        /// </summary>
        IMetaData Clone();
    }

    /// <summary>
    /// Indicates that the type has a meta data store
    /// </summary>
    public interface IHaveMetaData
    {
        /// <summary>
        /// Meta data store
        /// </summary>
        IMetaData MetaData { get; }
    }

    /// <summary>
    /// Reference counted memory block
    /// </summary>
    public interface IReferenceCountedMemory
    {
        /// <summary>
        /// Size of the memory block
        /// </summary>
        uint Size { get; }

        /// <summary>
        /// Adds a reference
        /// </summary>
        /// <returns>Current number of references</returns>
        int AddRef();

        /// <summary>
        /// Removes a reference
        /// </summary>
        /// <returns>Current number of references</returns>
        int Release();

        /// <summary>
        /// Returns this block's allocation index
        /// </summary>
        long AllocationIndex { get; }

        /// <summary>
        /// Checks if the block is valid
        /// </summary>
        bool IsValid { get; }
    }

    /// <summary>
    /// Tensor base interface
    /// </summary>
    public interface ITensor : IDisposable, ISerializable, IHaveDataContext
    {
        /// <summary>
        /// The shape of the tensor (array of each dimension)
        /// </summary>
        uint[] Shape { get; }

        /// <summary>
        /// Total number of elements in tensor
        /// </summary>
        uint Size { get; }

        /// <summary>
        /// Size of the Shape array
        /// </summary>
        uint Rank { get; }

        /// <summary>
        /// True if the underlying memory has been properly scoped
        /// </summary>
        bool IsValid { get; }
    }

    /// <summary>
    /// Typed data reader
    /// </summary>
    public interface IDataReader
    {
        /// <summary>
        /// Reads a typed value from a binary reader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        T Read<T>(BinaryReader reader) where T : notnull;

        /// <summary>
        /// Reads a typed array from a binary reader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        T[] ReadArray<T>(BinaryReader reader) where T : notnull;
    }

    /// <summary>
    /// A memory pool of tensors
    /// </summary>
    public interface ITensorPool
    {
        /// <summary>
        /// Returns an existing cached array if available or allocates a new array otherwise
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="size">Size of the tensor to allocate</param>
        /// <returns></returns>
        MemoryOwner<T> Get<T>(uint size) where T : struct;

        /// <summary>
        /// Indicates that this array can be reused
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="block"></param>
        //void Reuse<T>(T[] block) where T : struct;

        /// <summary>
        /// Maximum size to cache for reusable arrays
        /// </summary>
        //long MaxCacheSize { get; }

        /// <summary>
        /// Current size of cached arrays
        /// </summary>
        //long CacheSize { get; }

#if DEBUG
        /// <summary>
        /// Register the reference counted memory block for debug and tracing
        /// </summary>
        /// <param name="block"></param>
        /// <param name="size"></param>
        void Register(IReferenceCountedMemory block, uint size);

        /// <summary>
        /// Unregister the reference counted memory block for debug and tracing
        /// </summary>
        /// <param name="block"></param>
        void UnRegister(IReferenceCountedMemory block);
#endif
    }

    /// <summary>
    /// Collects a list of disposable objects that can all be disposed when the layer is disposed
    /// </summary>
    public interface IDisposableLayers
    {
        /// <summary>
        /// Adds a new disposable object
        /// </summary>
        /// <param name="disposable"></param>
        void Add(IDisposable disposable);

        /// <summary>
        /// Creates a new layer to add disposable objects to
        /// </summary>
        /// <returns></returns>
        IDisposable Push();

        /// <summary>
        /// Disposes all objects in the top layer and removes this layer
        /// </summary>
        void Pop();
    }

    /// <summary>
    /// Indicates that the type can set a linear algebra provider
    /// </summary>
    public interface ISetLinearAlgebraProvider
    {
        /// <summary>
        /// Linear algebra provider
        /// </summary>
        LinearAlgebraProvider LinearAlgebraProvider2 { set; }
    }

    /// <summary>
    /// Gives access to a linear algebra provider
    /// </summary>
    public interface IHaveLinearAlgebraProvider
    {
        /// <summary>
        /// Linear algebra provider
        /// </summary>
        LinearAlgebraProvider LinearAlgebraProvider2 { get; }
    }

    /// <summary>
    /// Bright data context
    /// </summary>
    //public interface BrightDataContext : IDisposable, IHaveLinearAlgebraProvider
    //{
    //    /// <summary>
    //    /// Random number generator
    //    /// </summary>
    //    Random Random { get; }

    //    /// <summary>
    //    /// Tensor pool
    //    /// </summary>
    //    ITensorPool TensorPool { get; }

    //    /// <summary>
    //    /// Disposable memory layers
    //    /// </summary>
    //    IDisposableLayers MemoryLayer { get; }

    //    /// <summary>
    //    /// Data reader
    //    /// </summary>
    //    IDataReader DataReader { get; }

    //    /// <summary>
    //    /// Creates a new temp stream provider
    //    /// </summary>
    //    IProvideTempStreams CreateTempStreamProvider();

    //    LinearAlgebraProvider LinearAlgebraProvider2 { get; }

    //    /// <summary>
    //    /// Returns transient, context specific meta data
    //    /// </summary>
    //    /// <typeparam name="T"></typeparam>
    //    /// <param name="name">Name of value</param>
    //    /// <param name="defaultValue">Default value if not already set</param>
    //    /// <returns></returns>
    //    T Get<T>(string name, T defaultValue) where T : notnull;

    //    /// <summary>
    //    /// Returns transient, context specific meta data
    //    /// </summary>
    //    /// <typeparam name="T"></typeparam>
    //    /// <param name="name">Name of value</param>
    //    /// <param name="defaultValueCreator">Returns a default value if not already set</param>
    //    /// <returns></returns>
    //    T Get<T>(string name, Func<T> defaultValueCreator) where T : notnull;

    //    /// <summary>
    //    /// Returns optional context specific meta data
    //    /// </summary>
    //    /// <typeparam name="T"></typeparam>
    //    /// <param name="name">Name of value</param>
    //    /// <returns></returns>
    //    T? Get<T>(string name) where T : class;

    //    /// <summary>
    //    /// Sets transient, context specific meta data
    //    /// </summary>
    //    /// <typeparam name="T"></typeparam>
    //    /// <param name="name">Name of value</param>
    //    /// <param name="value">Value</param>
    //    /// <returns></returns>
    //    T Set<T>(string name, T value) where T : notnull;

    //    /// <summary>
    //    /// Sets transient, context specific meta data
    //    /// </summary>
    //    /// <typeparam name="T"></typeparam>
    //    /// <param name="name">Name of value</param>
    //    /// <param name="valueCreator">Function that will create value to set on demand if not already set</param>
    //    /// <returns></returns>
    //    T Set<T>(string name, Func<T> valueCreator) where T : notnull;

    //    /// <summary>
    //    /// True if random generator has been initialized with a random initial seed
    //    /// </summary>
    //    bool IsStochastic { get; }

    //    /// <summary>
    //    /// Resets the random number generator
    //    /// </summary>
    //    /// <param name="seed">Random seed (or null to randomly initialize)</param>
    //    public void ResetRandom(int? seed);

    //    /// <summary>
    //    /// Progress notifications for long running operations
    //    /// </summary>
    //    public INotifyUser? UserNotifications { get; set; }

    //    /// <summary>
    //    /// Cancellation token for the current context
    //    /// </summary>
    //    CancellationToken CancellationToken { get; }
    //}

    /// <summary>
    /// Indicates that the type has a data context
    /// </summary>
    public interface IHaveDataContext
    {
        /// <summary>
        /// Bright data context
        /// </summary>
        BrightDataContext Context { get; }
    }

    /// <summary>
    /// Typed indexable data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITensorSegment<T> : IReferenceCountedMemory, IDisposable, IHaveDataContext
        where T : struct
    {
        /// <summary>
        /// True if the data values are contiguous in memory
        /// </summary>
        bool IsContiguous { get; }

        /// <summary>
        /// Returns a value at an index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        T this[uint index] { get; set; }

        /// <summary>
        /// Returns a value at an index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        T this[long index] { get; set; }

        /// <summary>
        /// Clones the data into a new managed block
        /// </summary>
        /// <returns></returns>
        MemoryOwner<T> Clone();

        /// <summary>
        /// All values
        /// </summary>
        IEnumerable<T> Values { get; }

        /// <summary>
        /// Initialize the segment from a stream
        /// </summary>
        /// <param name="stream"></param>
        void InitializeFrom(Stream stream);

        /// <summary>
        /// Initialize the segment from a callback
        /// </summary>
        /// <param name="initializer">Functions that returns values for each indexed value</param>
        void Initialize(Func<uint, T> initializer);

        /// <summary>
        /// Initialize the segment to a single value
        /// </summary>
        /// <param name="initialValue">Initial value</param>
        void Initialize(T initialValue);

        /// <summary>
        /// Initialize from an array
        /// </summary>
        /// <param name="initialData"></param>
        void Initialize(Span<T> initialData);

        /// <summary>
        /// Writes to a stream
        /// </summary>
        /// <param name="writerBaseStream"></param>
        void WriteTo(Stream writerBaseStream);

        /// <summary>
        /// Copies values from this segment to an existing array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="sourceIndex">Index to start copying from</param>
        /// <param name="count">Number of values to copy</param>
        void CopyTo(Span<T> array, uint sourceIndex = 0, uint count = uint.MaxValue);

        /// <summary>
        /// Copies all values to another segment
        /// </summary>
        /// <param name="segment"></param>
        void CopyTo(ITensorSegment<T> segment);

        /// <summary>
        /// Returns part of the segment as a numerics vector
        /// </summary>
        System.Numerics.Vector<T> AsNumericsVector(int start);

        /// <summary>
        /// Returns an array that is only valid in local scope
        /// </summary>
        /// <returns></returns>
        T[] GetArrayForLocalUseOnly();
    }

    //public interface ITensorComputation<T>
    //    where T : struct
    //{
    //    Matrix<T> Transpose(Matrix<T> m);
    //    Matrix<T> Multiply(Matrix<T> m1, Matrix<T> m2);
    //    Matrix<T> TransposeAndMultiply(Matrix<T> m1, Matrix<T> m2);
    //    Matrix<T> TransposeThisAndMultiply(Matrix<T> m1, Matrix<T> m2);
    //    Vector<T> RowSums(Matrix<T> m);
    //    Vector<T> ColumnSums(Matrix<T> m);
    //    void AddToEachRowInPlace(Matrix<T> m, Vector<T> v);
    //    void AddToEachColumnInPlace(Matrix<T> m, Vector<T> v);
    //    Matrix<T> ConcatRows(Matrix<T> left, Matrix<T> right);
    //    Matrix<T> ConcatColumns(Matrix<T> top, Matrix<T> bottom);
    //}

    /// <summary>
    /// Indicates that the type can write values to meta data
    /// </summary>
    public interface IWriteToMetaData
    {
        /// <summary>
        /// Writes values to meta data
        /// </summary>
        /// <param name="metadata">Meta data store</param>
        void WriteTo(IMetaData metadata);
    }

    /// <summary>
    /// Base data analyzer type
    /// </summary>
    public interface IDataAnalyser : IWriteToMetaData
    {
        /// <summary>
        /// Adds an object to analyze
        /// </summary>
        /// <param name="obj"></param>
        void AddObject(object obj);
    }

    /// <summary>
    /// Typed data can be sequentially added
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAcceptSequentialTypedData<in T> where T : notnull
    {
        /// <summary>
        /// Adds a typed object
        /// </summary>
        /// <param name="obj"></param>
        void Add(T obj);
    }

    /// <summary>
    /// Typed data analyser
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDataAnalyser<in T> : IAcceptSequentialTypedData<T>, IDataAnalyser where T : notnull
    {
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
        Max
    }

    /// <summary>
    /// Data distribution
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDistribution<out T>
        where T : struct
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
    public interface IDiscreteDistribution : IDistribution<int>
    {
    }

    /// <summary>
    /// Positive discrete data distribution
    /// </summary>
    public interface INonNegativeDiscreteDistribution : IDistribution<uint>
    {
    }

    /// <summary>
    /// Continuous data distribution
    /// </summary>
    public interface IContinuousDistribution : IDistribution<float>
    {
    }

    /// <summary>
    /// Temp stream provider
    /// </summary>
    public interface IProvideTempStreams : IDisposable
    {
        /// <summary>
        /// Returns the path to a new temp file
        /// </summary>
        string GetNewTempPath();

        /// <summary>
        /// Returns an existing or creates a new temporary stream
        /// </summary>
        /// <param name="uniqueId">Id that uniquely identifies the context</param>
        /// <returns></returns>
        Stream Get(string uniqueId);

        /// <summary>
        /// Checks if a stream has been created
        /// </summary>
        /// <param name="uniqueId">Id that uniquely identifies the context</param>
        /// <returns></returns>
        bool HasStream(string uniqueId);
    }

    /// <summary>
    /// Hybrid buffers write first to memory but then to disk once it's cache is exhausted
    /// </summary>
    public interface IHybridBuffer : ICanEnumerate, IHaveSize
    {
        /// <summary>
        /// Copies the buffer to a stream
        /// </summary>
        /// <param name="stream"></param>
        void CopyTo(Stream stream);

        /// <summary>
        /// Number of distinct items in the buffer (or null if not known)
        /// </summary>
        uint? NumDistinct { get; }

        /// <summary>
        /// Adds an object to the buffer
        /// </summary>
        /// <param name="obj">Object to add</param>
        void AddObject(object obj);

        /// <summary>
        /// Buffer data type
        /// </summary>
        Type DataType { get; }
    }


    /// <summary>
    /// Typed hybrid buffer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IHybridBuffer<T> : IHybridBuffer, ICanEnumerateWithSize<T>, IAcceptSequentialTypedData<T>
        where T : notnull
    {
        Dictionary<T, uint>? DistinctItems { get; }
    }

    public interface IHybridBufferWithMetaData : IHybridBuffer, IHaveMetaData
    {
    }

    public interface IHybridBufferWithMetaData<T> : IHybridBuffer<T>, IHaveMetaData
        where T : notnull
    {
    }

    /// <summary>
    /// Type of hybrid buffer
    /// </summary>
    public enum HybridBufferType : byte
    {
        /// <summary>
        /// Unknown type
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Buffer of structs
        /// </summary>
        Struct,

        /// <summary>
        /// Buffer of strings
        /// </summary>
        String,

        /// <summary>
        /// Buffer of encoded structs
        /// </summary>
        EncodedStruct,

        /// <summary>
        /// Buffer of encoded strings
        /// </summary>
        EncodedString,

        /// <summary>
        /// Buffer of objects
        /// </summary>
        Object
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
    /// Indicates that the type can enumerate items
    /// </summary>
    public interface ICanEnumerate
    {
        /// <summary>
        /// Enumerates all items
        /// </summary>
        /// <returns></returns>
        IEnumerable<object> Enumerate();
    }

    /// <summary>
    /// Indicates that the type can enumerate items of this type
    /// </summary>
    /// <typeparam name="T">Type to enumerate</typeparam>
    public interface ICanEnumerate<out T>
        where T : notnull
    {
        /// <summary>
        /// Enumerates all items
        /// </summary>
        /// <returns></returns>
        IEnumerable<T> EnumerateTyped();
    }

    public interface ICanEnumerateDisposable<out T> : ICanEnumerate<T>, IDisposable where T : notnull
    {
    }

    public interface ICanEnumerateDisposable : ICanEnumerate, IDisposable
    {
    }

    public interface ICanEnumerateWithSize<out T> : ICanEnumerate<T>, IHaveSize where T : notnull
    {
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
    public interface IHaveIndexer
    {
        /// <summary>
        /// String indexer
        /// </summary>
        IIndexStrings? Indexer { get; }
    }

    /// <summary>
    /// Indicates that an operation can be completed
    /// </summary>
    public interface ICanComplete
    {
        /// <summary>
        /// Complete the operation
        /// </summary>
        void Complete();
    }

    /// <summary>
    /// Indicates that the type has a dictionary (string table)
    /// </summary>
    public interface IHaveDictionary
    {
        /// <summary>
        /// Current dictionary (string table)
        /// </summary>
        string[] Dictionary { get; }
    }

    public interface IStructByReferenceEnumerator<T> : IDisposable where T : struct
    {
        IStructByReferenceEnumerator<T> GetEnumerator() => this;
        bool MoveNext();
        void Reset();
        ref T Current { get; }
    }

    /// <summary>
    /// Implemented by types that can repeatedly read the same section of a stream
    /// </summary>
    public interface ICanReadSection : IDisposable
    {
        /// <summary>
        /// Creates a new reader for the readable section of the stream
        /// </summary>
        /// <returns></returns>
        BinaryReader GetReader();
        IStructByReferenceEnumerator<T> GetStructByReferenceEnumerator<T>(uint count) where T : struct;
        IEnumerable<T> Enumerate<T>(uint count) where T : struct;
    }

    /// <summary>
    /// Clones streams
    /// </summary>
    public interface ICloneStreams
    {
        /// <summary>
        /// Creates a new repeatable section reader
        /// </summary>
        /// <returns></returns>
        ICanReadSection Clone(long? position = null);
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
        /// Adds an additional predicate that must match to be considered valid
        /// </summary>
        /// <param name="predicate"></param>
        void AddPredicate(Predicate<T> predicate);
    }

    /// <summary>
    /// Notifies the user of operations and messages
    /// </summary>
    public interface INotifyUser
    {
        /// <summary>
        /// Called at the start of an operation
        /// </summary>
        /// <param name="operationId">Unique id for this operation</param>
        /// <param name="msg">Optional message associated with the operation</param>
        void OnStartOperation(string operationId, string? msg = null);

        /// <summary>
        /// Called when the operation has progressed
        /// </summary>
        /// <param name="operationId">Unique id for this operation</param>
        /// <param name="progressPercent">Progress percentage (between 0 and 1)</param>
        void OnOperationProgress(string operationId, float progressPercent);

        /// <summary>
        /// Called when the operation has completed
        /// </summary>
        /// <param name="operationId">Unique id for this operation</param>
        /// <param name="wasCancelled">True if the operation was cancelled</param>
        void OnCompleteOperation(string operationId, bool wasCancelled);

        /// <summary>
        /// Called to notify the user
        /// </summary>
        /// <param name="msg">Message to user</param>
        void OnMessage(string msg);
    }

    public interface ITensor2 : IDisposable, ISerializable, IHaveSpan, IHaveSize, IHaveTensorSegment
    {
        BrightDataContext Context { get; }
        LinearAlgebraProvider LinearAlgebraProvider { get; }
        ITensorSegment2 Segment { get; }
        IVector Reshape();
        IMatrix Reshape(uint? rows, uint? columns);
        ITensor3D Reshape(uint? depth, uint? rows, uint? columns);
        ITensor4D Reshape(uint? count, uint? depth, uint? rows, uint? columns);
        void Clear();
        ITensor2 Clone();
        uint TotalSize { get; }
        uint[] Shape { get; }
        void AddInPlace(ITensor2 tensor);
        void AddInPlace(ITensor2 tensor, float coefficient1, float coefficient2);
        void AddInPlace(float scalar);
        void MultiplyInPlace(float scalar);
        void SubtractInPlace(ITensor2 tensor);
        void SubtractInPlace(ITensor2 tensor, float coefficient1, float coefficient2);
        void PointwiseMultiplyInPlace(ITensor2 tensor);
        void PointwiseDivideInPlace(ITensor2 tensor);
        float DotProduct(ITensor2 tensor);
        uint? Search(float value);
        void ConstrainInPlace(float? minValue, float? maxValue);
        float Average();
        float L1Norm();
        float L2Norm();
        (float Min, float Max, uint MinIndex, uint MaxIndex) GetMinAndMaxValues();
        uint GetMinIndex();
        uint GetMaxIndex();
        float GetMin();
        float GetMax();
        bool IsEntirelyFinite();
        float CosineDistance(ITensor2 other);
        float EuclideanDistance(ITensor2 other);
        float MeanSquaredDistance(ITensor2 other);
        float SquaredEuclideanDistance(ITensor2 other);
        float ManhattanDistance(ITensor2 other);
        float StdDev(float? mean);
        IMatrix SoftmaxDerivative();
        void RoundInPlace(float lower, float upper, float? mid);
        void MapInPlace(Func<float, float> mutator);
        void L1Regularisation(float coefficient);
    }

    public interface ITensor2<out T> : ITensor2
        where T: ITensor2
    {
        new T Clone();
        T Add(ITensor2 tensor);
        T Add(ITensor2 tensor, float coefficient1, float coefficient2);
        T Add(float scalar);
        T Multiply(float scalar);
        T Subtract(ITensor2 tensor);
        T Subtract(ITensor2 tensor, float coefficient1, float coefficient2);
        T PointwiseMultiply(ITensor2 tensor);
        T PointwiseDivide(ITensor2 tensor);
        T Sqrt();
        T Reverse();
        IEnumerable<T> Split(uint blockCount);
        T Abs();
        T Log();
        T Exp();
        T Squared();
        T Sigmoid();
        T SigmoidDerivative();
        T Tanh();
        T TanhDerivative();
        T Relu();
        T ReluDerivative();
        T LeakyRelu();
        T LeakyReluDerivative();
        T Softmax();
        T Pow(float power);
        T CherryPick(uint[] indices);
        T Map(Func<float, float> mutator);
    }

    public interface IVector : ITensor2<IVector>, IVectorInfo
    {
        new float this[int index] { get; set; }
        new float this[uint index] { get; set; }
        float this[long index] { get; set; }
        float this[ulong index] { get; set; }
        IVector MapIndexed(Func<uint, float, float> mutator);
        void MapIndexedInPlace(Func<uint, float, float> mutator);
    }

    public interface IMatrix : ITensor2<IMatrix>, IMatrixInfo
    {
        new float this[int rowY, int columnX] { get; set; }
        new float this[uint rowY, uint columnX] { get; set; }
        float this[long rowY, long columnX] { get; set; }
        float this[ulong rowY, ulong columnX] { get; set; }
        ReadOnlySpan<float> GetRowSpan(uint rowIndex, ref SpanOwner<float> temp);
        ReadOnlySpan<float> GetColumnSpan(uint columnIndex);
        IVectorInfo GetRow(uint index);
        IVectorInfo GetColumn(uint index);
        IVectorInfo[] AllRows();
        IVectorInfo[] AllColumns();
        MemoryOwner<float> ToRowMajor();
        IMatrix Transpose();
        IMatrix Multiply(IMatrix other);
        IMatrix TransposeAndMultiply(IMatrix other);
        IMatrix TransposeThisAndMultiply(IMatrix other);
        IVector GetDiagonal();
        IVector RowSums();
        IVector ColumnSums();
        IMatrix Multiply(IVector vector);
        (IMatrix Left, IMatrix Right) SplitAtColumn(uint columnIndex);
        (IMatrix Top, IMatrix Bottom) SplitAtRow(uint rowIndex);
        IMatrix ConcatColumns(IMatrix bottom);
        IMatrix ConcatRows(IMatrix right);
        IMatrix MapIndexed(Func<uint, uint, float, float> mutator);
        void MapIndexedInPlace(Func<uint, uint, float, float> mutator);
        (IMatrix U, IVector S, IMatrix VT) Svd();
        IMatrix GetNewMatrixFromRows(IEnumerable<uint> rowIndices);
        IMatrix GetNewMatrixFromColumns(IEnumerable<uint> columnIndices);
        void AddToEachRow(ITensorSegment2 segment);
        void AddToEachColumn(ITensorSegment2 segment);
    }

    public interface IMatrixSegments
    {
        TensorSegmentWrapper Row(uint index);
        TensorSegmentWrapper Column(uint index);
    }

    public interface ITensor3D : ITensor2<ITensor3D>, ITensor3DInfo
    {
        new float this[int depth, int rowY, int columnX] { get; set; }
        new float this[uint depth, uint rowY, uint columnX] { get; set; }
        float this[long depth, long rowY, long columnX] { get; set; }
        float this[ulong depth, ulong rowY, ulong columnX] { get; set; }
        IMatrix GetMatrix(uint index);
        ITensor3D AddPadding(uint padding);
        ITensor3D RemovePadding(uint padding);
        IMatrix Im2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride);
        (ITensor3D Result, ITensor3D? Indices) MaxPool(uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices);
        ITensor3D ReverseMaxPool(ITensor3D indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride);
        ITensor3D ReverseIm2Col(IMatrix filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride);
        IMatrix CombineDepthSlices();
        ITensor3D Multiply(IMatrix matrix);
        void AddToEachRow(IVector vector);
        ITensor3D TransposeThisAndMultiply(ITensor4D other);
    }

    public interface ITensor4D : ITensor2<ITensor4D>, ITensor4DInfo
    {
        new float this[int count, int depth, int rowY, int columnX] { get; set; }
        new float this[uint count, uint depth, uint rowY, uint columnX] { get; set; }
        float this[long count, long depth, long rowY, long columnX] { get; set; }
        float this[ulong count, ulong depth, ulong rowY, ulong columnX] { get; set; }
        ITensor3D GetTensor(uint index);
        ITensor4D AddPadding(uint padding);
        ITensor4D RemovePadding(uint padding);
        (ITensor4D Result, ITensor4D? Indices) MaxPool(uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices);
        ITensor4D ReverseMaxPool(ITensor4D indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride);
        ITensor3D Im2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride);
        ITensor4D ReverseIm2Col(IMatrix filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride);
        IVector ColumnSums();
    }

    public interface ICountReferences
    {
        int AddRef();
        int Release();
        bool IsValid { get; }
    }

    public interface ITensorSegment2 : ICountReferences, IDisposable
    {
        uint Size { get; }
        string SegmentType { get; }
        float this[int index] { get; set; }
        float this[uint index] { get; set; }
        float this[long index] { get; set; }
        float this[ulong index] { get; set; }
        IEnumerable<float> Values { get; }
        float[]? GetArrayForLocalUseOnly();
        float[] ToNewArray();
        void CopyFrom(ReadOnlySpan<float> span);
        void CopyTo(ITensorSegment2 segment);
        void CopyTo(Span<float> destination);
        unsafe void CopyTo(float* destination, int offset, int stride, int count);
        void Clear();
        ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed);
        ReadOnlySpan<float> GetSpan();
    }

    public interface ICanIterateData<T> : IDisposable where T: unmanaged
    {
        IEnumerable<T> Enumerate();
        IReadOnlyEnumerator<T> GetEnumerator();
    }

    public interface ICanRandomlyAccessUnmanagedData<T> : IDisposable where T: unmanaged
    {
        void Get(int index, out T value);
        void Get(uint index, out T value);
        ReadOnlySpan<T> GetSpan(uint startIndex, uint count);
    }

    public interface ICanRandomlyAccessData : IDisposable
    {
        object this[int index] { get; }
        object this[uint index] { get; }
    }

    public interface ICanRandomlyAccessData<out T> : ICanRandomlyAccessData
    {
        T this[int index] { get; }
        T this[uint index] { get; }
    }

    public interface IHaveMutableReference<T> where T : unmanaged
    {
        ref T Current { get; }
    }

    public interface IReadOnlyEnumerator<T> : IDisposable where T : unmanaged
    {
        bool MoveNext();
        void Reset();
        ref readonly T Current { get; }
    }

    public interface IReadOnlyBuffer : IDisposable
    {
        ICanIterateData<T> GetIterator<T>(long offset, long sizeInBytes) where T : unmanaged;
        ICanRandomlyAccessUnmanagedData<T> GetBlock<T>(long offset, long sizeInBytes) where T : unmanaged;
    }

    public interface IConvertStructsToObjects<CT, out T> where CT: unmanaged where T: notnull
    {
        T Convert(ref CT item);
    }

    public interface IOperation<out T> : IDisposable
    {
        T Complete(INotifyUser? notifyUser, CancellationToken cancellationToken);
    }

    delegate void FillDelegate<CT, in T>(T item, Span<CT> ptr, int index) 
        where T : notnull 
        where CT : struct
    ;
}
