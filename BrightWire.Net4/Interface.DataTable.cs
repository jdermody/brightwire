using BrightWire.Models;
using BrightWire.Models.DataTable;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire
{
    /// <summary>
    /// Data normalisation options
    /// </summary>
    public enum NormalisationType
    {
        /// <summary>
        /// Normalises based on standard deviation
        /// </summary>
        Standard,

        /// <summary>
        /// Normalise from manhattan distance
        /// </summary>
        Manhattan,

        /// <summary>
        /// Normalise from eucildean distance
        /// </summary>
        Euclidean,

        /// <summary>
        /// Normalise based on min and max values
        /// </summary>
        FeatureScale,
    }

    /// <summary>
    /// Data table column type
    /// </summary>
    public enum ColumnType
    {
        /// <summary>
        /// Nothing
        /// </summary>
        Null = 0,

        /// <summary>
        /// Boolean values
        /// </summary>
        Boolean,

        /// <summary>
        /// Byte values
        /// </summary>
        Byte,

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
        Tensor
    }

    /// <summary>
    /// A data table row
    /// </summary>
    public interface IRow
    {
        /// <summary>
        /// Gets the raw data from the row
        /// </summary>
        /// <returns></returns>
        IReadOnlyList<object> Data { get; }

        /// <summary>
        /// Gets the value of the specified column (converted to T)
        /// </summary>
        /// <typeparam name="T">The type of data to return (will be converted if neccessary)</typeparam>
        /// <param name="index">The column index to query</param>
        T GetField<T>(int index);

        /// <summary>
        /// Gets the specified strongly typed values
        /// </summary>
        /// <typeparam name="T">The type of data to return (will be converted if neccessary)</typeparam>
        /// <param name="indices">The column indices to return</param>
        IReadOnlyList<T> GetFields<T>(IReadOnlyList<int> indices);

        /// <summary>
        /// Returns the column information
        /// </summary>
        IHaveColumns Table { get; }
    }

    /// <summary>
    /// A classifier that uses a data table row
    /// </summary>
    public interface IRowClassifier
    {
        /// <summary>
        /// Classifies the input data and returns the classifications with their weights
        /// </summary>
        /// <param name="row">The row to classify</param>
        IReadOnlyList<(string Classification, float Weight)> Classify(IRow row);
    }

    /// <summary>
    /// A column within a data table
    /// </summary>
    public interface IColumn
    {
        /// <summary>
        /// The name of the column
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The data type
        /// </summary>
        ColumnType Type { get; }

        /// <summary>
        /// The number of distinct values
        /// </summary>
        int NumDistinct { get; }

        /// <summary>
        /// True if the value is continuous (not categorical)
        /// </summary>
        bool IsContinuous { get; set; }

        /// <summary>
        /// True if the column is a classification target (or label)
        /// </summary>
        bool IsTarget { get; }
    }

    /// <summary>
    /// Column provider
    /// </summary>
    public interface IHaveColumns
    {
        /// <summary>
        /// The list of columns
        /// </summary>
        IReadOnlyList<IColumn> Columns { get; }

        /// <summary>
        /// The number of columns
        /// </summary>
        int ColumnCount { get; }
    }

    /// <summary>
    /// Converts data table rows to vectors
    /// </summary>
    public interface IDataTableVectoriser
    {
        /// <summary>
        /// Vectorises the input columns of the specified row
        /// </summary>
        /// <param name="row">The row to vectorise</param>
        FloatVector GetInput(IRow row);

        /// <summary>
        /// Vectorises the output column of the specified row
        /// </summary>
        /// <param name="row">The row to vectorise</param>
        FloatVector GetOutput(IRow row);

        /// <summary>
        /// The size of the input vector
        /// </summary>
        int InputSize { get; }

        /// <summary>
        /// The size of the output vector
        /// </summary>
        int OutputSize { get; }

        /// <summary>
        /// The list of column names
        /// </summary>
        IReadOnlyList<string> ColumnNames { get; }

        /// <summary>
        /// Returns the classification label
        /// </summary>
        /// <param name="columnIndex">The data table column index</param>
        /// <param name="vectorIndex">The one hot vector index</param>
        string GetOutputLabel(int columnIndex, int vectorIndex);
    }

    /// <summary>
    /// Tabular data table
    /// </summary>
    public interface IDataTable : IHaveColumns
    {
        /// <summary>
        /// The number of rows
        /// </summary>
        int RowCount { get; }

        /// <summary>
        /// The column of the classification target (defaults to the last column if none set)
        /// </summary>
        int TargetColumnIndex { get; set; }

        /// <summary>
        /// Applies each row of the table to the specified processor
        /// </summary>
        /// <param name="rowProcessor">Will be called with each row</param>
        void Process(IRowProcessor rowProcessor);

        /// <summary>
        /// Data table statistics
        /// </summary>
        IDataTableAnalysis GetAnalysis();

        /// <summary>
        /// Invokes the callback on each row in the table
        /// </summary>
        /// <param name="callback">Callback that is invoked for each row</param>
        void ForEach(Action<IRow> callback);

        /// <summary>
        /// Invokes the callback on each row in the table
        /// </summary>
        /// <param name="callback">Callback that is invoked for each row</param>
        void ForEach(Action<IRow, int> callback);

        /// <summary>
        /// Invokes the callback on each row in the table
        /// </summary>
        /// <param name="callback">Callback that is invoked for each row and returns true to continue processing</param>
        void ForEach(Func<IRow, bool> callback);

        /// <summary>
        /// Gets a list of rows
        /// </summary>
        /// <param name="offset">The first row index</param>
        /// <param name="count">The number of rows to query</param>
        IReadOnlyList<IRow> GetSlice(int offset, int count);

        /// <summary>
        /// Gets a list of rows
        /// </summary>
        /// <param name="rowIndex">A sequence of row indices</param>
        IReadOnlyList<IRow> GetRows(IEnumerable<int> rowIndex);

        /// <summary>
        /// Returns the row at the specified index
        /// </summary>
        /// <param name="rowIndex">The row index to retrieve</param>
        IRow GetRow(int rowIndex);

        /// <summary>
        /// Splits the table into two random tables
        /// </summary>
        /// <param name="randomSeed">Optional random seed</param>
        /// <param name="firstSize">The size of the first table (expressed as a value between 0 and 1)</param>
        /// <param name="shuffle">True to shuffle the table before splitting</param>
        /// <param name="output1">Optional stream to write the first output table to</param>
        /// <param name="output2">Optional stream to write the second output table to</param>
        (IDataTable Training, IDataTable Test) Split(int? randomSeed = null, double firstSize = 0.8, bool shuffle = true, Stream output1 = null, Stream output2 = null);

        /// <summary>
        /// Creates a normalised version of the current table
        /// </summary>
        /// <param name="normalisationType">The type of normalisation to apply</param>
        /// <param name="output">Optional stream to write the normalised table to</param>
        IDataTable Normalise(NormalisationType normalisationType, Stream output = null);

        /// <summary>
        /// Creates a normalised version of the current table
        /// </summary>
        /// <param name="normalisationModel">The normalisation model to apply</param>
        /// <param name="output">Optional stream to write the normalised table to</param>
        IDataTable Normalise(DataTableNormalisation normalisationModel, Stream output = null);

        /// <summary>
        /// Builds a normalisation model from the table that can be used to normalise data to the same scale
        /// </summary>
        /// <param name="normalisationType">The type of normalisation</param>
        DataTableNormalisation GetNormalisationModel(NormalisationType normalisationType);

        /// <summary>
        /// Converts the rows to vectors
        /// </summary>
        /// <param name="lap">Linear algebra provider</param>
        /// <param name="columns">Optional list of columns to extract (or null for all rows)</param>
        IReadOnlyList<IVector> GetNumericRows(ILinearAlgebraProvider lap, IEnumerable<int> columns = null);

        /// <summary>
        /// Converts the columns to vectors
        /// </summary>
        /// <param name="lap">Linear algebra provider</param>
        /// <param name="columns">Optional list of columns to extract (or null for all columns)</param>
        IReadOnlyList<IVector> GetNumericColumns(ILinearAlgebraProvider lap, IEnumerable<int> columns = null);

        /// <summary>
        /// Gets a column from the table
        /// </summary>
        /// <typeparam name="T">The type to convert the data to</typeparam>
        /// <param name="columnIndex">The column to retrieve</param>
        IReadOnlyList<T> GetColumn<T>(int columnIndex);

        /// <summary>
        /// Creates a new data table with bagged rows from the current table
        /// </summary>
        /// <param name="count">The count of rows to bag</param>
        /// <param name="output">Optional stream to write the new table to</param>
        /// <param name="randomSeed">Optional random seed</param>
        /// <returns></returns>
        IDataTable Bag(int? count = null, Stream output = null, int? randomSeed = null);

        /// <summary>
        /// Converts rows to lists of floats
        /// </summary>
        /// <param name="columns">Optional list of columns to convert (or null for all columns)</param>
        /// <returns></returns>
        IReadOnlyList<float[]> GetNumericRows(IEnumerable<int> columns = null);

        /// <summary>
        /// Gets each specified column (or all columns) as an array of floats
        /// </summary>
        /// <param name="columns">The columns to return (or null for all)</param>
        IReadOnlyList<float[]> GetNumericColumns(IEnumerable<int> columns = null);

        /// <summary>
        /// Classifies each row
        /// </summary>
        /// <param name="classifier">The classifier to use</param>
        IReadOnlyList<(IRow Row, string Classification)> Classify(IRowClassifier classifier);

        /// <summary>
        /// Creates a new data table with the specified columns
        /// </summary>
        /// <param name="columns">The columns to include in the new table</param>
        /// <param name="output">Optional stream to write the new table to</param>
        IDataTable SelectColumns(IEnumerable<int> columns, Stream output = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mutator"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        IDataTable Project(Func<IRow, IReadOnlyList<object>> mutator, Stream output = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="k"></param>
        /// <param name="randomSeed"></param>
        /// <param name="shuffle"></param>
        /// <returns></returns>
        IEnumerable<(IDataTable Training, IDataTable Validation)> Fold(int k, int? randomSeed = null, bool shuffle = true);

        /// <summary>
        /// Writes the index data to the specified stream
        /// </summary>
        /// <param name="stream">The stream to hold the index data</param>
        void WriteIndexTo(Stream stream);

        /// <summary>
        /// Writes the data table to the stream
        /// </summary>
        /// <param name="stream">The stream to write to</param>
        void WriteTo(Stream stream);

        /// <summary>
        /// For each classification label - duplicate each data table except for the classification column which is converted to a boolean (true for each matching example)
        /// </summary>
        /// <returns></returns>
        IReadOnlyList<(IDataTable Table, string Classification)> ConvertToBinaryClassification();

        /// <summary>
        /// Mutates each row of the table
        /// </summary>
        /// <typeparam name="T">The type returned by the mutator</typeparam>
        /// <param name="mutator">The function called for each row in the table</param>
        IReadOnlyList<T> Map<T>(Func<IRow, T> mutator);

        /// <summary>
        /// Returns an interface that can convert rows in the current table to vectors
        /// </summary>
        /// <param name="useTargetColumnIndex">True to separate the target column index into a separate output vector</param>
        IDataTableVectoriser GetVectoriser(bool useTargetColumnIndex = true);

        /// <summary>
        /// Returns a copy of the current table
        /// </summary>
        /// <param name="rowIndex">The list of rows to copy</param>
        /// <param name="output">Optional stream to write the new table to</param>
        /// <returns></returns>
        IDataTable CopyWithRows(IEnumerable<int> rowIndex, Stream output = null);

        /// <summary>
        /// Converts the current data table to a numeric data table (the classification column is a string)
        /// </summary>
        /// <param name="vectoriser">Optional vectoriser</param>
        /// <param name="useTargetColumnIndex">True to separate the target column index into a separate output vector</param>
        /// <param name="output">Optional stream to write the new table to</param>
        /// <returns></returns>
        IDataTable ConvertToNumeric(IDataTableVectoriser vectoriser = null, bool useTargetColumnIndex = true, Stream output = null);

        /// <summary>
        /// Returns table meta-data and the top 20 rows of the table as XML
        /// </summary>
        string XmlPreview { get; }

        /// <summary>
        /// Returns true if the data table contains any non-numeric columns
        /// </summary>
        bool HasCategoricalData { get; }
    }

    /// <summary>
    /// Row processor
    /// </summary>
    public interface IRowProcessor
    {
        /// <summary>
        /// Will be called for each row
        /// </summary>
        /// <param name="row">The current row</param>
        /// <returns>False to stop</returns>
        bool Process(IRow row);
    }

    /// <summary>
    /// Column statistics within a data table
    /// </summary>
    public interface IColumnInfo
    {
        /// <summary>
        /// The index of the column in the data table
        /// </summary>
        int ColumnIndex { get; }

        /// <summary>
        /// The distinct values within the column
        /// </summary>
        IEnumerable<object> DistinctValues { get; }

        /// <summary>
        /// The number of distinct values (or null if there are too many)
        /// </summary>
        int? NumDistinct { get; }
    }

    /// <summary>
    /// Column statistics for a string based column
    /// </summary>
    public interface IStringColumnInfo : IColumnInfo
    {
        /// <summary>
        /// The minimum string length
        /// </summary>
        int MinLength { get; }

        /// <summary>
        /// The maximum string length
        /// </summary>
        int MaxLength { get; }

        /// <summary>
        /// The most common string
        /// </summary>
        string MostCommonString { get; }
    }

    /// <summary>
    /// Column statistics for a numeric column
    /// </summary>
    public interface INumericColumnInfo : IColumnInfo
    {
        /// <summary>
        /// The minimum value
        /// </summary>
        double Min { get; }

        /// <summary>
        /// The maximum value
        /// </summary>
        double Max { get; }

        /// <summary>
        /// The mean (or average)
        /// </summary>
        double Mean { get; }

        /// <summary>
        /// The standard deviation
        /// </summary>
        double? StdDev { get; }

        /// <summary>
        /// The median value
        /// </summary>
        double? Median { get; }

        /// <summary>
        /// The mode
        /// </summary>
        double? Mode { get; }

        /// <summary>
        /// The L1 Norm
        /// </summary>
        double L1Norm { get; }

        /// <summary>
        /// The L2 Norm
        /// </summary>
        double L2Norm { get; }
    }

    internal interface IFrequencyColumnInfo : IColumnInfo
    {
        IEnumerable<KeyValuePair<string, ulong>> Frequency { get; }
    }

    /// <summary>
    /// Data table statistics
    /// </summary>
    public interface IDataTableAnalysis
    {
        /// <summary>
        /// List of column statistics
        /// </summary>
        IEnumerable<IColumnInfo> ColumnInfo { get; }

        /// <summary>
        /// Gets the statistics for a particular column
        /// </summary>
        /// <param name="columnIndex">The column index to query</param>
        IColumnInfo this[int columnIndex] { get; }

        /// <summary>
        /// Returns a summary of the table analysis
        /// </summary>
        string AsXml { get; }
    }

    /// <summary>
    /// Used to programatically construct data tables
    /// </summary>
    public interface IDataTableBuilder
    {
        /// <summary>
        /// The list of columns
        /// </summary>
        IReadOnlyList<IColumn> Columns { get; }

        /// <summary>
        /// The number of rows
        /// </summary>
        int RowCount { get; }

        /// <summary>
        /// The number of columns
        /// </summary>
        int ColumnCount { get; }

        /// <summary>
        /// Adds a new column to the data table
        /// </summary>
        /// <param name="column">The data type of the new column</param>
        /// <param name="name">The name of the new column</param>
        /// <param name="isTarget">True if the column is a classification target</param>
        IColumn AddColumn(ColumnType column, string name = "", bool isTarget = false);

        /// <summary>
        /// Adds a new row to the table
        /// </summary>
        /// <param name="data">The data in the new row</param>
        /// <returns></returns>
        IRow Add(params object[] data);

        /// <summary>
        /// Creates a new data table
        /// </summary>
        /// <param name="output">Optional stream to write the data table to</param>
        /// <returns></returns>
        IDataTable Build(Stream output = null);

        /// <summary>
        /// Clears the current list of rows
        /// </summary>
        void ClearRows();
    }
}
