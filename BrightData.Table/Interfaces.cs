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
    public interface IProvideTempStreams
    {
        SafeFileHandle Get(Guid id);
        void Clear();
    }

    public delegate void BlockCallback<T>(ReadOnlySpan<T> block);
    public delegate T CreateFromReadOnlyByteSpan<out T>(ReadOnlySpan<byte> data) where T: IHaveDataAsReadOnlyByteSpan;

    public interface ICompositeBuffer : IHaveSize, IHaveMetaData
    {
        public Guid Id { get; }
        public new MetaData MetaData { get; set; }
        public uint BlockCount { get; }
        uint? DistinctItems { get; }
        Type DataType { get; }
    }

    /// <summary>
    /// Composite buffers add data in memory until a pre specified limit and then store the remainder in a temp file
    /// * Not thread safe *
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICompositeBuffer<T> : ICompositeBuffer where T: notnull
    {
        Task ForEachBlock(BlockCallback<T> callback);
        Task<ReadOnlyMemory<T>> GetBlock(uint blockIndex);
        void Add(in T item);
        void Add(ReadOnlySpan<T> inputBlock);
    }

    public interface IByteReader : IDisposable
    {
        Task<ReadOnlyMemory<byte>> GetBlock(uint offset, uint numBytes);
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
}
