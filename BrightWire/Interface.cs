using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
        CategoryList,

        /// <summary>
        /// Weighted list of indices
        /// </summary>
        WeightedCategoryList,

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
        /// True if the current row is a sub item of a deep data table row
        /// </summary>
        bool IsSubItem { get; }

        /// <summary>
        /// The number of sub-items in the current row
        /// </summary>
        int Depth { get; }

        /// <summary>
        /// Gets the raw data from the row
        /// </summary>
        /// <param name="depth">The depth of the data to query (optional)</param>
        /// <returns></returns>
        IReadOnlyList<object> GetData(int depth = 0);

        /// <summary>
        /// Gets the value of the specified column (converted to T)
        /// </summary>
        /// <typeparam name="T">The type of data to return (will be converted if neccessary)</typeparam>
        /// <param name="index">The column index to query</param>
        /// <param name="depth">The depth of the data to query (optional)</param>
        T GetField<T>(int index, int depth = 0);

        /// <summary>
        /// Gets the specified strongly typed values
        /// </summary>
        /// <typeparam name="T">The type of data to return (will be converted if neccessary)</typeparam>
        /// <param name="indices">The column indices to return</param>
        /// /// <param name="depth">The depth of the data to query (optional)</param>
        IReadOnlyList<T> GetFields<T>(IReadOnlyList<int> indices, int depth = 0);

        /// <summary>
        /// Return the list of sub rows
        /// </summary>
        IReadOnlyList<IRow> SubItem { get; }
    }

    /// <summary>
    /// A classifier that uses a data table row
    /// </summary>
    public interface IRowClassifier
    {
        /// <summary>
        /// Classifies the row
        /// </summary>
        /// <param name="row">The row to classify</param>
        /// <returns>A ranked list of classifications</returns>
        IEnumerable<string> Classify(IRow row);

        /// <summary>
        /// Classifies the input data and returns the classifications with their weights
        /// </summary>
        /// <param name="row">The row to classify</param>
        IReadOnlyList<(string Classification, float Weight)> GetWeightedClassifications(IRow row);
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
        float[] GetInput(IRow row);

        /// <summary>
        /// Vectorises the output column of the specified row
        /// </summary>
        /// <param name="row">The row to vectorise</param>
        float[] GetOutput(IRow row);

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
    /// Preconfigured data table structurs
    /// </summary>
    public enum DataTableTemplate
    {
        /// <summary>
        /// No default structure
        /// </summary>
        Standard = 0,

        /// <summary>
        /// Two column table: category list in first column, classification target in second
        /// </summary>
        CategoryList,

        /// <summary>
        /// Two column table: weighted category list in first column, classification target in second
        /// </summary>
        WeightedCategoryList,

        /// <summary>
        /// Two column table: vector in first column, classification target in second
        /// </summary>
        Vector,

        /// <summary>
        /// Two column table: matrix in first column, classification target in second
        /// </summary>
        Matrix,

        /// <summary>
        /// Two column table: tensor in first column, classification target in second
        /// </summary>
        Tensor,
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
        /// Data table template
        /// </summary>
        DataTableTemplate Template { get; }

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
        /// <param name="output">Optional stream to write the new table to</param>
        /// <returns></returns>
        IDataTable ConvertToNumeric(IDataTableVectoriser vectoriser = null, Stream output = null);

        /// <summary>
        /// Returns table meta-data and the top 20 rows of the table as XML
        /// </summary>
        string XmlPreview { get; }

        /// <summary>
        /// Returns true if the data table contains any non-numeric columns
        /// </summary>
        bool HasCategoricalData { get; }

        /// <summary>
        /// Returns the depth of each row
        /// </summary>
        /// <returns>An array of depths, each corresponding to the depth at that row</returns>
        uint[] GetRowDepths();
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
    /// Provides linear algebra functionality
    /// </summary>
    public interface ILinearAlgebraProvider : IDisposable
    {
        /// <summary>
        /// Creates a vector based on an array
        /// </summary>
        /// <param name="data">The initial values in the vector</param>
        IVector Create(float[] data);

        /// <summary>
        /// Creates a vector based on an enumerable of floats
        /// </summary>
        /// <param name="data">The initial values in the vector</param>
        IVector Create(IEnumerable<float> data);

        /// <summary>
        /// Creates a vector
        /// </summary>
        /// <param name="length">Size of the vector</param>
        /// <param name="value">Value to initialise each element</param>
        IVector Create(int length, float value);

        /// <summary>
        /// Creates a vector
        /// </summary>
        /// <param name="length">Size of the vector</param>
        /// <param name="init">Callback to initialise each element of the vector</param>
        IVector Create(int length, Func<int, float> init);

        /// <summary>
        /// Creates a vector
        /// </summary>
        /// <param name="vector">The vector to use as the initial values</param>
        IVector Create(IIndexableVector vector);

        /// <summary>
        /// Creates a matrix
        /// </summary>
        /// <param name="rows">The number of rows</param>
        /// <param name="columns">The number of columns</param>
        /// <param name="init">Callback to initialise each element of the matrix</param>
        IMatrix Create(int rows, int columns, Func<int, int, float> init);

        /// <summary>
        /// Creates a matrix
        /// </summary>
        /// <param name="rows">The number of rows</param>
        /// <param name="columns">The number of columns</param>
        /// <param name="value">Value to initialise element</param>
        IMatrix Create(int rows, int columns, float value);

        /// <summary>
        /// Creates a matrix from a list of vectors
        /// </summary>
        /// <param name="vectorData">The list of rows in the new matrix</param>
        IMatrix Create(IReadOnlyList<IVector> vectorData);

        /// <summary>
        /// Creates a matrix from a list of vectors
        /// </summary>
        /// <param name="vectorData">The list of rows in the new matrix</param>
        IMatrix Create(IReadOnlyList<IIndexableVector> vectorData);

        /// <summary>
        /// Creates a matrix
        /// </summary>
        /// <param name="matrix">The matrix to use as the initial values</param>
        IMatrix Create(IIndexableMatrix matrix);

        /// <summary>
        /// Creates a matrix
        /// </summary>
        /// <param name="data">The serialised representation of the matrix</param>
        IMatrix CreateMatrix(FloatMatrix data);

        /// <summary>
        /// Creates a vector
        /// </summary>
        /// <param name="data">The serialised representation of the vector</param>
        IVector CreateVector(FloatArray data);

        /// <summary>
        /// Creates an indexable vector
        /// </summary>
        /// <param name="length">Size of the vector</param>
        IIndexableVector CreateIndexable(int length);

        /// <summary>
        /// Creates an indexable vector
        /// </summary>
        /// <param name="length">Size of the vector</param>
        /// <param name="init">Callback to initialise each element of the vector</param>
        IIndexableVector CreateIndexable(int length, Func<int, float> init);

        /// <summary>
        /// Creates an indexable matrix (initialised to zero)
        /// </summary>
        /// <param name="rows">The number of rows</param>
        /// <param name="columns">The number of columns</param>
        IIndexableMatrix CreateIndexable(int rows, int columns);

        /// <summary>
        /// Creates an indexable matrix
        /// </summary>
        /// <param name="rows">The number of rows</param>
        /// <param name="columns">The number of columns</param>
        /// <param name="init">Callback to initialise each element of the matrix</param>
        IIndexableMatrix CreateIndexable(int rows, int columns, Func<int, int, float> init);

        /// <summary>
        /// Creates an indexable matrix
        /// </summary>
        /// <param name="rows">The number of rows</param>
        /// <param name="columns">The number of columns</param>
        /// <param name="value">Value to initialise each element</param>
        IIndexableMatrix CreateIndexable(int rows, int columns, float value);

        /// <summary>
        /// Creates an identity matrix
        /// </summary>
        /// <param name="size">The size of the identity matrix</param>
        IMatrix CreateIdentity(int size);

        /// <summary>
        /// Creates a diagonal matrix
        /// </summary>
        /// <param name="values">The diagonal values</param>
        IMatrix CreateDiagonal(IReadOnlyList<float> values);

        /// <summary>
        /// Creates a 3D tensor
        /// </summary>
        /// <param name="data">The list of matrices that form the tensor</param>
        /// <returns></returns>
        I3DTensor CreateTensor(IReadOnlyList<IMatrix> data);

        /// <summary>
        /// Creates a 3D tensor
        /// </summary>
        /// <param name="tensor">An indexable 3D tensor to use as a source</param>
        I3DTensor CreateTensor(IIndexable3DTensor tensor);

        /// <summary>
        /// Creates a 3D tensor
        /// </summary>
        /// <param name="tensor">The serialised representation of the 3D tensor</param>
        /// <returns></returns>
        I3DTensor CreateTensor(FloatTensor tensor);

        /// <summary>
        /// Creates a save point in the allocation history
        /// </summary>
        void PushLayer();

        /// <summary>
        /// Releases all allocated memory since the last save point
        /// </summary>
        void PopLayer();

        bool IsStochastic { get; }
    }

    /// <summary>
    /// Distance metrics
    /// </summary>
    public enum DistanceMetric
    {
        /// <summary>
        /// Euclidean Distance
        /// </summary>
        Euclidean,

        /// <summary>
        /// Cosine Distance Metric
        /// </summary>
        Cosine,

        /// <summary>
        /// Manhattan Distance
        /// </summary>
        Manhattan,

        /// <summary>
        /// Means Square Error
        /// </summary>
        MeanSquared,

        /// <summary>
        /// Square Euclidean
        /// </summary>
        SquaredEuclidean
    }

    /// <summary>
    /// A vector
    /// </summary>
    public interface IVector : IDisposable
    {
        /// <summary>
        /// Checks if the vector has not been disposed
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Converts the vector to a column matrix
        /// </summary>
        /// <param name="numCols">The number of columns in the matrix</param>
        IMatrix ToColumnMatrix(int numCols = 1);

        /// <summary>
        /// Converts the vector to a row matrix
        /// </summary>
        /// <param name="numRows">The number of rows in the matrix</param>
        IMatrix ToRowMatrix(int numRows = 1);

        /// <summary>
        /// The number of elements in the vector
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Converts the vector into protobuf format
        /// </summary>
        FloatArray Data { get; set; }

        /// <summary>
        /// Adds a vector (without in place modification)
        /// </summary>
        /// <param name="vector">The vector to add</param>
        IVector Add(IVector vector);

        /// <summary>
        /// Subtracts a vector (without in place modification)
        /// </summary>
        /// <param name="vector">The vector to subtract</param>
        IVector Subtract(IVector vector);

        /// <summary>
        /// Calculates the absolute values (L1) norm: https://en.wikipedia.org/wiki/Norm_(mathematics)
        /// </summary>
        float L1Norm();

        /// <summary>
        /// Calculates the euclidean (L2) norm: https://en.wikipedia.org/wiki/Norm_(mathematics)
        /// </summary>
        float L2Norm();

        /// <summary>
        /// Returns the index of the vector with the greatest value
        /// </summary>
        int MaximumIndex();

        /// <summary>
        /// Returns the index of the vector with the smallest value
        /// </summary>
        int MinimumIndex();

        /// <summary>
        /// Multiples (in place) by a scalar
        /// </summary>
        /// <param name="scalar">The value to multiple each element</param>
        void Multiply(float scalar);

        /// <summary>
        /// Adds (in place) a scalar
        /// </summary>
        /// <param name="scalar">The value to add to each element</param>
        void Add(float scalar);

        /// <summary>
        /// Adds a vector in place
        /// </summary>
        /// <param name="vector">The target vector to add to the current vector</param>
        /// <param name="coefficient1">A value to multiply each element of the current vector</param>
        /// <param name="coefficient2">A value to multiply each element of the target vector</param>
        void AddInPlace(IVector vector, float coefficient1 = 1.0f, float coefficient2 = 1.0f);

        /// <summary>
        /// Subtracts a vector in place
        /// </summary>
        /// <param name="vector">The target vector to subtract from the current vector</param>
        /// <param name="coefficient1">A value to multiply each element of the current vector</param>
        /// <param name="coefficient2">A value to multiply each element of the target vector</param>
        void SubtractInPlace(IVector vector, float coefficient1 = 1.0f, float coefficient2 = 1.0f);

        /// <summary>
        /// Converts the vector to an indexable vector
        /// </summary>
        IIndexableVector AsIndexable();

        /// <summary>
        /// Pointwise multiplication (without in place modification) with a vector
        /// </summary>
        IVector PointwiseMultiply(IVector vector);

        /// <summary>
        /// The dot product of two vectors
        /// </summary>
        /// <param name="vector">The target vector</param>
        float DotProduct(IVector vector);

        /// <summary>
        /// Returns a new vector from a subset of the vector indices
        /// </summary>
        /// <param name="indices">A list of indexes to use as the source of the new vector</param>
        IVector GetNewVectorFromIndexes(IReadOnlyList<int> indices);

        /// <summary>
        /// Creates a new copy of the vector
        /// </summary>
        IVector Clone();

        /// <summary>
        /// Creates a new vector in which each element is the square root of the current vector
        /// </summary>
        IVector Sqrt();

        /// <summary>
        /// Creates a new vector in which each element is the absolute value of the current vector
        /// </summary>
        IVector Abs();

        /// <summary>
        /// Copies values from the target vector into the current vector
        /// </summary>
        /// <param name="vector"></param>
        void CopyFrom(IVector vector);

        /// <summary>
        /// Calculates the euclidean distance between the current and the target vector
        /// </summary>
        /// <param name="vector">The target vector</param>
        float EuclideanDistance(IVector vector);

        /// <summary>
        /// Calculates the cosine distance between the current and the target vector
        /// </summary>
        /// <param name="vector">The target vector></param>
        float CosineDistance(IVector vector);

        /// <summary>
        /// Calculates the manhattan distance between the current and the target vector
        /// </summary>
        /// <param name="vector">The target vector</param>
        float ManhattanDistance(IVector vector);

        /// <summary>
        /// Calculates the mean squared distance between the current and the target vector
        /// </summary>
        /// <param name="vector">The target vector</param>
        float MeanSquaredDistance(IVector vector);

        /// <summary>
        /// Calculates the squared euclidean distance between the current and the target vector
        /// </summary>
        /// <param name="vector">The target vector</param>
        float SquaredEuclidean(IVector vector);

        /// <summary>
        /// Finds the minimum and maximum values in the current vector
        /// </summary>
        (float Min, float Max) GetMinMax();

        /// <summary>
        /// Calculates the average value from the elements of the current vector
        /// </summary>
        float Average();

        /// <summary>
        /// Calculates the standard deviation from the elements of the current vector
        /// </summary>
        /// <param name="mean">(optional) pre calculated mean</param>
        float StdDev(float? mean);

        /// <summary>
        /// Normalises (in place) the values of the current vector
        /// </summary>
        /// <param name="type">The type of normalisation</param>
        void Normalise(NormalisationType type);

        /// <summary>
        /// Returns the softmax function (without in place modification) applied to the current vector
        /// https://en.wikipedia.org/wiki/Softmax_function
        /// </summary>
        IVector Softmax();

        /// <summary>
        /// Returns the jacobian matrix of the softmax derivative
        /// </summary>
        /// <returns></returns>
        IMatrix SoftmaxDerivative();

        /// <summary>
        /// Returns a vector of distances between the current and target vectors
        /// </summary>
        /// <param name="data">The list of target vectors</param>
        /// <param name="distance">The distance metric</param>
        /// <returns>A vector in which each element n is the distance between the current and the nth target vector</returns>
        IVector FindDistances(IReadOnlyList<IVector> data, DistanceMetric distance);

        /// <summary>
        /// Returns the distance between the current and the target vector
        /// </summary>
        /// <param name="other">The target vector</param>
        /// <param name="distance">The distance metric</param>
        float FindDistance(IVector other, DistanceMetric distance);

        /// <summary>
        /// Returns a vector of the cosine distance between the current and target vectors
        /// </summary>
        /// <param name="data">The list of target vectors</param>
        /// <param name="dataNorm">A buffer to hold the norms of the target vectors</param>
        /// <returns>A vector in which each element n is the cosine distance between the current and the nth target vector</returns>
        IVector CosineDistance(IReadOnlyList<IVector> data, ref float[] dataNorm);

        /// <summary>
        /// Returns a vector (without in place modification) in which each element is the natural log of each element in the current vector
        /// </summary>
        IVector Log();

        /// <summary>
        /// Returns the sigmoid function (without in place modification) applied to the current vector
        /// </summary>
        IVector Sigmoid();

        /// <summary>
        /// Fast conversion to matrix (internal buffer is used directly)
        /// </summary>
        /// <param name="rows">The number of rows in the matrix</param>
        /// <param name="columns">The number of columns in the matrix</param>
        IMatrix ConvertInPlaceToMatrix(int rows, int columns);

        /// <summary>
        /// Splits the vector into a list of vectors
        /// </summary>
        /// <param name="blockCount">The size of each sub vector</param>
        IReadOnlyList<IVector> Split(int blockCount);
    }

    /// <summary>
    /// Returns an indexable vector (in which elements can be directly indexed)
    /// </summary>
    public interface IIndexableVector : IVector
    {
        /// <summary>
        /// Returns an element at the specified index
        /// </summary>
        /// <param name="index">The index to retrieve</param>
        float this[int index] { get; set; }

        /// <summary>
        /// Gets the values as an enumerable
        /// </summary>
        IEnumerable<float> Values { get; }

        /// <summary>
        /// Converts the vector to an array
        /// </summary>
        float[] ToArray();

        /// <summary>
        /// Creates a new vector (without in place modification) in which new values are appended onto the end of the current vector
        /// </summary>
        /// <param name="data">The values to append</param>
        IIndexableVector Append(IReadOnlyList<float> data);
    }

    /// <summary>
    /// A matrix
    /// </summary>
    public interface IMatrix : IDisposable
    {
        /// <summary>
        /// Checks if the matrix has not been disposed
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Multiplies the current vector (without in place modification) with the target matrix
        /// </summary>
        /// <param name="matrix">The target matrix</param>
        IMatrix Multiply(IMatrix matrix);

        /// <summary>
        /// The number of columns
        /// </summary>
        int ColumnCount { get; }

        /// <summary>
        /// The number of rows
        /// </summary>
        int RowCount { get; }

        /// <summary>
        /// Returns a column as a vector
        /// </summary>
        /// <param name="index">The column index</param>
        IVector Column(int index);

        /// <summary>
        /// Returns the matrix diagonal as a vector
        /// </summary>
        IVector Diagonal();

        /// <summary>
        /// Returns a row as a vector
        /// </summary>
        /// <param name="index">The row index</param>
        IVector Row(int index);

        /// <summary>
        /// Returns the current matrix (without in place modification) added to the target matrix
        /// </summary>
        /// <param name="matrix">The target matrix</param>
        IMatrix Add(IMatrix matrix);

        /// <summary>
        /// Returns the current matrix  (without in place modification) minus the target matrix
        /// </summary>
        /// <param name="matrix">The target matrix</param>
        IMatrix Subtract(IMatrix matrix);

        /// <summary>
        /// Returns the pointwise product of the current matrix (without in place modification) with the target matrix
        /// </summary>
        /// <param name="matrix">The target matrix</param>
        IMatrix PointwiseMultiply(IMatrix matrix);

        /// <summary>
        /// Returns the current matrix (without in place modification) and multipled with the transposed target matrix
        /// </summary>
        /// <param name="matrix">The target matrix</param>
        IMatrix TransposeAndMultiply(IMatrix matrix);

        /// <summary>
        /// Returns the transpose of the current matrix (without in place modification) multipled with the target matrix
        /// </summary>
        /// <param name="matrix"></param>
        IMatrix TransposeThisAndMultiply(IMatrix matrix);

        /// <summary>
        /// Returns a vector that contains the sum of the elements in each row of the current matrix
        /// </summary>
        /// <param name="coefficient">Coefficient to multiply each final summation</param>
        IVector RowSums(float coefficient = 1f);

        /// <summary>
        /// Returns a vector that contains the sum of the elements in each column of the current matrix
        /// </summary>
        /// <param name="coefficient">Coefficient to multiply each final summation</param>
        IVector ColumnSums(float coefficient = 1f);

        /// <summary>
        /// Returns the transpose of the current matrix
        /// </summary>
        IMatrix Transpose();

        /// <summary>
        /// Multiplies (in place) each element of the matrix by a scalar
        /// </summary>
        /// <param name="scalar">The scalar to multiply each element</param>
        void Multiply(float scalar);

        /// <summary>
        /// Returns the product of the current matrix (without in place modification) with the target vector
        /// </summary>
        /// <param name="vector">The target vector</param>
        IMatrix Multiply(IVector vector);

        /// <summary>
        /// Adds the target matrix to the current matrix (in place)
        /// </summary>
        /// <param name="matrix">The target matrix</param>
        /// <param name="coefficient1">A coefficient to multiply each element of the current matrix</param>
        /// <param name="coefficient2">A coefficient to multipy each element of the target matrix</param>
        void AddInPlace(IMatrix matrix, float coefficient1 = 1.0f, float coefficient2 = 1.0f);

        /// <summary>
        /// Subtracts the target matrix from the current matrix (in place)
        /// </summary>
        /// <param name="matrix">The target matrix</param>
        /// <param name="coefficient1">A coefficient to multiply each element of the current matrix</param>
        /// <param name="coefficient2">A coefficient to multipy each element of the target matrix</param>
        void SubtractInPlace(IMatrix matrix, float coefficient1 = 1.0f, float coefficient2 = 1.0f);

        /// <summary>
        /// Returns a new matrix with the sigmoid function applied to each element
        /// </summary>
        IMatrix SigmoidActivation();

        /// <summary>
        /// Returns a new matrix with the sigmoid derivative of each element
        /// </summary>
        IMatrix SigmoidDerivative();

        /// <summary>
        /// Returns a new matrix with the tanh function applied to each element
        /// </summary>
        IMatrix TanhActivation();

        /// <summary>
        /// Returns a new matrix with the tanh derivative of each element
        /// </summary>
        IMatrix TanhDerivative();

        /// <summary>
        /// Returns a new matrix with the softmax function applied to each row of the matrix
        /// </summary>
        IMatrix SoftmaxActivation();

        /// <summary>
        /// Adds the target vector to each row of the current matrix (in place)
        /// </summary>
        /// <param name="vector">The target vector</param>
        void AddToEachRow(IVector vector);

        /// <summary>
        /// Adds the target vector to each column of the current matrix (in place)
        /// </summary>
        /// <param name="vector">The target vector</param>
        void AddToEachColumn(IVector vector);

        /// <summary>
        /// Converts the current matrix to protobuf format
        /// </summary>
        FloatMatrix Data { get; set; }

        /// <summary>
        /// Converts the matrix to an indexable matrix
        /// </summary>
        IIndexableMatrix AsIndexable();

        /// <summary>
        /// Returns a new matrix from a subset of the current matrix's rows
        /// </summary>
        /// <param name="rowIndexes">The list of row indices</param>
        IMatrix GetNewMatrixFromRows(IReadOnlyList<int> rowIndexes);

        /// <summary>
        /// Returns a new matrix from a subset of the current matrix's columns
        /// </summary>
        /// <param name="columnIndexes">The list of column indices</param>
        IMatrix GetNewMatrixFromColumns(IReadOnlyList<int> columnIndexes);

        /// <summary>
        /// Set to zero the specified rows in the current matrix
        /// </summary>
        /// <param name="indexes">The list of row indices</param>
        void ClearRows(IReadOnlyList<int> indexes);

        /// <summary>
        /// Set to zero the specified columns in the current matrix
        /// </summary>
        /// <param name="indexes">The list of column indices</param>
        void ClearColumns(IReadOnlyList<int> indexes);

        /// <summary>
        /// Returns the RELU function applied to each element of the current matrix
        /// </summary>
        IMatrix ReluActivation();

        /// <summary>
        /// Returns the RELU derivative of each element in the current matrix
        /// </summary>
        IMatrix ReluDerivative();

        /// <summary>
        /// Returns the leaky RELU function applied to each element in the current matrix
        /// </summary>
        IMatrix LeakyReluActivation();

        /// <summary>
        /// Returns the leaky RELU derivative of each element in the current matrix
        /// </summary>
        IMatrix LeakyReluDerivative();

        /// <summary>
        /// Creates a copy of the current matrix
        /// </summary>
        IMatrix Clone();

        /// <summary>
        /// Sets each element to zero
        /// </summary>
        void Clear();

        /// <summary>
        /// Returns the square root of each element in the current matrix
        /// </summary>
        /// <param name="valueAdjustment">Term to add to each element in the result matrix</param>
        IMatrix Sqrt(float valueAdjustment = 0);

        /// <summary>
        /// Returns each element raised to specified power
        /// </summary>
        /// <param name="power">The power to apply to each element</param>
        IMatrix Pow(float power);

        /// <summary>
        /// Returns the current matrix (not modified in place) divided by the target matrix
        /// </summary>
        /// <param name="matrix">The target matrix</param>
        IMatrix PointwiseDivide(IMatrix matrix);

        /// <summary>
        /// L1 Regularisation applied to each element of the current matrix (in place)
        /// </summary>
        /// <param name="coefficient">The L1 coefficient</param>
        void L1Regularisation(float coefficient);

        /// <summary>
        /// Returns a vector of the L2 norms of each column
        /// </summary>
        IVector ColumnL2Norm();

        /// <summary>
        /// Returns a vector of the L2 norms of each row
        /// </summary>
        IVector RowL2Norm();

        /// <summary>
        /// Pointwise divide each row by the target vector (in place)
        /// </summary>
        /// <param name="vector">The target vector</param>
        void PointwiseDivideRows(IVector vector);

        /// <summary>
        /// Pointwise divide each column by the target vector (in place)
        /// </summary>
        /// <param name="vector">The target vector</param>
        void PointwiseDivideColumns(IVector vector);

        /// <summary>
        /// Constrain each value within the specified min and max values (in place)
        /// </summary>
        /// <param name="min">The minimum allowed value</param>
        /// <param name="max">The maximum allowed value</param>
        void Constrain(float min, float max);

        /// <summary>
        /// Updates the values of the current matrix from the target vector
        /// </summary>
        /// <param name="index">The row to update</param>
        /// <param name="vector">The target vector</param>
        /// <param name="columnIndex">Update the column from this offset</param>
        void UpdateRow(int index, IIndexableVector vector, int columnIndex);

        /// <summary>
        /// Updates the values of the current matrix from the target vector
        /// </summary>
        /// <param name="index">The column to update</param>
        /// <param name="vector">The target vector</param>
        /// <param name="rowIndex">Update the row from this offset</param>
        void UpdateColumn(int index, IIndexableVector vector, int rowIndex);

        /// <summary>
        /// Returns a segment from a row of the current matrix
        /// </summary>
        /// <param name="rowIndex">The row index</param>
        /// <param name="columnIndex">The start index to return</param>
        /// <param name="length">The number of elements to return</param>
        IVector GetRowSegment(int rowIndex, int columnIndex, int length);

        /// <summary>
        /// Returns a segment from a column of the current matrix
        /// </summary>
        /// <param name="columnIndex">The column index</param>
        /// <param name="rowIndex">The start index to return</param>
        /// <param name="length">The number of elements to return</param>
        IVector GetColumnSegment(int columnIndex, int rowIndex, int length);

        /// <summary>
        /// Returns a new matrix with the columns of the target matrix appended to each column of the current matrix
        /// </summary>
        /// <param name="bottom">The target matrix</param>
        IMatrix ConcatColumns(IMatrix bottom);

        /// <summary>
        /// Returns a new matrix with the rows of the target matrix appended to each row of the current matrix
        /// </summary>
        /// <param name="right">The target matrix</param>
        IMatrix ConcatRows(IMatrix right);

        /// <summary>
        /// Splits the rows of the current matrix into two matrices
        /// </summary>
        /// <param name="columnIndex">The column index at which to split</param>
        (IMatrix Left, IMatrix Right) SplitAtColumn(int columnIndex);

        /// <summary>
        /// Splits the columns of the current matrix into two matrices
        /// </summary>
        /// <param name="rowIndex">The row index at which to split</param>
        (IMatrix Top, IMatrix Bottom) SplitAtRow(int rowIndex);

        /// <summary>
        /// Singular value decomposition
        /// </summary>
        (IMatrix U, IVector S, IMatrix VT) Svd();

        /// <summary>
        /// Fast conversion to vector (the internal buffer is not modified)
        /// </summary>
        IVector ConvertInPlaceToVector();

        /// <summary>
        /// Rotates the matrix 180 degrees
        /// </summary>
        /// <returns></returns>
        IMatrix Rotate180();
    }

    /// <summary>
    /// A matrix whose elements can be indexed directly
    /// </summary>
    public interface IIndexableMatrix : IMatrix
    {
        /// <summary>
        /// Returns an element from the current matrix
        /// </summary>
        /// <param name="row">Row index</param>
        /// <param name="column">Column index</param>
        float this[int row, int column] { get; set; }

        /// <summary>
        /// Returns the rows of the current matrix as vectors
        /// </summary>
        IEnumerable<IIndexableVector> Rows { get; }

        /// <summary>
        /// Returns the columns of the current matrix as vectors
        /// </summary>
        IEnumerable<IIndexableVector> Columns { get; }

        /// <summary>
        /// Returns each element in the current matrix as enumerable
        /// </summary>
        IEnumerable<float> Values { get; }

        /// <summary>
        /// Mutates each element of the current matrix
        /// </summary>
        /// <param name="mutator">The function to apply to each element</param>
        /// <returns></returns>
        IIndexableMatrix Map(Func<float, float> mutator);

        /// <summary>
        /// Mutates each element of the current matrix
        /// </summary>
        /// <param name="mutator">The function to apply to each element (rowIndex: int, columnIndex: int, value: float) => float</param>
        /// <returns></returns>
        IIndexableMatrix MapIndexed(Func<int, int, float, float> mutator);

        /// <summary>
        /// Returns the matrix as xml
        /// </summary>
        string AsXml { get; }
    }

    /// <summary>
    /// A 3D tensor is a list of matrices
    /// </summary>
    public interface I3DTensor : IDisposable
    {
        /// <summary>
        /// The number of rows in each matrix
        /// </summary>
        int RowCount { get; }

        /// <summary>
        /// The number of columns in each matrix
        /// </summary>
        int ColumnCount { get; }

        /// <summary>
        /// The number of matrices
        /// </summary>
        int Depth { get; }

        /// <summary>
        /// Converts the current tensor to protobuf format
        /// </summary>
        FloatTensor Data { get; set; }

        /// <summary>
        /// Returns a matrix at the specified depth
        /// </summary>
        /// <param name="depth">The depth to query</param>
        /// <returns></returns>
        IMatrix GetDepthSlice(int depth);

        /// <summary>
        /// Returns an indexable 3D tensor
        /// </summary>
        /// <returns></returns>
        IIndexable3DTensor AsIndexable();

        /// <summary>
        /// Adds padding to each matrix
        /// </summary>
        /// <param name="padding">The padding (both vertical and horizontal)</param>
        /// <returns>A new tensor</returns>
        I3DTensor AddPadding(int padding);

        /// <summary>
        /// Removes padding from each matrix
        /// </summary>
        /// <param name="padding">The padding to remove</param>
        /// <returns>A new tensor</returns>
        I3DTensor RemovePadding(int padding);

        /// <summary>
        /// Performs a convolution on each source matrix
        /// </summary>
        /// <param name="filterWidth">The filter width</param>
        /// <param name="filterHeight">The filter height</param>
        /// <param name="stride">The convolution stride</param>
        /// <returns></returns>
        IMatrix Im2Col(int filterWidth, int filterHeight, int stride);

        /// <summary>
        /// Converts the tensor to a vector (each matrix is concatenated into a single vector)
        /// </summary>
        /// <returns></returns>
        IVector ConvertToVector();

        /// <summary>
        /// Converts the tensor to a matrix (each matrix becomes a column in the new matrix)
        /// </summary>
        /// <returns></returns>
        IMatrix ConvertToMatrix();

        /// <summary>
        /// Performs a max pooling operation on the tensor
        /// </summary>
        /// <param name="filterWidth">The pooling filter width</param>
        /// <param name="filterHeight">The pooling filter height</param>
        /// <param name="stride">The pooling stride</param>
        /// <param name="indexPosList">A map of the indexes that were pooled (mapping from output to input positions)</param>
        /// <returns>A max pooled tensor</returns>
        I3DTensor MaxPool(int filterWidth, int filterHeight, int stride, List<Dictionary<Tuple<int, int>, Tuple<int, int>>> indexPosList);
    }

    /// <summary>
    /// A 3D tensor that can be directly indexed
    /// </summary>
    public interface IIndexable3DTensor : I3DTensor
    {
        /// <summary>
        /// Returns a value from the tensor
        /// </summary>
        /// <param name="row">The row to query</param>
        /// <param name="column">The column to query</param>
        /// <param name="depth">The depth to query</param>
        float this[int row, int column, int depth] { get; set; }

        /// <summary>
        /// Gets a list of the indexable matrices
        /// </summary>
        IReadOnlyList<IIndexableMatrix> Matrices { get; }
    }

    public interface IGraphData
    {
        IVector AsVector();
        IMatrix AsMatrix();
        I3DTensor AsTensor();
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
        /// Adds a row with depth
        /// </summary>
        IDeepDataTableRowBuilder AddDeepRow();

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

    /// <summary>
    /// A builder to create a deep data table row
    /// </summary>
    public interface IDeepDataTableRowBuilder
    {
        /// <summary>
        /// Adds a sub item to the deep data table row
        /// </summary>
        /// <param name="data">The data in the new row</param>
        /// <returns>The sub row item</returns>
        IRow AddSubItem(params object[] data);
    }

    public interface IDataSource
    {
        int InputSize { get; }
        int OutputSize { get; }
        int RowCount { get; }
        bool IsSequential { get; }
        uint[] RowDepth { get; }
        IReadOnlyList<(float[], float[])> Get(IReadOnlyList<int> rows);
        IReadOnlyList<IReadOnlyList<(float[], float[])>> GetSequential(IReadOnlyList<int> rows);
        string GetOutputLabel(int columnIndex, int vectorIndex);
    }

    public interface IMiniBatchProvider
    {
        IDataSource DataSource { get; }
        IEnumerable<(IMatrix, IMatrix)> GetMiniBatches(int batchSize, bool isStochastic);
    }

    public interface ILearningContext
    {
        ILinearAlgebraProvider LinearAlgebraProvider { get; }
        int CurrentEpoch { get; }
        float LearningRate { get; }
        int BatchSize { get; }
        int RowCount { get; }
        void Store(IMatrix error, Action<IMatrix> updater);
        bool CalculateTrainingError { get; }
        void ApplyUpdates();
        void StartEpoch();
        void EndEpoch();
        void SetRowCount(int rowCount);
    }

    public interface ILayer : IDisposable
    {
        (IMatrix Output, IBackpropagation BackProp) Forward(IMatrix input);
        IMatrix Execute(IMatrix input);
    }

    public interface IGradientDescentOptimisation : IDisposable
    {
        void Update(IMatrix source, IMatrix delta, ILearningContext context);
    }

    public interface IBackpropagation : IDisposable
    {
        IMatrix Backward(IMatrix errorSignal, ILearningContext context, bool calculateOutput);
    }

    public interface IComponent
    {
        IMatrix Execute(IMatrix input, IWireContext context);
    }

    public interface IWireContext
    {
        ILearningContext Context { get; }
        IMatrix Target { get; }
        void AddOutput(IMatrix output);
        void AddBackpropagation(IBackpropagation backProp);
        double? TrainingError { get; }

        void Backpropagate(IMatrix delta);
    }

    public interface IWire
    {
        IMatrix Send(IMatrix signal, IWireContext context);
    }

    public interface IErrorMetric
    {
        bool DisplayAsPercentage { get; }
        float Compute(IIndexableVector output, IIndexableVector targetOutput);
        IMatrix CalculateGradient(IMatrix output, IMatrix targetOutput);
    }

    public interface IGraphInput
    {
        int InputSize { get; }
        int OutputSize { get; }
        int RowCount { get; }
        void AddTarget(IWire target);
        double? Train(ILearningContext context);
        IReadOnlyList<(IIndexableVector Output, IIndexableVector TargetOutput)> Test(int batchSize = 128);
        IReadOnlyList<IIndexableVector> Execute(int batchSize = 128);
    }

    public interface IWeightInitialisation
    {
        IVector CreateBias(int size);
        IMatrix CreateWeight(int rows, int columns);
    }

    public interface IPropertySet
    {
        ILinearAlgebraProvider LinearAlgebraProvider { get; }
        IWeightInitialisation WeightInitialisation { get; set; }
        ICreateWeightInitialisation WeightInitialisationDescriptor { get; set; }
        IGradientDescentOptimisation GradientDescent { get; set; }
        ICreateTemplateBasedGradientDescent TemplateGradientDescentDescriptor { get; set; }
        ICreateGradientDescent GradientDescentDescriptor { get; set; }

        void Use(ICreateTemplateBasedGradientDescent descriptor);
        void Use(ICreateGradientDescent descriptor);
        void Use(ICreateWeightInitialisation descriptor);
        void Use(IGradientDescentOptimisation optimisation);
        void Use(IWeightInitialisation weightInit);

        T Get<T>(string name, T defaultValue = default(T));
        void Set<T>(string name, T obj);
        void Clear(string name);
    }

    public interface ICreateGradientDescent
    {
        IGradientDescentOptimisation Create(IPropertySet propertySet);
    }

    public interface ICreateTemplateBasedGradientDescent
    {
        IGradientDescentOptimisation Create(IGradientDescentOptimisation prev, IMatrix template, IPropertySet propertySet);
    }

    public interface ICreateWeightInitialisation
    {
        IWeightInitialisation Create(IPropertySet propertySet);
    }

    public interface IExecutionEngine
    {
        IReadOnlyList<IIndexableVector> Execute(int batchSize = 128);
    }

    public interface ITrainingEngine
    {
        double? Train();
        void Test(IErrorMetric errorMetric, int batchSize = 128);
        IReadOnlyList<(IIndexableVector Output, IIndexableVector TargetOutput)> Execute(int batchSize = 128);
    }
}
