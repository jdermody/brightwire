using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.CompilerServices;
using System.IO;
using BrightData.LinearAlgebra.ReadOnly;

namespace BrightData
{
    /// <summary>
    /// Determines if the data table is oriented as either rows or columns
    /// </summary>
    public enum DataTableOrientation : byte
    {
        /// <summary>
        /// Pathological case
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Data table is stored as a series of rows
        /// </summary>
        RowOriented,

        /// <summary>
        /// Data table is stored as aa series of columns
        /// </summary>
        ColumnOriented
    }

    /// <summary>
    /// Data types enumeration
    /// </summary>
    public enum BrightDataType : byte
    {
        /// <summary>
        /// Nothing
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Boolean values
        /// </summary>
        Boolean,

        /// <summary>
        /// Signed byte values (-128 to 128)
        /// </summary>
        SByte,

        /// <summary>
        /// Short values
        /// </summary>
        Short,

        /// <summary>
        /// Integer values
        /// </summary>
        Int,

        /// <summary>
        /// Long values
        /// </summary>
        Long,

        /// <summary>
        /// Float values
        /// </summary>
        Float,

        /// <summary>
        /// Double values
        /// </summary>
        Double,

        /// <summary>
        /// Decimal values
        /// </summary>
        Decimal,

        /// <summary>
        /// String values
        /// </summary>
        String,

        /// <summary>
        /// Date values
        /// </summary>
        Date,

        /// <summary>
        /// List of indices
        /// </summary>
        IndexList,

        /// <summary>
        /// Weighted list of indices
        /// </summary>
        WeightedIndexList,

        /// <summary>
        /// Vector of floats
        /// </summary>
        Vector,

        /// <summary>
        /// Matrix of floats
        /// </summary>
        Matrix,

        /// <summary>
        /// 3D tensor of floats
        /// </summary>
        Tensor3D,

        /// <summary>
        /// 4D tensor of floats
        /// </summary>
        Tensor4D,

        /// <summary>
        /// Binary data
        /// </summary>
        BinaryData,

        /// <summary>
        /// Time only
        /// </summary>
        TimeOnly,

        /// <summary>
        /// Date only
        /// </summary>
        DateOnly
    }

    /// <summary>
    /// Column classifications
    /// </summary>
    [Flags]
    public enum ColumnClass : byte
    {
        /// <summary>
        /// Unknown category
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Forms a category - a set of possible values
        /// </summary>
        Categorical = 1,

        /// <summary>
        /// Numbers (float, int etc)
        /// </summary>
        Numeric = 2,

        /// <summary>
        /// Floating point numbers (float, double, decimal)
        /// </summary>
        Decimal = 4,

        /// <summary>
        /// Struct (blittable)
        /// </summary>
        Struct = 8,

        /// <summary>
        /// Tensor (vector, matrix etc)
        /// </summary>
        Tensor = 16,

        /// <summary>
        /// Has an index (index list, weighted index list)
        /// </summary>
        IndexBased = 32,

        /// <summary>
        /// Date and time
        /// </summary>
        DateTime = 64,

        /// <summary>
        /// Whole number
        /// </summary>
        Integer = 128
    }

    /// <summary>
    /// Single column conversion options
    /// </summary>
    public enum ColumnConversionOperation
    {
        /// <summary>
        /// Leave the column unchanged (nop)
        /// </summary>
        Unchanged = 0,

        /// <summary>
        /// Convert to boolean
        /// </summary>
        ToBoolean,

        /// <summary>
        /// Convert to date
        /// </summary>
        ToDate,

        /// <summary>
        /// Convert to numeric (best numeric size will be automatically determined)
        /// </summary>
        ToNumeric,

        /// <summary>
        /// Convert to string
        /// </summary>
        ToString,

        /// <summary>
        /// Convert to index list
        /// </summary>
        ToIndexList,

        /// <summary>
        /// Convert to weighted index list
        /// </summary>
        ToWeightedIndexList,

        /// <summary>
        /// Convert to vector
        /// </summary>
        ToVector,

        /// <summary>
        /// Convert each value to an index within a dictionary
        /// </summary>
        ToCategoricalIndex,

        /// <summary>
        /// Convert to signed byte
        /// </summary>
        ToByte,

        /// <summary>
        /// Convert to short
        /// </summary>
        ToShort,

        /// <summary>
        /// Convert to int
        /// </summary>
        ToInt,

        /// <summary>
        /// Convert to long 
        /// </summary>
        ToLong,

        /// <summary>
        /// Convert to float
        /// </summary>
        ToFloat,

        /// <summary>
        /// Convert to double
        /// </summary>
        ToDouble,

        /// <summary>
        /// Convert to decimal
        /// </summary>
        ToDecimal
    }

    /// <summary>
    /// Transforms columns
    /// </summary>
    public interface IConvertColumn : ICanConvert
    {
        /// <summary>
        /// Complete the transformation
        /// </summary>
        /// <param name="metaData">Meta data store to receive transformation information</param>
        void Finalise(MetaData metaData);
    }

    /// <summary>
    /// Typed column transformer
    /// </summary>
    /// <typeparam name="TF"></typeparam>
    /// <typeparam name="TT"></typeparam>
    public interface IConvertColumn<in TF, TT> : IConvertColumn
        where TT : notnull
        where TF : notnull
    {
        /// <summary>
        /// Writes the converted input to the buffer
        /// </summary>
        /// <param name="input"></param>
        /// <param name="buffer"></param>
        /// <param name="index">Index within the buffer</param>
        /// <returns></returns>
        bool Convert(TF input, ICompositeBuffer<TT> buffer, uint index);
    }

    /// <summary>
    /// Data table vectoriser
    /// </summary>
    public interface IDataTableVectoriser : ICanWriteToBinaryWriter, IDisposable
    {
        /// <summary>
        /// Vectorise a table row
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        float[] Vectorise(object[] row);

        /// <summary>
        /// Vectorise a data table segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        float[] Vectorise(ICanRandomlyAccessData segment);

        /// <summary>
        /// Size of the output vectors
        /// </summary>
        uint OutputSize { get; }

        /// <summary>
        /// Returns the associated label from the one hot encoding dictionary
        /// </summary>
        /// <param name="vectorIndex">Index within one hot encoded vector</param>
        /// <param name="columnIndex">Data table column index</param>
        /// <returns></returns>
        string GetOutputLabel(uint vectorIndex, uint columnIndex = 0);

        /// <summary>
        /// Returns a sequence of vectorised table rows
        /// </summary>
        /// <returns></returns>
        IEnumerable<IVector> Enumerate();
    }

    public interface IConvertObjects
    {
        T ConvertObjectTo<T>(object ret) where T : notnull;
    }

    public interface ITensorDataProvider
    {
        ReadOnlyMemory<float> GetTensorData();
    }

    public readonly record struct TableRow(uint RowIndex, object[] Values);

    public interface IDataTable : IDisposable, IHaveMetaData, ITensorDataProvider, IHaveBrightDataContext
    {
        uint RowCount { get; }
        uint ColumnCount { get; }
        DataTableOrientation Orientation { get; }
        BrightDataType[] ColumnTypes { get; }
        MetaData[] ColumnMetaData { get; }
        void PersistMetaData();
        IReadOnlyBufferWithMetaData GetColumn(uint index);
        IReadOnlyBufferWithMetaData<T> GetColumn<T>(uint index) where T : notnull;
        Task<MetaData[]> GetColumnAnalysis(params uint[] columnIndices);
        void SetTensorData(ITensorDataProvider dataProvider);
        Task<T> Get<T>(uint columnIndex, uint rowIndex) where T : notnull;
        Task<T[]> Get<T>(uint columnIndex, params uint[] rowIndices) where T : notnull;
        IAsyncEnumerable<TableRow> EnumerateRows(CancellationToken ct = default);
        IReadOnlyBufferWithMetaData[] GetColumns(params uint[] columnIndices);
        IReadOnlyBufferWithMetaData[] GetColumns(IEnumerable<uint> columnIndices);
        Task WriteColumnsTo(Stream stream, params uint[] columnIndices);
        Task WriteRowsTo(Stream stream, params uint[] rowIndices);
        Task<TableRow[]> GetRows(params uint[] rowIndices);
    }

    public interface ITempData : IDisposable, IHaveSize
    {
        public Guid Id { get; }
        void Write(ReadOnlySpan<byte> data, uint offset);
        ValueTask WriteAsync(ReadOnlyMemory<byte> data, uint offset);
        uint Read(Span<byte> data, uint offset);
        Task<uint> ReadAsync(Memory<byte> data, uint offset);
    }

    public interface IProvideTempData : IDisposable
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
    public interface IReadOnlyBufferWithMetaData<T> : IReadOnlyBuffer<T>, IReadOnlyBufferWithMetaData where T : notnull
    {
    }

    public interface IHaveDistinctItemCount
    {
        uint? DistinctItems { get; }
    }

    public interface IAppendToBuffer
    {
        void AddObject(object obj);
    }

    public interface ICompositeBuffer : IReadOnlyBufferWithMetaData, IHaveDistinctItemCount, IAppendToBuffer
    {
        public Guid Id { get; }
    }

    public interface IAppendToBuffer<T> where T: notnull
    {
        void Add(in T item);
        void Add(ReadOnlySpan<T> inputBlock);
    }

    public interface IConstraintValidator<T> where T : notnull
    {
        bool Allow(in T item);
    }

    /// <summary>
    /// Composite buffers add data in memory until a pre specified limit and then store the remainder in a temp file
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICompositeBuffer<T> : ICompositeBuffer, IReadOnlyBufferWithMetaData<T>, IAppendToBuffer<T> where T: notnull
    {
        IReadOnlySet<T>? DistinctSet { get; }
        IConstraintValidator<T>? ConstraintValidator { get; set; }
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

    public enum VectorisationType : byte
    {
        Unknown = 0,
        CategoricalIndex,
        IndexList,
        Numeric,
        OneHot,
        Tensor,
        WeightedIndexList
    }

    public interface ICanVectorise : IWriteToMetaData, IReadFromMetaData
    {
        VectorisationType Type { get; }
        uint OutputSize { get; }
        Task WriteBlock(IReadOnlyBuffer buffer, uint blockIndex, uint offset, float[,] output);
        void Vectorise(object obj, Span<float> output);
    }

    public interface IBuildDataTables : IHaveBrightDataContext
    {
        /// <summary>
        /// Table level meta data
        /// </summary>
        MetaData TableMetaData { get; }

        /// <summary>
        /// Copies existing column definitions from another table
        /// </summary>
        /// <param name="table">Other table</param>
        /// <param name="columnIndices">Indices of column definitions to copy</param>
        ICompositeBuffer[] AddColumnsFrom(IDataTable table, params uint[] columnIndices);

        /// <summary>
        /// Adds a new column
        /// </summary>
        /// <param name="type">New column type</param>
        /// <param name="name">New column name</param>
        /// <returns></returns>
        ICompositeBuffer AddColumn(BrightDataType type, string? name = null);

        /// <summary>
        /// Adds a new column
        /// </summary>
        /// <param name="type">New column type</param>
        /// <param name="metaData">Column meta data</param>
        /// <returns></returns>
        ICompositeBuffer AddColumn(BrightDataType type, MetaData metaData);

        /// <summary>
        /// Adds a new column
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">New column name</param>
        /// <returns></returns>
        ICompositeBuffer<T> AddColumn<T>(string? name = null) where T : notnull;

        /// <summary>
        /// Adds a row to the table
        /// </summary>
        /// <param name="items"></param>
        void AddRow(params object[] items);

        /// <summary>
        /// Writes the data table to a stream
        /// </summary>
        /// <param name="stream"></param>
        Task WriteTo(Stream stream);

        /// <summary>
        /// Adds a fixed size vector column
        /// </summary>
        /// <param name="size">Size of the vector</param>
        /// <param name="name">New column name</param>
        /// <returns></returns>
        ICompositeBuffer<ReadOnlyVector> AddFixedSizeVectorColumn(uint size, string? name);

        /// <summary>
        /// Adds a fixed size matrix column
        /// </summary>
        /// <param name="rows">Number of rows</param>
        /// <param name="columns">Number of columns</param>
        /// <param name="name">New column name</param>
        /// <returns></returns>
        ICompositeBuffer<ReadOnlyMatrix> AddFixedSizeMatrixColumn(uint rows, uint columns, string? name);

        /// <summary>
        /// Adds a fixed size 3D tensor column 
        /// </summary>
        /// <param name="depth">Number of matrices</param>
        /// <param name="rows">Number of rows</param>
        /// <param name="columns">Number of columns</param>
        /// <param name="name">New column name</param>
        /// <returns></returns>
        ICompositeBuffer<ReadOnlyTensor3D> AddFixedSize3DTensorColumn(uint depth, uint rows, uint columns, string? name);

        /// <summary>
        /// Adds a fixed size 4D tensor column
        /// </summary>
        /// <param name="count">Number of 3D tensors</param>
        /// <param name="depth">Number of matrices</param>
        /// <param name="rows">Number of rows</param>
        /// <param name="columns">Number of columns</param>
        /// <param name="name">New column name</param>
        /// <returns></returns>
        ICompositeBuffer<ReadOnlyTensor4D> AddFixedSize4DTensorColumn(uint count, uint depth, uint rows, uint columns, string? name);
    }

    public interface IWriteDataTables
    {
        Task Write(MetaData tableMetaData, IReadOnlyBufferWithMetaData[] buffers, Stream output);
    }
}
