using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BrightData.Helper;
using BrightData.Operations.Conversion;
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

    public interface IHaveDataAsReadOnlyByteSpan
    {
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

    public interface IHaveMemory<T>
    {
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
    public interface IDataAnalyser : IWriteToMetaData, IAcceptBlock
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

    public interface IAcceptBlock
    {
    }

    public interface IAcceptBlock<T> : IAcceptBlock
    {
        void Add(ReadOnlySpan<T> block);
    }

    /// <summary>
    /// Typed data analyser
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDataAnalyser<T> : IAcceptSequentialTypedData<T>, IAcceptBlock<T>, IDataAnalyser where T : notnull
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
        /// Called to notify the user
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
    /// Indicates that the type can randomly access typed data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICanRandomlyAccessUnmanagedData<T> : IDisposable, IHaveSize where T: unmanaged
    {
        /// <summary>
        /// Returns a randomly accessed item
        /// </summary>
        /// <param name="index">Item index</param>
        /// <param name="value">Item value</param>
        void Get(int index, out T value);

        /// <summary>
        /// Returns a randomly accessed item
        /// </summary>
        /// <param name="index">Item index</param>
        /// <param name="value">Item value</param>
        void Get(uint index, out T value);

        /// <summary>
        /// Returns a span of data
        /// </summary>
        /// <param name="startIndex">Inclusive first index of the span</param>
        /// <param name="count">Size of the span</param>
        /// <returns></returns>
        ReadOnlySpan<T> GetSpan(uint startIndex, uint count);
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
    /// Indicates that the type can randomly access typed data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICanRandomlyAccessData<out T> : ICanRandomlyAccessData
    {
        /// <summary>
        /// Returns the typed at at this index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        new T this[int index] { get; }

        /// <summary>
        /// Returns the typed at at this index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        new T this[uint index] { get; }
    }

    /// <summary>
    /// Indicates that type exposes a mutable reference
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IHaveMutableReference<T> where T : unmanaged
    {
        /// <summary>
        /// The current mutable reference
        /// </summary>
        ref T Current { get; }
    }

    /// <summary>
    /// A read only reference enumerator for unmanaged types
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReadOnlyUnmanagedEnumerator<T> : IDisposable where T : unmanaged
    {
        /// <summary>
        /// Moves to the next item
        /// </summary>
        /// <returns></returns>
        bool MoveNext();

        /// <summary>
        /// Resets the enumerator
        /// </summary>
        void Reset();

        /// <summary>
        /// Returns a readonly reference to the current item
        /// </summary>
        ref readonly T Current { get; }
    }

    /// <summary>
    /// Indicates that the type can convert structs to other types
    /// </summary>
    /// <typeparam name="CT"></typeparam>
    /// <typeparam name="T"></typeparam>
    public interface IConvertStructsToObjects<CT, out T> where CT: unmanaged where T: notnull
    {
        /// <summary>
        /// Converts a struct to another type
        /// </summary>
        /// <param name="item">The item to convert</param>
        /// <returns></returns>
        T Convert(in CT item);
    }

    /// <summary>
    /// A generic operation that might require user notification and that can be cancelled
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IOperation<out T> : IDisposable
    {
        /// <summary>
        /// Tries to complete the operation
        /// </summary>
        /// <param name="notifyUser">Optional interface to notify the user of progress</param>
        /// <param name="cancellationToken">Cancellation token to cancel operation</param>
        /// <returns></returns>
        T Complete(INotifyUser? notifyUser, CancellationToken cancellationToken);
    }

    delegate void FillDelegate<CT, in T>(T item, Span<CT> ptr, int index) 
        where T : notnull 
        where CT : struct
    ;

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

    public record ColumnConversionInfo(uint ColumnIndex, ColumnConversion Conversion)
    {
        public virtual async Task<IReadOnlyBufferWithMetaData> Convert(IDataTable dataTable)
        {
            var column = dataTable.GetColumn(ColumnIndex);
            var ret = await column.Convert(Conversion);
            column.MetaData.CopyTo(ret.MetaData, Consts.Name, Consts.IsTarget, Consts.ColumnIndex);
            return ret;
        }
    }

    public record CustomColumnConversionInfo<FT, TT>(uint ColumnIndex, Func<FT, TT> Converter) : ColumnConversionInfo(ColumnIndex, ColumnConversion.Custom)
    {
        public override async Task<IReadOnlyBufferWithMetaData> Convert(IDataTable dataTable)
        {
            var column = dataTable.GetColumn(ColumnIndex);
            if (column.DataType != typeof(FT))
                throw new Exception($"Expected column {ColumnIndex} to be of type {typeof(FT)} but found {column.DataType}");
            var output = column.DataType.GetBrightDataType().CreateCompositeBuffer();
            var conversion = GenericActivator.Create<IOperation>(typeof(CustomConversion<,>).MakeGenericType(column.DataType, typeof(TT)), Converter, column, output);
            await conversion.Process();
            column.MetaData.CopyTo(output.MetaData, Consts.Name, Consts.IsTarget, Consts.ColumnIndex);
            return output;
        }
    }
}
