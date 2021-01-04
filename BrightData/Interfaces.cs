using System;
using System.Collections.Generic;
using System.IO;

namespace BrightData
{
    /// <summary>
    /// Interface 
    /// </summary>
    public interface IHaveIndices
    {
        IEnumerable<uint> Indices { get; }
    }

    public interface ICanWriteToBinaryWriter
    {
        void WriteTo(BinaryWriter writer);
    }

    public interface ICanInitializeFromBinaryReader
    {
        void Initialize(IBrightDataContext context, BinaryReader reader);
    }

    public interface ISerializable : ICanWriteToBinaryWriter, ICanInitializeFromBinaryReader
    {
    }

    public interface IMetaData : ICanWriteToBinaryWriter
    {
        object Get(string name);
        T? GetNullable<T>(string name) where T : struct;
        T Get<T>(string name, T valueIfMissing = default) where T : IConvertible;
        T Set<T>(string name, T value) where T : IConvertible;
        string AsXml { get; }
        void CopyTo(IMetaData metadata);
        void CopyTo(IMetaData metadata, params string[] keys);
        void CopyAllExcept(IMetaData metadata, params string[] keys);
        void ReadFrom(BinaryReader reader);
        IEnumerable<string> GetStringsWithPrefix(string prefix);
    }

    public interface IHaveMetaData
    {
        IMetaData MetaData { get; }
    }

    public interface IReferenceCountedMemory
    {
        uint Size { get; }
        int AddRef();
        int Release();
        long AllocationIndex { get; }
        bool IsValid { get; }
    }

    public interface ITensor : IDisposable, ISerializable
    {
        uint[] Shape { get; }
        uint Size { get; }
        uint Rank { get; }
    }

    public interface ITensor<T> : ITensor
        where T : struct
    {
        ITensorSegment<T> GetDataCopy();
        ITensorSegment<T> Segment { get; }
        INumericComputation<T> Computation { get; }
    }

    public interface IDataReader
    {
        T Read<T>(BinaryReader reader);
        T[] ReadArray<T>(BinaryReader reader);
    }

    public interface ITensorPool
    {
        T[] Get<T>(uint size) where T : struct;
        void Reuse<T>(T[] block) where T : struct;
        long MaxCacheSize { get; }
        long CacheSize { get; }
    }

    public interface IDisposableLayers
    {
        void Add(IDisposable disposable);
        IDisposable Push();
        void Pop();
    }

    public interface ISetLinearAlgebraProvider
    {
        ILinearAlgebraProvider LinearAlgebraProvider { set; }
    }

    public interface IBrightDataContext : IDisposable
    {
        Random Random { get; }
        ITensorPool TensorPool { get; }
        IDisposableLayers MemoryLayer { get; }
        IDataReader DataReader { get; }
        INumericComputation<T> GetComputation<T>() where T : struct;
        ILinearAlgebraProvider LinearAlgebraProvider { get; }
        IProvideTempStreams TempStreamProvider { get; }
        T Get<T>(string name);
        T Set<T>(string name, T value);
        T Set<T>(string name, Func<T> valueCreator);
        bool IsStochastic { get; }
    }

    public interface IHaveBrightDataContext
    {
        IBrightDataContext Context { get; }
    }

    public interface ITensorSegment<T> : IReferenceCountedMemory, IDisposable, IHaveBrightDataContext
        where T : struct
    {
        bool IsContiguous { get; }
        T this[uint index] { get; set; }
        T this[long index] { get; set; }
        T[] ToArray();
        IEnumerable<T> Values { get; }
        void InitializeFrom(Stream stream);
        void Initialize(Func<uint, T> initializer);
        void Initialize(T initializer);
        void Initialize(T[] initialData);
        void WriteTo(Stream writerBaseStream);
        void CopyTo(T[] array);
        void CopyTo(ITensorSegment<T> segment);
    }

    public interface INumericComputation<T>
        where T : struct
    {
        ITensorSegment<T> Abs(ITensorSegment<T> tensor);
        ITensorSegment<T> Add(ITensorSegment<T> tensor1, ITensorSegment<T> tensor2);
        void AddInPlace(ITensorSegment<T> target, ITensorSegment<T> other);
        void AddInPlace(ITensorSegment<T> target, ITensorSegment<T> other, T coefficient1, T coefficient2);
        void AddInPlace(ITensorSegment<T> target, T scalar);
        void ConstrainInPlace(ITensorSegment<T> segment, T? minValue, T? maxValue);

        T Average(ITensorSegment<T> segment);
        T CosineDistance(ITensorSegment<T> tensor, ITensorSegment<T> other);
        T DotProduct(ITensorSegment<T> segment, ITensorSegment<T> other);
        T EuclideanDistance(ITensorSegment<T> tensor, ITensorSegment<T> other);
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
        void SubtractInPlace(ITensorSegment<T> target, ITensorSegment<T> other);
        void SubtractInPlace(ITensorSegment<T> target, ITensorSegment<T> other, T coefficient1, T coefficient2);
        T Sum(ITensorSegment<T> tensor);

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

        T Get(uint val);
        T Get(float val);
        T Get(double val);
        T Get(decimal val);
    }

    public interface ITensorComputation<T>
        where T : struct
    {
        Matrix<T> Transpose(Matrix<T> m);
        Matrix<T> Multiply(Matrix<T> m1, Matrix<T> m2);
        Matrix<T> TransposeAndMultiply(Matrix<T> m1, Matrix<T> m2);
        Matrix<T> TransposeThisAndMultiply(Matrix<T> m1, Matrix<T> m2);
        Vector<T> RowSums(Matrix<T> m);
        Vector<T> ColumnSums(Matrix<T> m);
        void AddToEachRowInPlace(Matrix<T> m, Vector<T> v);
        void AddToEachColumnInPlace(Matrix<T> m, Vector<T> v);
        Matrix<T> ConcatRows(Matrix<T> left, Matrix<T> right);
        Matrix<T> ConcatColumns(Matrix<T> top, Matrix<T> bottom);
    }

    public interface IWriteToMetaData
    {
        void WriteTo(IMetaData metadata);
    }

    public interface IDataAnalyser : IWriteToMetaData
    {
        void AddObject(object obj);
    }

    public interface IDataAnalyser<in T> : IDataAnalyser
    {
        void Add(T obj);
    }

    public enum NormalizationType
    {
        None = 0,
        Standard,
        Euclidean,
        Manhattan,
        FeatureScale
    }

    public interface IHaveEncodedData
    {
        bool IsEncoded { get; }
    }

    public interface ICanConvert
    {
        Type From { get; }
        Type To { get; }
    }

    public interface ICanConvert<in TF, out TT> : ICanConvert
    {
        TT Convert(TF data);
    }

    public interface INormalize
    {
        NormalizationType NormalizationType { get; }
        double Divide { get; }
        double Subtract { get; }
    }

    public enum OperationType
    {
        Add,
        Average,
        Max
    }

    public interface IDistribution<out T>
        where T: struct
    {
        T Sample();
    }

    public interface IDiscreteDistribution : IDistribution<int>
    {
    }

    public interface INonNegativeDiscreteDistribution : IDistribution<uint>
    {
    }

    public interface IContinuousDistribution : IDistribution<float>
    {
    }

    public interface IProvideTempStreams : IDisposable
    {
        Stream Get(string uniqueId);
        bool HasStream(string uniqueId);
    }

    public interface IHybridBuffer
    {
        void CopyTo(Stream stream);
        IEnumerable<object> Enumerate();
        uint Length { get; }
        uint? NumDistinct { get; }
        void Add(object obj);
    }

    public interface IHybridBuffer<T> : IHybridBuffer
    {
        void Add(T item);
        IEnumerable<T> EnumerateTyped();
    }

    public enum BufferType : byte
    {
        Unknown = 0,
        Struct,
        String,
        EncodedStruct,
        EncodedString,
        Object
    }

    public interface ICanEnumerate<out T>
    {
        IEnumerable<T> EnumerateTyped();
        uint Size { get; }
    }

    public interface IIndexStrings
    {
        uint GetIndex(string str);
        uint OutputSize { get; }
    }

    public interface IHaveIndexer
    {
        IIndexStrings Indexer { get; }
    }
}
