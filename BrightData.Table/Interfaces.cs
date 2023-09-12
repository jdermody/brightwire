using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance.Buffers;
using Microsoft.Win32.SafeHandles;

namespace BrightData.Table
{
    public interface IConvertObjects
    {
        T ConvertObjectTo<T>(object ret) where T : notnull;
    }

    public interface IDataTable : IDisposable, IHaveMetaData
    {
        uint RowCount { get; }
        uint ColumnCount { get; }
        DataTableOrientation Orientation { get; }
        BrightDataType[] ColumnTypes { get; }
        void PersistMetaData();
        IReadOnlyBuffer GetColumn(uint index);
        IReadOnlyBuffer<T> GetColumn<T>(uint index) where T : notnull;
        Task<MetaData[]> GetColumnAnalysis(params uint[] columnIndices);
    }

    public interface ITempData : IDisposable, IHaveSize
    {
        public Guid Id { get; }
        void Write(ReadOnlySpan<byte> data, uint offset);
        ValueTask WriteAsync(ReadOnlyMemory<byte> data, uint offset);
        uint Read(Span<byte> data, uint offset);
        Task<uint> ReadAsync(Memory<byte> data, uint offset);
    }

    public interface IProvideTempData
    {
        ITempData Get(Guid id);
        void Clear();
    }

    public delegate void BlockCallback<T>(ReadOnlySpan<T> block);
    public delegate T CreateFromReadOnlyByteSpan<out T>(ReadOnlySpan<byte> data) where T: IHaveDataAsReadOnlyByteSpan;

    public interface IReadOnlyBuffer : IHaveSize
    {
        public uint BlockSize { get; }
        public uint BlockCount { get; }
        Type DataType { get; }
        IAsyncEnumerable<object> EnumerateAll();
    }

    public interface IReadOnlyBuffer<T> : IReadOnlyBuffer where T : notnull
    {
        Task ForEachBlock(BlockCallback<T> callback, INotifyUser? notify = null, string? message = null, CancellationToken ct = default);
        Task<ReadOnlyMemory<T>> GetTypedBlock(uint blockIndex);
        IAsyncEnumerable<T> EnumerateAllTyped();
        IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken ct = default);
    }

    public interface IReadOnlyBufferWithMetaData : IReadOnlyBuffer, IHaveMetaData
    {
    }
    public interface IReadOnlyBufferWithMetaData<T> : IReadOnlyBuffer<T>, IHaveMetaData where T : notnull
    {
    }

    public interface ICompositeBuffer : IReadOnlyBufferWithMetaData
    {
        public Guid Id { get; }
        uint? DistinctItems { get; }
    }

    public interface IAppendToBuffer<T> where T: notnull
    {
        void Add(in T item);
        void Add(ReadOnlySpan<T> inputBlock);
    }

    /// <summary>
    /// Composite buffers add data in memory until a pre specified limit and then store the remainder in a temp file
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICompositeBuffer<T> : ICompositeBuffer, IReadOnlyBuffer<T>, IAppendToBuffer<T> where T: notnull
    {
        IReadOnlySet<T>? DistinctSet { get; }
    }

    public interface IByteBlockReader : IDisposable, IHaveSize
    {
        Task<ReadOnlyMemory<byte>> GetBlock(uint byteOffset, uint numBytes);
        Task Update(uint byteOffset, ReadOnlyMemory<byte> data);
    }

    internal record struct DataRangeColumnType(uint StartIndex, uint Size) : IHaveSize;

    internal record struct MatrixColumnType(uint StartIndex, uint RowCount, uint ColumnCount) : IHaveSize
    {
        public readonly uint Size => RowCount * ColumnCount;
    }

    internal record struct Tensor3DColumnType(uint StartIndex, uint Depth, uint RowCount, uint ColumnCount) : IHaveSize
    {
        public readonly uint Size => Depth * RowCount * ColumnCount;
    }

    internal record struct Tensor4DColumnType(uint StartIndex, uint Count, uint Depth, uint RowCount, uint ColumnCount) : IHaveSize
    {
        public readonly uint Size => Count * Depth * RowCount * ColumnCount;
    }

    public interface IOperation
    {
        Task Process(INotifyUser? notify = null, string? msg = null, CancellationToken ct = default);
    }

    public interface ICanIndex<T> : IHaveSize
    {
        uint GetIndex(in T item);
    }

    public interface IReadFromMetaData
    {
        void ReadFrom(MetaData metaData);
    }

    public interface ICanVectorise : IWriteToMetaData, IReadFromMetaData
    {
        uint OutputSize { get; }
        Task WriteBlock(uint blockIndex, uint offset, float[,] output);
    }
}
