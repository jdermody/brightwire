using System;
using System.Collections.Generic;
using BrightData.LinearAlgebra;

namespace BrightData
{
    /// <summary>
    /// Determines if the data table is oriented as either rows or columns
    /// </summary>
    public enum DataTableOrientation
    {
        /// <summary>
        /// Pathological case
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Data table is a series of rows, optimised for per-row processing
        /// </summary>
        RowOriented,

        /// <summary>
        /// Data table is a series of columns, optimised for column based processing
        /// </summary>
        ColumnOriented
    }

    /// <summary>
    /// Segment table column type
    /// </summary>
    public enum ColumnType : byte
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
        /// Byte values (-128 to 128)
        /// </summary>
        Byte,

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
        BinaryData
    }

    /// <summary>
    /// Column classifications
    /// </summary>
    [Flags]
    public enum ColumnClass : byte
    {
#pragma warning disable 1591
        Unknown = 0,
        Categorical = 1,
        Numeric = 2,
        Decimal = 4,
        Structable = 8,
        Tensor = 16,
        IndexBased = 32,
        Continuous = 64,
        Integer = 128
#pragma warning restore 1591
    }

    /// <summary>
    /// A segment (series of values) in a table of which each element has the same type
    /// </summary>
    public interface ISingleTypeTableSegment : IHaveMetaData, ICanWriteToBinaryWriter, IDisposable
    {
        /// <summary>
        /// The single type of the segment
        /// </summary>
        ColumnType SingleType { get; }

        /// <summary>
        /// Enumerate each item (casting to object)
        /// </summary>
        /// <returns></returns>
        IEnumerable<object> Enumerate();

        /// <summary>
        /// Number of values (size of segment)
        /// </summary>
        uint Size { get; }
    }

    /// <summary>
    /// A series of values from a data table
    /// </summary>
    public interface IDataTableSegment
    {
        /// <summary>
        /// Number of values (size of segment)
        /// </summary>
        uint Size { get; }

        /// <summary>
        /// The column type of each value
        /// </summary>
        ColumnType[] Types { get; }

        /// <summary>
        /// The value at each index (cast to object)
        /// </summary>
        /// <param name="index">Index to retrieve</param>
        object this[uint index] { get; }
    }

    /// <summary>
    /// Typed data table segment (all of the same type)
    /// </summary>
    /// <typeparam name="T">Data type of values within the segment</typeparam>
    public interface IDataTableSegment<out T> : ISingleTypeTableSegment, IHaveDataContext
        where T : notnull
    {
        /// <summary>
        /// Enumerates the values
        /// </summary>
        /// <returns></returns>
        IEnumerable<T> EnumerateTyped();
    }

    /// <summary>
    /// A data table is an immutable collection of data with columns and rows (in which the columns have the same type of data)
    /// </summary>
    public interface IDataTable : IHaveMetaData, IDisposable, IHaveDataContext
    {
        /// <summary>
        /// Number of rows
        /// </summary>
        uint RowCount { get; }

        /// <summary>
        /// Number of columns
        /// </summary>
        uint ColumnCount { get; }

        /// <summary>
        /// The type of each column
        /// </summary>
        ColumnType[] ColumnTypes { get; }

        /// <summary>
        /// How the table is aligned (either row or column)
        /// </summary>
        DataTableOrientation Orientation { get; }

        /// <summary>
        /// Invokes the callback on each row of the data table
        /// </summary>
        /// <param name="callback">Callback for each row</param>
        /// <param name="maxRows">Maximum number of rows to process</param>
        void ForEachRow(Action<object[], uint> callback, uint maxRows = uint.MaxValue);

        /// <summary>
        /// Invokes the callback on each row of the data table
        /// </summary>
        /// <param name="callback">Callback for each row</param>
        /// <param name="maxRows">Maximum number of rows to process</param>
        void ForEachRow(Action<object[]> callback, uint maxRows = uint.MaxValue);

        /// <summary>
        /// Enumerates the columns of the data table
        /// </summary>
        /// <param name="columnIndices">Column indices to retrieve</param>
        IEnumerable<ISingleTypeTableSegment> Columns(params uint[] columnIndices);

        /// <summary>
        /// Consumes data in the table via an array of consumers, which will each consume data for each row in the table
        /// </summary>
        /// <param name="consumers">Array of consumers, for each column in the table</param>
        /// <param name="maxRows">Maximum number of rows to process</param>
        void ReadTyped(IEnumerable<IConsumeColumnData> consumers, uint maxRows = uint.MaxValue);

        /// <summary>
        /// Returns a single column from the table
        /// </summary>
        /// <param name="columnIndex">Column index to retrieve</param>
        ISingleTypeTableSegment Column(uint columnIndex);

        /// <summary>
        /// Returns analysed metadata for the specified columns
        /// </summary>
        /// <param name="columnIndices">Column indices</param>
        /// <returns></returns>
        IEnumerable<(uint ColumnIndex, IMetaData MetaData)> ColumnAnalysis(IEnumerable<uint> columnIndices);

        /// <summary>
        /// Returns the metadata for a single column (without analysis)
        /// </summary>
        /// <param name="columnIndex">Column index to retrieve</param>
        /// <returns></returns>
        IMetaData ColumnMetaData(uint columnIndex);

        /// <summary>
        /// Applies a projection function to each row of this data table
        /// </summary>
        /// <param name="projector">Mutates the rows of each row by changing values, types, or both</param>
        /// <param name="filePath">File path to store new table on disk (optional)</param>
        IRowOrientedDataTable? Project(Func<object[], object[]?> projector, string? filePath = null);
    }

    /// <summary>
    /// Column oriented data table
    /// </summary>
    public interface IColumnOrientedDataTable : IDataTable
    {
        /// <summary>
        /// Converts to a row oriented data table
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        IRowOrientedDataTable AsRowOriented(string? filePath = null);

        /// <summary>
        /// Creates a new table with columns that have been converted
        /// </summary>
        /// <param name="conversion">Column conversion parameters</param>
        /// <returns></returns>
        IColumnOrientedDataTable Convert(params IColumnTransformationParam[] conversion);

        /// <summary>
        /// Creates a new table with columns that have been converted
        /// </summary>
        /// <param name="filePath">File path to store new table on disk</param>
        /// <param name="conversion">Column conversion parameters</param>
        /// <returns></returns>
        IColumnOrientedDataTable Convert(string? filePath, params IColumnTransformationParam[] conversion);

        /// <summary>
        /// Normalizes the data in all columns of the table
        /// </summary>
        /// <param name="type">Normalization type</param>
        /// <param name="filePath">File path to store new table on disk (optional)</param>
        /// <returns></returns>
        IColumnOrientedDataTable Normalize(NormalizationType type, string? filePath = null);

        /// <summary>
        /// Normalizes the data in all columns of the table
        /// </summary>
        /// <param name="conversion">Column normalization parameters</param>
        /// <returns></returns>
        IColumnOrientedDataTable Normalize(params IColumnTransformationParam[] conversion);

        /// <summary>
        /// Normalizes the data in all columns of the table
        /// </summary>
        /// <param name="filePath">File path to store new table on disk</param>
        /// <param name="conversion">Column normalization parameters</param>
        /// <returns></returns>
        IColumnOrientedDataTable Normalize(string? filePath, params IColumnTransformationParam[] conversion);

        /// <summary>
        /// Copies the selected columns to a new data table
        /// </summary>
        /// <param name="columnIndices">Column indices to copy</param>
        /// <returns></returns>
        IColumnOrientedDataTable CopyColumns(params uint[] columnIndices);

        /// <summary>
        /// Copies the selected columns to a new data table
        /// </summary>
        /// <param name="filePath">File path to store new table on disk</param>
        /// <param name="columnIndices">Column indices to copy</param>
        /// <returns></returns>
        IColumnOrientedDataTable CopyColumns(string? filePath, params uint[] columnIndices);

        /// <summary>
        /// Creates a new data table with this concatenated with other column oriented data tables
        /// </summary>
        /// <param name="others">Other tables to concatenate</param>
        IColumnOrientedDataTable ConcatColumns(params IColumnOrientedDataTable[] others);

        /// <summary>
        /// Creates a new data table with this concatenated with other column oriented data tables
        /// </summary>
        /// <param name="filePath">File path to store new table on disk</param>
        /// <param name="others">Other tables to concatenate</param>
        /// <returns></returns>
        IColumnOrientedDataTable ConcatColumns(string? filePath, params IColumnOrientedDataTable[] others);

        /// <summary>
        /// Creates a new data table of the rows that match the predicate
        /// </summary>
        /// <param name="predicate">Predicate function to evaluate which rows to include in the new table</param>
        /// <param name="filePath">File path to store new table on disk (optional)</param>
        /// <returns></returns>
        IColumnOrientedDataTable FilterRows(Predicate<object[]> predicate, string? filePath = null);

        /// <summary>
        /// Many to one or one to many style column transformations
        /// </summary>
        /// <param name="columns">Parameters to determine which columns are reinterpreted</param>
        /// <returns></returns>
        IColumnOrientedDataTable ReinterpretColumns(params IReinterpretColumnsParam[] columns);

        /// <summary>
        /// Many to one or one to many style column transformations
        /// </summary>
        /// <param name="filePath">File path to store new table on disk</param>
        /// <param name="columns">Parameters to determine which columns are reinterpreted</param>
        /// <returns></returns>
        IColumnOrientedDataTable ReinterpretColumns(string? filePath, params IReinterpretColumnsParam[] columns);

        /// <summary>
        /// Clones the current data table
        /// </summary>
        /// <param name="filePath">File path to store new table on disk (optional)</param>
        /// <returns></returns>
        IColumnOrientedDataTable Clone(string? filePath);

        /// <summary>
        /// Returns the metadata for a single column after performing analysis on the column
        /// </summary>
        /// <param name="columnIndex">Column index to retrieve</param>
        /// <param name="force">True to force metadata analysis</param>
        /// <param name="writeCount">Maximum size of sequences to write in final meta data</param>
        /// <param name="maxCount">Maximum number of distinct items to track</param>
        /// <returns></returns>
        IMetaData ColumnAnalysis(uint columnIndex, bool force = false, uint writeCount = Consts.MaxWriteCount, uint maxCount = Consts.MaxDistinct);
    }

    /// <summary>
    /// Row oriented data table
    /// </summary>
    public interface IRowOrientedDataTable : IDataTable
    {
        /// <summary>
        /// Converts to a column oriented data table
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        IColumnOrientedDataTable AsColumnOriented(string? filePath = null);

        /// <summary>
        /// Invokes the callback on each specified row of the table
        /// </summary>
        /// <param name="rowIndices">Row indices to select</param>
        /// <param name="callback">Callback to invoke on each selected row</param>
        void ForEachRow(IEnumerable<uint> rowIndices, Action<object[]> callback);

        /// <summary>
        /// Samples (with replacement) from the data table
        /// </summary>
        /// <param name="sampleCount">Number of rows to sample</param>
        /// <param name="filePath">File path to store new table on disk (optional)</param>
        /// <returns></returns>
        IRowOrientedDataTable Bag(uint sampleCount, string? filePath = null);

        /// <summary>
        /// Returns the row at the specified index
        /// </summary>
        /// <param name="rowIndex">Row index to retrieve</param>
        IDataTableSegment Row(uint rowIndex);

        /// <summary>
        /// Returns the rows at the specified indices
        /// </summary>
        /// <param name="rowIndices">Row indices to retrieve</param>
        /// <returns></returns>
        IEnumerable<IDataTableSegment> Rows(params uint[] rowIndices);

        /// <summary>
        /// Creates a new table of this concatenated with other row oriented data tables
        /// </summary>
        /// <param name="others">Other row oriented data tables to concatenate</param>
        /// <returns></returns>
        IRowOrientedDataTable Concat(params IRowOrientedDataTable[] others);

        /// <summary>
        /// Creates a new table of this concatenated with other row oriented data tables
        /// </summary>
        /// <param name="filePath">File path to store new table on disk (optional)</param>
        /// <param name="others">Other row oriented data tables to concatenate</param>
        /// <returns></returns>
        IRowOrientedDataTable Concat(string? filePath, params IRowOrientedDataTable[] others);

        /// <summary>
        /// Copy specified rows from this to a new data table
        /// </summary>
        /// <param name="rowIndices">Row indices to copy</param>
        /// <returns></returns>
        IRowOrientedDataTable CopyRows(params uint[] rowIndices);

        /// <summary>
        /// Copy specified rows from this to a new data table
        /// </summary>
        /// <param name="filePath">File path to store new table on disk (optional)</param>
        /// <param name="rowIndices">Row indices to copy</param>
        /// <returns></returns>
        IRowOrientedDataTable CopyRows(string? filePath, params uint[] rowIndices);

        /// <summary>
        /// Creates a new data table from the randomly shuffled rows of this data table
        /// </summary>
        /// <param name="filePath">File path to store new table on disk (optional)</param>
        /// <returns></returns>
        IRowOrientedDataTable Shuffle(string? filePath = null);

        /// <summary>
        /// Creates a new sorted data table
        /// </summary>
        /// <param name="columnIndex">Column index to sort</param>
        /// <param name="ascending">True to sort ascending</param>
        /// <param name="filePath">File path to store new table on disk (optional)</param>
        /// <returns></returns>
        IRowOrientedDataTable Sort(uint columnIndex, bool ascending, string? filePath = null);

        /// <summary>
        /// Splits this table into many data tables based on the value from a column
        /// </summary>
        /// <param name="columnIndex">Column index to group on</param>
        /// <returns></returns>
        IEnumerable<(string Label, IRowOrientedDataTable Table)> GroupBy(uint columnIndex);

        /// <summary>
        /// Returns the first row as a string
        /// </summary>
        string FirstRow { get; }

        /// <summary>
        /// Returns the second row as a string
        /// </summary>
        string SecondRow { get; }

        /// <summary>
        /// Returns the third row as a string
        /// </summary>
        string ThirdRow { get; }

        /// <summary>
        /// Returns the last row as a string
        /// </summary>
        string LastRow { get; }

        /// <summary>
        /// Clones the current data table
        /// </summary>
        /// <param name="filePath">File path to store new table on disk (optional)</param>
        /// <returns></returns>
        IRowOrientedDataTable Clone(string? filePath = null);
    }

    /// <summary>
    /// Single column conversion options
    /// </summary>
    public enum ColumnConversionType
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
        ToCategoricalIndex
    }

    /// <summary>
    /// Transforms columns
    /// </summary>
    public interface ITransformColumn : ICanConvert
    {
        /// <summary>
        /// Complete the transformation
        /// </summary>
        /// <param name="metaData">Meta data store to receive transformation information</param>
        void Finalise(IMetaData metaData);
    }

    /// <summary>
    /// Typed column transformer
    /// </summary>
    /// <typeparam name="TF"></typeparam>
    /// <typeparam name="TT"></typeparam>
    public interface ITransformColumn<in TF, TT> : ITransformColumn
        where TT : notnull
        where TF : notnull
    {
        /// <summary>
        /// Writes the converted input to the buffer
        /// </summary>
        /// <param name="input"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        bool Convert(TF input, IHybridBuffer<TT> buffer);
    }

    /// <summary>
    /// Transforms a column
    /// </summary>
    internal interface ITransformationContext
    {
        /// <summary>
        /// Performs the transformation
        /// </summary>
        /// <returns></returns>
        uint Transform();

        /// <summary>
        /// Buffer that is written to
        /// </summary>
        IHybridBuffer Buffer { get; }
    }

    /// <summary>
    /// Table column information
    /// </summary>
    public interface IColumnInfo : IHaveMetaData
    {
        /// <summary>
        /// Column index
        /// </summary>
        public uint Index { get; }

        /// <summary>
        /// Column type
        /// </summary>
        ColumnType ColumnType { get; }
    }

    /// <summary>
    /// Informtion about a column transformation
    /// </summary>
    public interface IColumnTransformationParam
    {
        /// <summary>
        /// Column index
        /// </summary>
        public uint? ColumnIndex { get; }

        /// <summary>
        /// Gets a column transformer
        /// </summary>
        /// <param name="fromType">Convert from column type</param>
        /// <param name="column">Column to convert</param>
        /// <param name="analysedMetaData">Function to produce analysed column meta data if needed</param>
        /// <param name="tempStreams">Temp stream provider</param>
        /// <param name="inMemoryRowCount">Number of rows to cache in memory</param>
        /// <returns></returns>
        public ITransformColumn? GetTransformer(ColumnType fromType, ISingleTypeTableSegment column, Func<IMetaData> analysedMetaData, IProvideTempStreams tempStreams, uint inMemoryRowCount = 32768);
    }

    /// <summary>
    /// A table that is easily convertible to other types
    /// </summary>
    public interface IConvertibleTable
    {
        /// <summary>
        /// Returns a row that can be easily converted
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        IConvertibleRow Row(uint index);

        /// <summary>
        /// Returns rows that can be easily converted
        /// </summary>
        /// <param name="rowIndices">Row indices to return</param>
        /// <returns></returns>
        IEnumerable<IConvertibleRow> Rows(params uint[] rowIndices);

        /// <summary>
        /// The underlying data table
        /// </summary>
        IRowOrientedDataTable DataTable { get; }

        /// <summary>
        /// Maps each row
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rowMapper">Callback that will be invoked on each convertible row</param>
        /// <returns></returns>
        IEnumerable<T> Map<T>(Func<IConvertibleRow, T> rowMapper) where T : notnull;

        /// <summary>
        /// Invokes a callback on each convertible row
        /// </summary>
        /// <param name="action">Callback</param>
        void ForEachRow(Action<IConvertibleRow> action);
    }

    /// <summary>
    /// Indicates that the type has a data table
    /// </summary>
    public interface IHaveDataTable
    {
        /// <summary>
        /// Data table
        /// </summary>
        IDataTable DataTable { get; }
    }

    /// <summary>
    /// A row that whose elements can be converted to other types
    /// </summary>
    public interface IConvertibleRow : IHaveDataTable
    {
        /// <summary>
        /// Gets an element
        /// </summary>
        /// <param name="index">Column index</param>
        /// <returns></returns>
        object Get(uint index);

        /// <summary>
        /// Returns the row segment
        /// </summary>
        IDataTableSegment Segment { get; }

        /// <summary>
        /// Returns a value (dynamic conversion to type T)
        /// </summary>
        /// <typeparam name="T">Type to convert to</typeparam>
        /// <param name="index">Column index</param>
        /// <returns></returns>
        T GetTyped<T>(uint index) where T : notnull;

        /// <summary>
        /// Row index
        /// </summary>
        uint RowIndex { get; }
    }

    /// <summary>
    /// Interface that 
    /// </summary>
    public interface IConsumeColumnData
    {
        /// <summary>
        /// Column index that will be consumed
        /// </summary>
        uint ColumnIndex { get; }

        /// <summary>
        /// Column type of incoming data
        /// </summary>
        ColumnType ColumnType { get; }
    }

    /// <summary>
    /// Typed column consumer that writes to a buffer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IConsumeColumnData<in T> : IConsumeColumnData, IAppendableBuffer<T>
        where T : notnull
    {
    }

    /// <summary>
    /// Data table vectoriser
    /// </summary>
    public interface IDataTableVectoriser
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
        float[] Vectorise(IDataTableSegment segment);

        /// <summary>
        /// Size of the output vectors
        /// </summary>
        uint OutputSize { get; }

        string GetOutputLabel(uint columnIndex, uint vectorIndex);

        /// <summary>
        /// Returns a sequence of vectorised table rows
        /// </summary>
        /// <returns></returns>
        IEnumerable<Vector<float>> Enumerate();
    }

    /// <summary>
    /// Reinterpret columns parameters
    /// </summary>
    public interface IReinterpretColumnsParam
    {
        /// <summary>
        /// Source column indices
        /// </summary>
        uint[] ColumnIndices { get; }

        /// <summary>
        /// Gets new columns
        /// </summary>
        /// <param name="context">Bright data context</param>
        /// <param name="tempStreams">Temp stream provider</param>
        /// <param name="initialColumnIndex">First column index in the sequence</param>
        /// <param name="columns">Source column data</param>
        /// <returns></returns>
        IEnumerable<ISingleTypeTableSegment> GetNewColumns(IBrightDataContext context, IProvideTempStreams tempStreams, uint initialColumnIndex, (IColumnInfo Info, ISingleTypeTableSegment Segment)[] columns);
    }
}
