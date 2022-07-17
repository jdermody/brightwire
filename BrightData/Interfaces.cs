using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using BrightData.LinearAlgebra;
using Microsoft.Toolkit.HighPerformance.Buffers;

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
    public interface IAmSerializable : ICanWriteToBinaryWriter, ICanInitializeFromBinaryReader
    {
    }

    /// <summary>
    /// Indicates that the type has a meta data store
    /// </summary>
    public interface IHaveMetaData
    {
        /// <summary>
        /// Meta data store
        /// </summary>
        MetaData MetaData { get; }
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
    /// Indicates that the type can write values to meta data
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
        IEnumerable<object> Values { get; }
    }

    /// <summary>
    /// Indicates that the type can enumerate items of this type
    /// </summary>
    /// <typeparam name="T">Type to enumerate</typeparam>
    public interface ICanEnumerate<out T> : ICanEnumerate
        where T : notnull
    {
        /// <summary>
        /// Enumerates all items
        /// </summary>
        /// <returns></returns>
        new IEnumerable<T> Values { get; }
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
    public interface IHaveStringIndexer
    {
        /// <summary>
        /// String indexer
        /// </summary>
        IIndexStrings? Indexer { get; }
    }

    /// <summary>
    /// Indicates that the type has a string table
    /// </summary>
    public interface IHaveStringTable
    {
        /// <summary>
        /// Current string table
        /// </summary>
        string[] StringTable { get; }
    }

    public interface IStructByReferenceEnumerator<T> : IDisposable where T : struct
    {
        IStructByReferenceEnumerator<T> GetEnumerator() => this;
        bool MoveNext();
        void Reset();
        ref readonly T Current { get; }
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

    public interface ICountReferences
    {
        int AddRef();
        int Release();
        bool IsValid { get; }
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
        T Convert(in CT item);
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
