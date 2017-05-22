using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire
{
    public interface IGpuLinearAlgebraProvider
    {
        void BindThread();
    }

    public interface ICountReferences
    {
        int AddRef();
        int Release();
    }

    /// <summary>
    /// Provides linear algebra functionality
    /// </summary>
    public interface ILinearAlgebraProvider : IDisposable
    {
        /// <summary>
        /// Creates a vector based on an enumerable of floats
        /// </summary>
        /// <param name="data">The initial values in the vector</param>
        IVector CreateVector(IEnumerable<float> data);

        /// <summary>
        /// Creates a vector
        /// </summary>
        /// <param name="length">Size of the vector</param>
        /// <param name="init">Callback to initialise each element of the vector</param>
        IVector CreateVector(int length, Func<int, float> init);

        /// <summary>
        /// Creates a matrix
        /// </summary>
        /// <param name="rows">The number of rows</param>
        /// <param name="columns">The number of columns</param>
        /// <param name="init">Callback to initialise each element of the matrix</param>
        IMatrix CreateMatrix(int rows, int columns, Func<int, int, float> init);

        /// <summary>
        /// Creates a matrix from a list of vectors
        /// </summary>
        /// <param name="rows">The list of rows in the new matrix</param>
        IMatrix CreateMatrix(IReadOnlyList<IVector> rows);

        /// <summary>
        /// Creates a 3D tensor
        /// </summary>
        /// <param name="data">The list of matrices that form the tensor</param>
        /// <returns></returns>
        I3DTensor CreateTensor(IReadOnlyList<IMatrix> data);

        /// <summary>
        /// Creates a save point in the allocation history
        /// </summary>
        void PushLayer();

        /// <summary>
        /// Releases all allocated memory since the last save point
        /// </summary>
        void PopLayer();

        /// <summary>
        /// Underlying setting for stochastic vs deterministic behaviour across BrightWire
        /// </summary>
        bool IsStochastic { get; }

        /// <summary>
        /// True if the provider uses the GPU
        /// </summary>
        bool IsGpu { get; }
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
    public interface IVector : IDisposable, ICountReferences
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
        FloatVector Data { get; set; }

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

        /// <summary>
        /// Rotates values in the vector (both within and along blocks)
        /// </summary>
        /// <param name="blockCount">The size of each sub vector</param>
        /// <returns></returns>
        IVector Rotate(int blockCount);

        /// <summary>
        /// In place reversal of the vector's values
        /// </summary>
        /// <returns></returns>
        IVector Reverse();
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
    public interface IMatrix : IDisposable, ICountReferences
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
        //IMatrix Rotate180();

        //IMatrix AddPadding(int padding);

        //IMatrix RemovePadding(int padding);

        //IMatrix Im2Col(int filterWidth, int filterHeight, int stride);
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
        (I3DTensor Result, IReadOnlyList<(int[] X, int[] Y)> Index) MaxPool(int filterWidth, int filterHeight, int stride);

        I3DTensor ReverseMaxPool(int rows, int columns, IReadOnlyList<(int[] X, int[] Y)> indexList);

        /// <summary>
        /// Returns the list of matrices
        /// </summary>
        IReadOnlyList<IMatrix> DepthSlices { get; }

        (IMatrix WeightUpdate, IVector BiasUpdate) CalculateWeightUpdate(IMatrix im2Col);

        I3DTensor CalculatePreviousError(IMatrix filterMatrix, int inputHeight, int inputWidth, int inputDepth, int padding, int filterHeight, int filterWidth, int stride);
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
        IReadOnlyList<IIndexableMatrix> Matrix { get; }
    }
}
