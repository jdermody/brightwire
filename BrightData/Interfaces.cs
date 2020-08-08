using System;
using System.Collections.Generic;
using System.IO;

namespace BrightData
{
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

    public interface IMetaData : ICanWriteToBinaryWriter
    {
        object Get(string name);
        T Get<T>(string name, T valueIfMissing = default) where T : IConvertible;
        T Set<T>(string name, T value) where T : IConvertible;
        string AsXml { get; }
        void CopyTo(IMetaData metadata);
        void CopyTo(IMetaData metadata, params string[] keys);
        void CopyAllExcept(IMetaData metadata, params string[] keys);
        void ReadFrom(BinaryReader reader);
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

    public interface ITensor : IDisposable
    {
        uint[] Shape { get; }
    }

    public interface ITensor<T> : ITensor
        where T : struct
    {
        ITensorSegment<T> GetDataCopy();
        ITensorSegment<T> Data { get; }
        INumericComputation<T> Computation { get; }
    }

    public interface IMemoryDeallocator
    {
        void Free();
    }

    public interface ITensorBlock<T> : IReferenceCountedMemory
        where T : struct
    {
        string TensorType { get; }
        ITensorSegment<T> GetSegment();
        IBrightDataContext Context { get; }
        void InitializeFrom(Stream stream);
        void InitializeFrom(ITensorBlock<T> tensor);
        void InitializeFrom(T[] array);
        Span<T> Data { get; }
        ITensorBlock<T> Clone();
    }

    public interface IDataReader
    {
        T Read<T>(BinaryReader reader);
        T[] ReadArray<T>(BinaryReader reader);
    }

    public interface ITensorPool
    {
        ITensorBlock<T> Get<T>(uint size) where T : struct;
        void Add<T>(ITensorBlock<T> block) where T : struct;
        long MaxCacheSize { get; }
        long AllocationSize { get; }
        long CacheSize { get; }
    }

    public interface IDisposableLayers
    {
        void Add(IDisposable disposable);
        IDisposable Push();
        void Pop();
    }

    public interface IBrightDataContext : IDisposable
    {
        Random Random { get; }
        ITensorPool TensorPool { get; }
        IDisposableLayers MemoryLayer { get; }
        IDataReader DataReader { get; }
        INumericComputation<T> GetComputation<T>() where T : struct;
        T Get<T>(string name);
        T Set<T>(string name, T value);
        T Set<T>(string name, Func<T> valueCreator);
        IComputableFactory ComputableFactory { get; set; }
    }

    public interface ITensorSegment<T> : IReferenceCountedMemory, IDisposable
        where T : struct
    {
        IBrightDataContext Context { get; }
        (ITensorBlock<T> Block, bool IsNewCopy) GetBlock(ITensorPool pool);
        T this[uint index] { get; set; }
        T this[long index] { get; set; }
        T[] ToArray();
        IEnumerable<T> Values { get; }
        void InitializeFrom(Stream stream);
        void Initialize(Func<uint, T> initializer);
        void Initialize(T initializer);
        void Initialize(T[] initialData);
        void WriteTo(Stream writerBaseStream);
    }

    public interface ITensorAllocator
    {
        ITensorBlock<T> Create<T>(IBrightDataContext context, uint size) where T : struct;
    }

    //public interface IHaveTensorSegment<T>
    //    where T : struct
    //{
    //    ITensorSegment<T> Data { get; }
    //}

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
        List<ITensorSegment<T>> Split(ITensorSegment<T> segment, int blockCount);
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

        T Get(uint val);
        T Get(float val);
        T Get(double val);
        T Get(decimal val);
    }

    public interface ITensorComputation<T>
        where T: struct
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
        Standard,
        Euclidean,
        Manhattan,
        FeatureScale
    }

    

    public interface IAutoGrowBuffer : ICanWriteToBinaryWriter
    {
        uint Size { get; }
        void Add(object obj);
        IEnumerable<object> Enumerate();
    }

    public interface IAutoGrowBuffer<T> : IAutoGrowBuffer
    {
        void Add(T obj);
        IEnumerable<T> EnumerateTyped();
        void Write(IReadOnlyCollection<T> items, BinaryWriter writer);
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
}
