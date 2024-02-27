using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using BrightData.LinearAlgebra.ReadOnly;
using BrightData.Types;
using BrightData.DataTable.Columns;
using BrightData.DataTable.Rows;

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
        /// Numbers (float, int etc.)
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
    public enum ColumnConversion : byte
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
        /// Convert to date time
        /// </summary>
        ToDateTime,

        /// <summary>
        /// Convert to date
        /// </summary>
        ToDate,

        /// <summary>
        /// Convert to time
        /// </summary>
        ToTime,

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
        ToDecimal,

        /// <summary>
        /// Custom conversion
        /// </summary>
        Custom
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

    /// <summary>
    /// Maps blocks from one type to another
    /// </summary>
    /// <typeparam name="FT"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <param name="span"></param>
    /// <returns></returns>
    public delegate Task<ReadOnlyMemory<T>> BlockMapper<FT, T>(ReadOnlyMemory<FT> span);

    /// <summary>
    /// Provides tensor data
    /// </summary>
    public interface ITensorDataProvider
    {
        /// <summary>
        /// Returns the entire block of tensor data
        /// </summary>
        /// <returns></returns>
        Task<ReadOnlyMemory<float>> GetTensorData();

        /// <summary>
        /// Sets the tensor mapping functions
        /// </summary>
        /// <param name="vectorMapper"></param>
        /// <param name="matrixMapper"></param>
        /// <param name="tensor3DMapper"></param>
        /// <param name="tensor4DMapper"></param>
        void SetTensorMappers(
            BlockMapper<DataRangeColumnType, ReadOnlyVector> vectorMapper,
            BlockMapper<MatrixColumnType, ReadOnlyMatrix> matrixMapper,
            BlockMapper<Tensor3DColumnType, ReadOnlyTensor3D> tensor3DMapper,
            BlockMapper<Tensor4DColumnType, ReadOnlyTensor4D> tensor4DMapper
        );
    }

    public partial interface IDataTable : IDisposable, IHaveMetaData, ITensorDataProvider, IHaveBrightDataContext
    {
        /// <summary>
        /// Number of rows in the table
        /// </summary>
        uint RowCount { get; }

        /// <summary>
        /// Number of columns in the table
        /// </summary>
        uint ColumnCount { get; }

        /// <summary>
        /// Data table storage orientation
        /// </summary>
        DataTableOrientation Orientation { get; }

        /// <summary>
        /// Array of the types of each column
        /// </summary>
        BrightDataType[] ColumnTypes { get; }

        /// <summary>
        /// Array of the column metadata
        /// </summary>
        MetaData[] ColumnMetaData { get; }

        /// <summary>
        /// Persists changes in the metadata to the underlying storage
        /// </summary>
        void PersistMetaData();

        /// <summary>
        /// Returns a column
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        IReadOnlyBufferWithMetaData GetColumn(uint index);

        /// <summary>
        /// Returns a typed column
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index"></param>
        /// <returns></returns>
        IReadOnlyBufferWithMetaData<T> GetColumn<T>(uint index) where T : notnull;

        /// <summary>
        /// Returns a typed column after applying a conversion function to each value
        /// </summary>
        /// <param name="index"></param>
        /// <param name="converter"></param>
        /// <typeparam name="FT"></typeparam>
        /// <typeparam name="TT"></typeparam>
        /// <returns></returns>
        IReadOnlyBufferWithMetaData<TT> GetColumn<FT, TT>(uint index, Func<FT, TT> converter) where FT : notnull where TT : notnull;

        /// <summary>
        /// Returns column analysis of the specified columns (or all if none specified)
        /// </summary>
        /// <param name="columnIndices"></param>
        /// <returns></returns>
        Task<MetaData[]> GetColumnAnalysis(params uint[] columnIndices);

        /// <summary>
        /// Returns a typed value from the data table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnIndex"></param>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        Task<T> Get<T>(uint columnIndex, uint rowIndex) where T : notnull;

        /// <summary>
        /// Returns an array of column values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnIndex"></param>
        /// <param name="rowIndices"></param>
        /// <returns></returns>
        Task<T[]> Get<T>(uint columnIndex, params uint[] rowIndices) where T : notnull;

        /// <summary>
        /// Enumerates all rows of the table
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        IAsyncEnumerable<GenericTableRow> EnumerateRows(CancellationToken ct = default);

        /// <summary>
        /// Returns an array of columns
        /// </summary>
        /// <param name="columnIndices"></param>
        /// <returns></returns>
        IReadOnlyBufferWithMetaData[] GetColumns(IEnumerable<uint> columnIndices);

        /// <summary>
        /// Writes the specified columns to a stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="columnIndices"></param>
        /// <returns></returns>
        Task WriteColumnsTo(Stream stream, params uint[] columnIndices);

        /// <summary>
        /// Writes the specified rows to a stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="rowIndices"></param>
        /// <returns></returns>
        Task WriteRowsTo(Stream stream, params uint[] rowIndices);

        /// <summary>
        /// Returns an array of rows
        /// </summary>
        /// <param name="rowIndices"></param>
        /// <returns></returns>
        Task<GenericTableRow[]> GetRows(params uint[] rowIndices);

        /// <summary>
        /// Returns a row from the table
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        GenericTableRow this[uint index] { get; }
    }

    /// <summary>
    /// Generic block byte data source
    /// </summary>
    public interface IByteBlockSource : IDisposable, IHaveSize
    {
        /// <summary>
        /// Unique id of the source
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Writes data to the source
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        void Write(ReadOnlySpan<byte> data, uint offset);

        /// <summary>
        /// Async writes data to the source
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        ValueTask WriteAsync(ReadOnlyMemory<byte> data, uint offset);

        /// <summary>
        /// Reads data from the source
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        uint Read(Span<byte> data, uint offset);

        /// <summary>
        /// Async read from the source
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        Task<uint> ReadAsync(Memory<byte> data, uint offset);
    }

    /// <summary>
    /// Provides data blocks
    /// </summary>
    public interface IProvideDataBlocks : IDisposable
    {
        /// <summary>
        /// Returns a data block associated with the id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IByteBlockSource Get(Guid id);

        /// <summary>
        /// Clears all data blocks
        /// </summary>
        void Clear();
    }

    /// <summary>
    /// Callback for a block
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="block"></param>
    public delegate void BlockCallback<T>(ReadOnlySpan<T> block);

    /// <summary>
    /// Creates a type from a block of bytes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <returns></returns>
    public delegate T CreateFromReadOnlyByteSpan<out T>(ReadOnlySpan<byte> data) where T: IHaveDataAsReadOnlyByteSpan;

    /// <summary>
    /// Read only buffer - composed of multiple blocks of a fixed size
    /// </summary>
    public interface IReadOnlyBuffer : IHaveSize
    {
        /// <summary>
        /// Size of each block in the buffer
        /// </summary>
        public uint BlockSize { get; }

        /// <summary>
        /// Number of blocks in the buffer
        /// </summary>
        public uint BlockCount { get; }

        /// <summary>
        /// The type of data within the buffer
        /// </summary>
        Type DataType { get; }

        /// <summary>
        /// Enumerates all objects within the buffer
        /// </summary>
        /// <returns></returns>
        IAsyncEnumerable<object> EnumerateAll();

        /// <summary>
        /// Returns the specified block as an array
        /// </summary>
        /// <param name="blockIndex"></param>
        /// <returns></returns>
        Task<Array> GetBlock(uint blockIndex);
    }

    /// <summary>
    /// Typed read only buffer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReadOnlyBuffer<T> : IReadOnlyBuffer where T : notnull
    {
        /// <summary>
        /// Executes a callback on each block within the buffer
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="notify"></param>
        /// <param name="message"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task ForEachBlock(BlockCallback<T> callback, INotifyOperationProgress? notify = null, string? message = null, CancellationToken ct = default);

        /// <summary>
        /// Returns a block from the buffer
        /// </summary>
        /// <param name="blockIndex"></param>
        /// <returns></returns>
        Task<ReadOnlyMemory<T>> GetTypedBlock(uint blockIndex);

        /// <summary>
        /// Enumerates all values in the buffer
        /// </summary>
        /// <returns></returns>
        IAsyncEnumerable<T> EnumerateAllTyped();

        /// <summary>
        /// Enumerates all values in the buffer
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken ct = default);
    }

    /// <summary>
    /// A read only buffer with metadata
    /// </summary>
    public interface IReadOnlyBufferWithMetaData : IReadOnlyBuffer, IHaveMetaData;

    /// <summary>
    /// A typed read only buffer with metadata
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReadOnlyBufferWithMetaData<T> : IReadOnlyBuffer<T>, IReadOnlyBufferWithMetaData where T : notnull;

    /// <summary>
    /// Indicates that the type counts distinct items
    /// </summary>
    public interface IHaveDistinctItemCount
    {
        /// <summary>
        /// Count of distinct items (or null if the count was not found)
        /// </summary>
        uint? DistinctItems { get; }
    }

    /// <summary>
    /// Appends an object to a buffer
    /// </summary>
    public interface IAppendToBuffer : IAppendBlocks
    {
        /// <summary>
        /// Appends an object
        /// </summary>
        /// <param name="obj"></param>
        void AppendObject(object obj);
    }

    /// <summary>
    /// Appends a typed value to a buffer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAppendToBuffer<T> : IAppendBlocks<T>, IAppendToBuffer where T: notnull
    {
        /// <summary>
        /// Appends a value
        /// </summary>
        /// <param name="item"></param>
        void Append(in T item);
    }

    /// <summary>
    /// Composite buffers are appendable buffers that track distinct items and are writeable to streams
    /// </summary>
    public interface ICompositeBuffer : IReadOnlyBufferWithMetaData, IHaveDistinctItemCount, IAppendToBuffer
    {
        /// <summary>
        /// Unique id
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Writes all data to a stream
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        Task WriteTo(Stream stream);
    }

    

    /// <summary>
    /// Checks if an item satisfies a constraint
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IConstraintValidator<T> where T : notnull
    {
        /// <summary>
        /// Returns true if the item was valid
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool Allow(in T item);
    }

    /// <summary>
    /// Composite buffers add data in memory until a pre-specified limit is reached and then stores the remainder in a temp file
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICompositeBuffer<T> : ICompositeBuffer, IReadOnlyBufferWithMetaData<T>, IAppendToBuffer<T> where T: notnull
    {
        /// <summary>
        /// The distinct (unique) set of items that have been added - null if the count exceeded the pre-defined limit
        /// </summary>
        IReadOnlySet<T>? DistinctSet { get; }

        /// <summary>
        /// Optional constraint validator on items that are added to the buffer
        /// </summary>
        IConstraintValidator<T>? ConstraintValidator { get; set; }
    }

    /// <summary>
    /// Reads blocks of data
    /// </summary>
    public interface IByteBlockReader : IDisposable, IHaveSize
    {
        /// <summary>
        /// Reads a block of data
        /// </summary>
        /// <param name="byteOffset">Byte offset to read</param>
        /// <param name="numBytes">Size in bytes to read</param>
        /// <returns></returns>
        Task<ReadOnlyMemory<byte>> GetBlock(uint byteOffset, uint numBytes);

        /// <summary>
        /// Writes a block of data to the buffer
        /// </summary>
        /// <param name="byteOffset">Byte offset at which to write</param>
        /// <param name="data">Data to write</param>
        /// <returns></returns>
        Task Update(uint byteOffset, ReadOnlyMemory<byte> data);
    }

    /// <summary>
    /// Represents a potentially long-running operation
    /// </summary>
    public interface IOperation
    {
        /// <summary>
        /// Executes the operation
        /// </summary>
        /// <param name="notify">Notify about progress (optional)</param>
        /// <param name="msg">Message to notification when operation starts (optional)</param>
        /// <param name="ct">Cancellation token (optional)</param>
        /// <returns></returns>
        Task Execute(INotifyOperationProgress? notify = null, string? msg = null, CancellationToken ct = default);
    }

    /// <summary>
    /// Maps objects to a consistent index
    /// </summary>
    public interface ICanIndex : IHaveSize
    {
        /// <summary>
        /// Returns the mapping
        /// </summary>
        /// <returns></returns>
        Dictionary<string, uint> GetMapping();
    }

    /// <summary>
    /// Maps objects of type T to an index
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICanIndex<T> : ICanIndex
    {
        /// <summary>
        /// Returns the index associated with the object
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        uint GetIndex(in T item);
    }

    /// <summary>
    /// Type of vectorisation
    /// </summary>
    public enum VectorisationType : byte
    {
        /// <summary>
        /// Undefined
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Maps values to an index (single offset)
        /// </summary>
        CategoricalIndex,

        /// <summary>
        /// Index lists vectorisation
        /// </summary>
        IndexList,

        /// <summary>
        /// Numeric vectorisation
        /// </summary>
        Numeric,

        /// <summary>
        /// Maps values to an index (each value will have its own offset)
        /// </summary>
        OneHot,
        
        /// <summary>
        /// Tensor vectorisation
        /// </summary>
        Tensor,

        /// <summary>
        /// Weighted index list vectorisation
        /// </summary>
        WeightedIndexList,

        /// <summary>
        /// Boolean vectorisation
        /// </summary>
        Boolean
    }

    /// <summary>
    /// Object vectorisation
    /// </summary>
    public interface ICanVectorise : IWriteToMetaData
    {
        /// <summary>
        /// Type of vectorisation
        /// </summary>
        VectorisationType Type { get; }

        /// <summary>
        /// Size of the vectorisation output
        /// </summary>
        uint OutputSize { get; }

        /// <summary>
        /// Vectorise a block from the buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="blockIndex"></param>
        /// <param name="offset"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        Task WriteBlock(IReadOnlyBuffer buffer, uint blockIndex, uint offset, float[,] output);

        /// <summary>
        /// Vectorise an object
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="output"></param>
        void Vectorise(object obj, Span<float> output);

        /// <summary>
        /// Maps back from a vectorised index to the source value
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        string? ReverseVectorise(uint index);

        /// <summary>
        /// Reads vectorisation parameters from metadata
        /// </summary>
        /// <param name="metaData"></param>
        void ReadFrom(MetaData metaData);
    }

    /// <summary>
    /// Builds data tables
    /// </summary>
    public interface IBuildDataTables : IHaveBrightDataContext
    {
        /// <summary>
        /// Table level meta data
        /// </summary>
        MetaData TableMetaData { get; }

        /// <summary>
        /// Per column meta data
        /// </summary>
        MetaData[] ColumnMetaData { get; }

        /// <summary>
        /// Number of output rows
        /// </summary>
        uint RowCount { get; }

        /// <summary>
        /// Number of output columns
        /// </summary>
        uint ColumnCount { get; }

        /// <summary>
        /// Copies existing column definitions from another table
        /// </summary>
        /// <param name="table">Other table</param>
        /// <param name="columnIndices">Indices of column definitions to copy</param>
        ICompositeBuffer[] CreateColumnsFrom(IDataTable table, params uint[] columnIndices);

        /// <summary>
        /// Creates columns from buffers
        /// </summary>
        /// <param name="buffers"></param>
        /// <returns></returns>
        ICompositeBuffer[] CreateColumnsFrom(IEnumerable<IReadOnlyBufferWithMetaData> buffers);

        /// <summary>
        /// Adds a new column
        /// </summary>
        /// <param name="type">New column type</param>
        /// <param name="name">New column name</param>
        /// <returns></returns>
        ICompositeBuffer CreateColumn(BrightDataType type, string? name = null);

        /// <summary>
        /// Adds a new column
        /// </summary>
        /// <param name="type">New column type</param>
        /// <param name="metaData">Column meta data</param>
        /// <returns></returns>
        ICompositeBuffer CreateColumn(BrightDataType type, MetaData metaData);

        /// <summary>
        /// Adds a new column
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">New column name</param>
        /// <returns></returns>
        ICompositeBuffer<T> CreateColumn<T>(string? name = null) where T : notnull;

        /// <summary>
        /// Adds a row to the table
        /// </summary>
        /// <param name="items"></param>
        void AddRow(params object[] items);

        /// <summary>
        /// Adds rows from the buffers
        /// </summary>
        /// <param name="buffers"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task AddRows(IReadOnlyList<IReadOnlyBuffer> buffers, CancellationToken ct = default);

        /// <summary>
        /// Adds rows from the buffers
        /// </summary>
        /// <param name="buffers"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public Task AddRows(IReadOnlyList<IReadOnlyBufferWithMetaData> buffers, CancellationToken ct = default);

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
        ICompositeBuffer<ReadOnlyVector> CreateFixedSizeVectorColumn(uint size, string? name);

        /// <summary>
        /// Adds a fixed size matrix column
        /// </summary>
        /// <param name="rows">Number of rows</param>
        /// <param name="columns">Number of columns</param>
        /// <param name="name">New column name</param>
        /// <returns></returns>
        ICompositeBuffer<ReadOnlyMatrix> CreateFixedSizeMatrixColumn(uint rows, uint columns, string? name);

        /// <summary>
        /// Adds a fixed size 3D tensor column 
        /// </summary>
        /// <param name="depth">Number of matrices</param>
        /// <param name="rows">Number of rows</param>
        /// <param name="columns">Number of columns</param>
        /// <param name="name">New column name</param>
        /// <returns></returns>
        ICompositeBuffer<ReadOnlyTensor3D> CreateFixedSize3DTensorColumn(uint depth, uint rows, uint columns, string? name);

        /// <summary>
        /// Adds a fixed size 4D tensor column
        /// </summary>
        /// <param name="count">Number of 3D tensors</param>
        /// <param name="depth">Number of matrices</param>
        /// <param name="rows">Number of rows</param>
        /// <param name="columns">Number of columns</param>
        /// <param name="name">New column name</param>
        /// <returns></returns>
        ICompositeBuffer<ReadOnlyTensor4D> CreateFixedSize4DTensorColumn(uint count, uint depth, uint rows, uint columns, string? name);
    }

    /// <summary>
    /// Writes tables to a stream
    /// </summary>
    public interface IWriteDataTables
    {
        /// <summary>
        /// Writes to a stream
        /// </summary>
        /// <param name="tableMetaData"></param>
        /// <param name="buffers"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        Task Write(MetaData tableMetaData, IReadOnlyBufferWithMetaData[] buffers, Stream output);
    }
}
