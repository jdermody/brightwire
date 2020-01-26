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
    }

    public interface IMemoryDeallocator
    {
        void Free();
    }

    public interface ITensorBlock<T> : IReferenceCountedMemory
        where T: struct
    {
        string TensorType { get; }
        ITensorSegment<T> GetSegment();
        IBrightDataContext Context { get; }
        void InitializeFrom(Stream stream);
        void InitializeFrom(ITensorBlock<T> tensor);
        void InitializeFrom(T[] array);
        Span<T> Data { get; }
    }

    public interface IDataReader
    {
        T Read<T>(BinaryReader reader);
        T[] ReadArray<T>(BinaryReader reader);
    }

    public interface ITensorPool
    {
        ITensorBlock<T> Get<T>(uint size) where T: struct;
        void Add<T>(ITensorBlock<T> block) where T: struct;
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
        ITensorPool TensorPool { get; }
        IDisposableLayers MemoryLayer { get; }
        IDataReader DataReader { get; }
        INumericComputation<T> GetComputation<T>() where T: struct;
        T Get<T>(string name);
        void Set<T>(string name, T value);
    }

    public interface ITensorSegment<T> : IReferenceCountedMemory, IDisposable
        where T : struct
    {
        IBrightDataContext Context { get; }
        ITensorBlock<T> GetBlock(ITensorPool pool);
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
        ITensorBlock<T> Create<T>(IBrightDataContext context, uint size) where T: struct;
    }

    public interface IHaveTensorSegment<T>
        where T: struct
    {
        ITensorSegment<T> Data { get; }
    }

    public interface INumericComputation<T>
        where T: struct
    {
        ITensorSegment<T> Add(ITensorSegment<T> tensor1, ITensorSegment<T> tensor2);
        void AddInPlace(ITensorSegment<T> target, ITensorSegment<T> other);
        void AddInPlace(ITensorSegment<T> target, T scalar);
        ITensorSegment<T> Subtract(ITensorSegment<T> tensor1, ITensorSegment<T> tensor2);
        void SubtractInPlace(ITensorSegment<T> target, ITensorSegment<T> other);
        ITensorSegment<T> Multiply(ITensorSegment<T> tensor1, ITensorSegment<T> tensor2);
        void MultiplyInPlace(ITensorSegment<T> target, ITensorSegment<T> other);
        void MultiplyInPlace(ITensorSegment<T> target, T scalar);
        ITensorSegment<T> Divide(ITensorSegment<T> tensor1, ITensorSegment<T> tensor2);
        void DivideInPlace(ITensorSegment<T> target, ITensorSegment<T> other);
        T SumIndexedProducts(uint size, Func<uint, T> p1, Func<uint, T> p2);
        T DotProduct(ITensorSegment<T> segment, ITensorSegment<T> other);
        ITensorSegment<T> Sqrt(ITensorSegment<T> tensor);
        uint? Search(ITensorSegment<T> segment, T value);
        void ConstrainInPlace(ITensorSegment<T> segment, T? minValue, T? maxValue);
        T Average(ITensorSegment<T> segment);
        T L1Norm(ITensorSegment<T> segment);
        T L2Norm(ITensorSegment<T> segment);
        T Sum(ITensorSegment<T> segment);
        T CosineDistance(ITensorSegment<T> tensor, ITensorSegment<T> other);
        T EuclideanDistance(ITensorSegment<T> tensor, ITensorSegment<T> other);
        T ManhattanDistance(ITensorSegment<T> tensor, ITensorSegment<T> other);
        ITensorSegment<T> Abs(ITensorSegment<T> tensor);
        ITensorSegment<T> Log(ITensorSegment<T> tensor);
        ITensorSegment<T> Squared(ITensorSegment<T> tensor);
        T StdDev(ITensorSegment<T> segment, T? mean);
        ITensorSegment<T> Sigmoid(ITensorSegment<T> val);
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

    public interface IVector<T> : IDisposable where T : struct 
    {
        uint Size { get; }
        IVector<T> Sigmoid();
        IVector<T> Subtract(IVector<T> vector);
        IMatrix<T> Reshape(uint rows, uint columns);
        IIndexableVector<T> AsIndexable();
        void AddInPlace(IVector<T> vector);
        void AddInPlace(T scalar);
        IVector<T> Log();
        IVector<T> Clone();
        T DotProduct(IVector<T> vector);
        void MultiplyInPlace(T scalar);
    }

    public interface IIndexableVector<T> : IVector<T> where T : struct
    {
        T this[int index] { get; }
        T this[uint index] { get; }
        IEnumerable<T> Values { get; }
    }

    public interface IMatrix<T> : IDisposable where T : struct
    {
        void MultiplyInPlace(T scalar);
        IMatrix<T> Multiply(IVector<T> vector);
        IMatrix<T> Multiply(IMatrix<T> matrix);
        IVector<T> Column(uint i);
        IVector<T> Row(uint i);
        uint RowCount { get; }
        uint ColumnCount { get; }
        IIndexableMatrix<T> AsIndexable();
    }

    public interface IIndexableMatrix<T> : IMatrix<T> where T : struct
    {
        T this[int rowY, int columnX] { get; }
        T this[uint rowY, uint columnX] { get; }
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
}
