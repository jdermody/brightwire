using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using BrightData.LinearAlegbra2;
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
        void Initialize(IBrightDataContext context, BinaryReader reader);
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
    /// Typed tensor base interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITensor<T> : ITensor
        where T : struct
    {
        /// <summary>
        /// Returns a copy of the underlying data segment
        /// </summary>
        /// <returns></returns>
        ITensorSegment<T> GetDataCopy();

        /// <summary>
        /// Underlying data segment
        /// </summary>
        ITensorSegment<T> Segment { get; }

        /// <summary>
        /// Typed computation interface
        /// </summary>
        INumericComputation<T> Computation { get; }

        /// <summary>
        /// Convert to an array
        /// </summary>
        /// <returns></returns>
        T[] ToArray();


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
        ILinearAlgebraProvider LinearAlgebraProvider { set; }
    }

    /// <summary>
    /// Gives access to a linear algebra provider
    /// </summary>
    public interface IHaveLinearAlgebraProvider
    {
        /// <summary>
        /// Linear algebra provider
        /// </summary>
        ILinearAlgebraProvider LinearAlgebraProvider { get; }
    }

    /// <summary>
    /// Bright data context
    /// </summary>
    public interface IBrightDataContext : IDisposable, IHaveLinearAlgebraProvider
    {
        /// <summary>
        /// Random number generator
        /// </summary>
        Random Random { get; }

        /// <summary>
        /// Tensor pool
        /// </summary>
        ITensorPool TensorPool { get; }

        /// <summary>
        /// Disposable memory layers
        /// </summary>
        IDisposableLayers MemoryLayer { get; }

        /// <summary>
        /// Data reader
        /// </summary>
        IDataReader DataReader { get; }

        /// <summary>
        /// Typed computation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        INumericComputation<T> GetComputation<T>() where T : struct;

        /// <summary>
        /// Temp Stream Provider
        /// </summary>
        IProvideTempStreams TempStreamProvider { get; }

        /// <summary>
        /// Returns transient, context specific meta data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">Name of value</param>
        /// <param name="defaultValue">Default value if not already set</param>
        /// <returns></returns>
        T Get<T>(string name, T defaultValue) where T : notnull;

        /// <summary>
        /// Returns transient, context specific meta data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">Name of value</param>
        /// <param name="defaultValueCreator">Returns a default value if not already set</param>
        /// <returns></returns>
        T Get<T>(string name, Func<T> defaultValueCreator) where T : notnull;

        /// <summary>
        /// Returns optional context specific meta data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">Name of value</param>
        /// <returns></returns>
        T? Get<T>(string name) where T : class;

        /// <summary>
        /// Sets transient, context specific meta data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">Name of value</param>
        /// <param name="value">Value</param>
        /// <returns></returns>
        T Set<T>(string name, T value) where T : notnull;

        /// <summary>
        /// Sets transient, context specific meta data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">Name of value</param>
        /// <param name="valueCreator">Function that will create value to set on demand if not already set</param>
        /// <returns></returns>
        T Set<T>(string name, Func<T> valueCreator) where T : notnull;

        /// <summary>
        /// True if random generator has been initialized with a random initial seed
        /// </summary>
        bool IsStochastic { get; }

        /// <summary>
        /// Resets the random number generator
        /// </summary>
        /// <param name="seed">Random seed (or null to randomly initialize)</param>
        public void ResetRandom(int? seed);

        /// <summary>
        /// Progress notifications for long running operations
        /// </summary>
        public INotifyUser? UserNotifications { get; set; }

        /// <summary>
        /// Cancellation token for the current context
        /// </summary>
        CancellationToken CancellationToken { get; }
    }

    /// <summary>
    /// Indicates that the type has a data context
    /// </summary>
    public interface IHaveDataContext
    {
        /// <summary>
        /// Bright data context
        /// </summary>
        IBrightDataContext Context { get; }
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

    /// <summary>
    /// Typed generic computation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface INumericComputation<T>
        where T : struct
    {
#pragma warning disable 1591
        ITensorSegment<T> Abs(ITensorSegment<T> tensor);
        ITensorSegment<T> Add(ITensorSegment<T> tensor1, ITensorSegment<T> tensor2);
        ITensorSegment<T> Add(ITensorSegment<T> tensor1, ITensorSegment<T> tensor2, T coefficient1, T coefficient2);
        ITensorSegment<T> Add(ITensorSegment<T> tensor1, T scalar);
        void AddInPlace(ITensorSegment<T> target, ITensorSegment<T> other);
        void AddInPlace(ITensorSegment<T> target, ITensorSegment<T> other, T coefficient1, T coefficient2);
        void AddInPlace(ITensorSegment<T> target, T scalar);
        void ConstrainInPlace(ITensorSegment<T> segment, T? minValue, T? maxValue);

        T Average(ITensorSegment<T> segment);
        T CosineDistance(ITensorSegment<T> tensor, ITensorSegment<T> other);
        T DotProduct(ITensorSegment<T> segment, ITensorSegment<T> other);
        T EuclideanDistance(ITensorSegment<T> tensor, ITensorSegment<T> other);
        T MeanSquaredDistance(ITensorSegment<T> tensor, ITensorSegment<T> other);
        T SquaredEuclideanDistance(ITensorSegment<T> tensor, ITensorSegment<T> other);
        ITensorSegment<T> Exp(ITensorSegment<T> tensor);
        T L1Norm(ITensorSegment<T> segment);
        T L2Norm(ITensorSegment<T> segment);
        ITensorSegment<T> Log(ITensorSegment<T> tensor);
        T ManhattanDistance(ITensorSegment<T> tensor, ITensorSegment<T> other);
        void MultiplyInPlace(ITensorSegment<T> target, T scalar);
        ITensorSegment<T> Multiply(ITensorSegment<T> target, T scalar);
        T NextRandom();
        ITensorSegment<T> PointwiseDivide(ITensorSegment<T> tensor1, ITensorSegment<T> tensor2);
        void PointwiseDivideInPlace(ITensorSegment<T> target, ITensorSegment<T> other);
        ITensorSegment<T> PointwiseMultiply(ITensorSegment<T> tensor1, ITensorSegment<T> tensor2);
        void PointwiseMultiplyInPlace(ITensorSegment<T> target, ITensorSegment<T> other);
        uint? Search(ITensorSegment<T> segment, T value);
        ITensorSegment<T> Sqrt(ITensorSegment<T> tensor);
        ITensorSegment<T> Squared(ITensorSegment<T> tensor);
        T StdDev(ITensorSegment<T> segment, T? mean);
        ITensorSegment<T> Subtract(ITensorSegment<T> tensor1, ITensorSegment<T> tensor2);
        ITensorSegment<T> Subtract(ITensorSegment<T> tensor1, ITensorSegment<T> tensor2, T coefficient1, T coefficient2);
        void SubtractInPlace(ITensorSegment<T> target, ITensorSegment<T> other);
        void SubtractInPlace(ITensorSegment<T> target, ITensorSegment<T> other, T coefficient1, T coefficient2);
        T Sum(ITensorSegment<T> tensor);
        T LengthOf(ITensorSegment<T> tensor);
        (T Min, T Max, uint MinIndex, uint MaxIndex) GetMinAndMaxValues(ITensorSegment<T> segment);
        bool IsEntirelyFinite(ITensorSegment<T> segment);
        ITensorSegment<T> Reverse(ITensorSegment<T> segment);
        List<ITensorSegment<T>> Split(ITensorSegment<T> segment, uint blockCount);
        ITensorSegment<T> Sigmoid(ITensorSegment<T> segment);
        ITensorSegment<T> SigmoidDerivative(ITensorSegment<T> segment);
        ITensorSegment<T> Tanh(ITensorSegment<T> segment);
        ITensorSegment<T> TanhDerivative(ITensorSegment<T> segment);
        ITensorSegment<T> Relu(ITensorSegment<T> segment);
        ITensorSegment<T> ReluDerivative(ITensorSegment<T> segment);
        ITensorSegment<T> LeakyRelu(ITensorSegment<T> segment);
        ITensorSegment<T> LeakyReluDerivative(ITensorSegment<T> segment);
        ITensorSegment<T> Softmax(ITensorSegment<T> segment);
        Matrix<T> SoftmaxDerivative(ITensorSegment<T> segment);
        ITensorSegment<T> Pow(ITensorSegment<T> segment, T power);
        void RoundInPlace(ITensorSegment<T> segment, T lower, T upper, T? mid);

        T Get(uint val);
        T Get(float val);
        T Get(double val);
        T Get(decimal val);
        T Add(T a, T b);
        T Subtract(T a, T b);
        T Multiply(T a, T b);
        T Divide(T a, T b);
        T Sqrt(T a);
        T Abs(T a);
        T Log(T a);
        T Exp(T a);
        T Pow(T a, T rank);

        public T Zero { get; }
        public T One { get; }
#pragma warning restore 1591
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
    /// Typed data analyser
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDataAnalyser<in T> : IDataAnalyser where T : notnull
    {
        /// <summary>
        /// Adds a typed object to analyze
        /// </summary>
        /// <param name="obj"></param>
        void Add(T obj);
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

    //public interface IHaveEncodedData
    //{
    //    bool IsEncoded { get; }
    //}

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
    public interface IHybridBuffer : ICanEnumerate
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
        /// <param name="index">Index to set within the buffer</param>
        void Add(object obj, uint index);

        /// <summary>
        /// Buffer data type
        /// </summary>
        Type DataType { get; }
    }

    /// <summary>
    /// Append only buffer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAppendableBuffer<in T> where T : notnull
    {
        /// <summary>
        /// Adds a new item
        /// </summary>
        /// <param name="value">Item to add</param>
        /// <param name="index">Row index</param>
        void Add(T value, uint index);
    }

    /// <summary>
    /// Typed hybrid buffer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IHybridBuffer<T> : IHybridBuffer, ICanEnumerate<T>, IAppendableBuffer<T>
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
    public interface ICanEnumerate : IHaveSize
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
    public interface ICanEnumerate<out T> : IHaveSize
        where T : notnull
    {
        /// <summary>
        /// Enumerates all items
        /// </summary>
        /// <returns></returns>
        IEnumerable<T> EnumerateTyped();
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
        ICanReadSection Clone();
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
        /// <param name="msg">Optional message associated with the operation</param>
        void OnStartOperation(string? msg = null);

        /// <summary>
        /// Called when the operation has progressed
        /// </summary>
        /// <param name="progressPercent">Progress percentage (between 0 and 1)</param>
        void OnOperationProgress(float progressPercent);

        /// <summary>
        /// Called when the operation has completed
        /// </summary>
        void OnCompleteOperation();

        /// <summary>
        /// Called to notify the user
        /// </summary>
        /// <param name="msg">Message to user</param>
        void OnMessage(string msg);
    }

    public interface ITensor2 : IDisposable
    {
        BrightDataContext2 Context { get; }
        ITensorSegment2 Segment { get; }
        IVector Reshape();
        IMatrix Reshape(uint? rows, uint? columns);
        void Clear();
        ITensor2 Clone();
    }

    public interface ITensor2<out T> : ITensor2 
        where T: ITensor2
    {
        T Add(ITensor2 tensor);
        T Add(ITensor2 tensor, float coefficient1, float coefficient2);
        T Add(float scalar);
        void AddInPlace(ITensor2 tensor);
        void AddInPlace(ITensor2 tensor, float coefficient1, float coefficient2);
        void AddInPlace(float scalar);
        void MultiplyInPlace(float scalar);
        T Multiply(float scalar);
        T Subtract(ITensor2 tensor);
        T Subtract(ITensor2 tensor, float coefficient1, float coefficient2);
        void SubtractInPlace(ITensor2 tensor);
        void SubtractInPlace(ITensor2 tensor, float coefficient1, float coefficient2);
        T PointwiseMultiply(ITensor2 tensor);
        void PointwiseMultiplyInPlace(ITensor2 tensor);
        T PointwiseDivide(ITensor2 tensor);
        void PointwiseDivideInPlace(ITensor2 tensor);
        float DotProduct(ITensor2 tensor);
        T Sqrt();
        uint? Search(float value);
        void ConstrainInPlace(float? minValue, float? maxValue);
        float Average();
        float L1Norm();
        float L2Norm();
        (float Min, float Max, uint MinIndex, uint MaxIndex) GetMinAndMaxValues();
        bool IsEntirelyFinite();
        T Reverse();
        IEnumerable<T> Split(uint blockCount);
        float CosineDistance(ITensor2 other);
        float EuclideanDistance(ITensor2 other);
        float MeanSquaredDistance(ITensor2 other);
        float SquaredEuclideanDistance(ITensor2 other);
        float ManhattanDistance(ITensor2 other);
        T Abs();
        T Log();
        T Exp();
        T Squared();
        float StdDev(float? mean);
        T Sigmoid();
        T SigmoidDerivative();
        T Tanh();
        T TanhDerivative();
        T Relu();
        T ReluDerivative();
        T LeakyRelu();
        T LeakyReluDerivative();
        T Softmax();
        IMatrix SoftmaxDerivative();
        T Pow(float power);
        void RoundInPlace(float lower, float upper, float? mid);
        T CherryPick(uint[] indices);
        T Map(Func<float, float> mutator);
        void MapInPlace(Func<float, float> mutator);
    }

    public interface IVector : ITensor2<IVector>
    {
        public uint Size { get; }
        float this[int index] { get; set; }
        float this[uint index] { get; set; }
        float this[long index] { get; set; }
        float this[ulong index] { get; set; }
        IVector MapIndexed(Func<uint, float, float> mutator);
        void MapIndexedInPlace(Func<uint, float, float> mutator);
    }

    public interface IMatrix : ITensor2<IMatrix>
    {
        uint RowCount { get; }
        uint ColumnCount { get; }
        float this[int rowY, int columnX] { get; set; }
        float this[uint rowY, uint columnX] { get; set; }
        float this[long rowY, long columnX] { get; set; }
        float this[ulong rowY, ulong columnX] { get; set; }
        IDisposableTensorSegmentWrapper Row(uint index);
        IDisposableTensorSegmentWrapper Column(uint index);
        IDisposableTensorSegmentWrapper[] Rows();
        IDisposableTensorSegmentWrapper[] Columns();
        MemoryOwner<float> ToNewColumnMajor();
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
    }

    public interface ITensor3D : ITensor2<ITensor3D>
    {
        uint Depth { get; }
        uint RowCount { get; }
        uint ColumnCount { get; }
        uint MatrixSize { get; }
        float this[int depth, int rowY, int columnX] { get; set; }
        float this[uint depth, uint rowY, uint columnX] { get; set; }
        float this[long depth, long rowY, long columnX] { get; set; }
        float this[ulong depth, ulong rowY, ulong columnX] { get; set; }
        IMatrix Matrix(uint index);
        MemoryOwner<float> ToNewColumnMajor();
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

    public interface ITensor4D : ITensor2<ITensor4D>
    {
        uint Count { get; }
        uint Depth { get; }
        uint RowCount { get; }
        uint ColumnCount { get; }
        uint MatrixSize { get; }
        uint TensorSize { get; }
        float this[int count, int depth, int rowY, int columnX] { get; set; }
        float this[uint count, uint depth, uint rowY, uint columnX] { get; set; }
        float this[long count, long depth, long rowY, long columnX] { get; set; }
        float this[ulong count, ulong depth, ulong rowY, ulong columnX] { get; set; }
        ITensor3D Tensor(uint index);
        ITensor4D AddPadding(uint padding);
        ITensor4D RemovePadding(uint padding);
        (ITensor4D Result, ITensor4D? Indices) MaxPool(uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices);
        ITensor4D ReverseMaxPool(ITensor4D indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride);
        ITensor3D Im2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride);
        ITensor4D ReverseIm2Col(IMatrix filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride);
    }

    public interface ICountReferences
    {
        int AddRef();
        int Release();
        bool IsValid { get; }
    }

    public interface ITensorSegment2 : ICountReferences
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
        void CopyFrom(Span<float> span, float[]? sourceArray);
        void CopyTo(ITensorSegment2 segment);
        void Clear();
    }

    public interface IDisposableTensorSegment : ITensorSegment2, IDisposable
    {
    }

    public interface IDisposableTensorSegmentWrapper : IDisposableTensorSegment
    {
    }
}
