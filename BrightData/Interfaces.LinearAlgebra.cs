using System;
using System.Collections.Generic;
using BrightData.LinearAlgebra;

namespace BrightData
{
    /// <summary>
    /// Linear algebra adaptor interfaces
    /// </summary>
    public interface ILinearAlgebraProvider : IDisposable, IHaveDataContext
    {
        /// <summary>
        /// Name of the linear algebra provider
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Creates a new vector
        /// </summary>
        /// <param name="length">Length of the vector</param>
        /// <param name="setToZero">True to initialise the data to zero (otherwise it might be anything)</param>
        IFloatVector CreateVector(uint length, bool setToZero = false);

        /// <summary>
        /// Creates a vector
        /// </summary>
        /// <param name="length">Size of the vector</param>
        /// <param name="init">Callback to initialise each element of the vector</param>
        IFloatVector CreateVector(uint length, Func<uint, float> init);

        /// <summary>
        /// Creates a matrix
        /// </summary>
        /// <param name="rows">The number of rows</param>
        /// <param name="columns">The number of columns</param>
        /// <param name="setToZero">True to initialise the data to zero (otherwise it might be anything)</param>
        IFloatMatrix CreateMatrix(uint rows, uint columns, bool setToZero = false);

        /// <summary>
        /// Creates a matrix
        /// </summary>
        /// <param name="rows">The number of rows</param>
        /// <param name="columns">The number of columns</param>
        /// <param name="init">Callback to initialise each element of the matrix</param>
        IFloatMatrix CreateMatrix(uint rows, uint columns, Func<uint, uint, float> init);

        /// <summary>
        /// Creates a matrix from a list of vectors. Each vector will become a row in the new matrix
        /// </summary>
        /// <param name="vectorRows">List of vectors for each row</param>
        /// <returns></returns>
        IFloatMatrix CreateMatrixFromRows(params IFloatVector[] vectorRows);

        /// <summary>
        /// Creates a matrix from a list of vectors. Each vector will become a column in the new matrix
        /// </summary>
        /// <param name="vectorColumns">List of vectors for each column</param>
        /// <returns></returns>
        IFloatMatrix CreateMatrixFromColumns(params IFloatVector[] vectorColumns);

        /// <summary>
        /// Creates a 3D tensor
        /// </summary>
        /// <param name="rows">Number of rows</param>
        /// <param name="columns">Number of columns</param>
        /// <param name="depth">Number of depth slices</param>
        /// <param name="setToZero">True to initialise the data to zero (otherwise it might be anything)</param>
        I3DFloatTensor Create3DTensor(uint rows, uint columns, uint depth, bool setToZero = false);

        /// <summary>
        /// Creates a 3D tensor from a list of matrices
        /// </summary>
        /// <param name="matrices">List of matrices</param>
        /// <returns></returns>
        I3DFloatTensor Create3DTensor(params IFloatMatrix[] matrices);

        /// <summary>
        /// Creates a 4D tensor
        /// </summary>
        /// <param name="rows">Number of rows</param>
        /// <param name="columns">Number of columns</param>
        /// <param name="depth">Number of matrices</param>
        /// <param name="count">Number of 3D tensors</param>
        /// <param name="setToZero">True to initialise the data to zero (otherwise it might be anything)</param>
        I4DFloatTensor Create4DTensor(uint rows, uint columns, uint depth, uint count, bool setToZero = false);

        /// <summary>
        /// Creates a 4D tensor from a list of 3D tensors
        /// </summary>
        /// <param name="tensors">List of 3D tensors</param>
        /// <returns></returns>
        I4DFloatTensor Create4DTensor(params I3DFloatTensor[] tensors);

        /// <summary>
        /// Creates a 4D tensor from a list of 3D tensors
        /// </summary>
        /// <param name="tensors">List of 3D tensors</param>
        /// <returns></returns>
        I4DFloatTensor Create4DTensor(params Tensor3D<float>[] tensors);

        /// <summary>
        /// Creates a save point in the allocation history
        /// </summary>
        void PushLayer();

        /// <summary>
        /// Releases all allocated memory since the last save point
        /// </summary>
        void PopLayer();

        /// <summary>
        /// True if the provider uses the GPU
        /// </summary>
        bool IsGpu { get; }

        /// <summary>
        /// Calculates the distance of each vector against the comparison vectors - the size of all vectors should be the same
        /// </summary>
        /// <param name="vectors"></param>
        /// <param name="comparison"></param>
        /// <param name="distanceMetric"></param>
        /// <returns></returns>
        IFloatMatrix CalculateDistances(IFloatVector[] vectors, IReadOnlyList<IFloatVector> comparison, DistanceMetric distanceMetric);

        /// <summary>
        /// Creates a vector from a tensor segment
        /// </summary>
        /// <param name="data">Tensor segment</param>
        IFloatVector CreateVector(ITensorSegment<float> data);
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
    public interface IFloatVector : IDisposable, IHaveLinearAlgebraProvider
    {
        /// <summary>
        /// Checks if the vector has not been disposed
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Converts the vector to a column matrix
        /// </summary>
        IFloatMatrix ReshapeAsColumnMatrix();

        /// <summary>
        /// Converts the vector to a row matrix
        /// </summary>
        IFloatMatrix ReshapeAsRowMatrix();

        /// <summary>
        /// The number of elements in the vector
        /// </summary>
        uint Count { get; }

        /// <summary>
        /// Converts the vector
        /// </summary>
        Vector<float> Data { get; set; }

        /// <summary>
        /// Adds a vector (without in place modification)
        /// </summary>
        /// <param name="vector">The vector to add</param>
        IFloatVector Add(IFloatVector vector);

        /// <summary>
        /// Subtracts a vector (without in place modification)
        /// </summary>
        /// <param name="vector">The vector to subtract</param>
        IFloatVector Subtract(IFloatVector vector);

        /// <summary>
        /// Calculates the absolute values (L1) norm: https://en.wikipedia.org/wiki/Norm_(mathematics)
        /// </summary>
        float L1Norm();

        /// <summary>
        /// Calculates the euclidean (L2) norm: https://en.wikipedia.org/wiki/Norm_(mathematics)
        /// </summary>
        float L2Norm();

        /// <summary>
        /// Returns the index of the vector with the greatest absolute value
        /// </summary>
        uint MaximumAbsoluteIndex();

        /// <summary>
        /// Returns the index of the vector with the smallest absolute value
        /// </summary>
        uint MinimumAbsoluteIndex();

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
        void AddInPlace(IFloatVector vector, float coefficient1 = 1.0f, float coefficient2 = 1.0f);

        /// <summary>
        /// Subtracts a vector in place
        /// </summary>
        /// <param name="vector">The target vector to subtract from the current vector</param>
        /// <param name="coefficient1">A value to multiply each element of the current vector</param>
        /// <param name="coefficient2">A value to multiply each element of the target vector</param>
        void SubtractInPlace(IFloatVector vector, float coefficient1 = 1.0f, float coefficient2 = 1.0f);

        /// <summary>
        /// Converts the vector to an indexable vector
        /// </summary>
        IIndexableFloatVector AsIndexable();

        /// <summary>
        /// Pointwise multiplication (without in place modification) with a vector
        /// </summary>
        IFloatVector PointwiseMultiply(IFloatVector vector);

        /// <summary>
        /// The dot product of two vectors
        /// </summary>
        /// <param name="vector">The target vector</param>
        float DotProduct(IFloatVector vector);

        /// <summary>
        /// Returns a new vector from a subset of the vector indices
        /// </summary>
        /// <param name="indices">A list of indexes to use as the source of the new vector</param>
        IFloatVector GetNewVectorFromIndexes(IEnumerable<uint> indices);

        /// <summary>
        /// Creates a new copy of the vector
        /// </summary>
        IFloatVector Clone();

        /// <summary>
        /// Creates a new vector in which each element is the square root of the current vector
        /// </summary>
        IFloatVector Sqrt();

        /// <summary>
        /// Creates a new vector in which each element is the absolute value of the current vector
        /// </summary>
        IFloatVector Abs();

        /// <summary>
        /// Copies values from the target vector into the current vector
        /// </summary>
        /// <param name="vector"></param>
        void CopyFrom(IFloatVector vector);

        /// <summary>
        /// Calculates the euclidean distance between the current and the target vector
        /// </summary>
        /// <param name="vector">The target vector</param>
        float EuclideanDistance(IFloatVector vector);

        /// <summary>
        /// Calculates the cosine distance between the current and the target vector
        /// </summary>
        /// <param name="vector">The target vector></param>
        float CosineDistance(IFloatVector vector);

        /// <summary>
        /// Calculates the manhattan distance between the current and the target vector
        /// </summary>
        /// <param name="vector">The target vector</param>
        float ManhattanDistance(IFloatVector vector);

        /// <summary>
        /// Calculates the mean squared distance between the current and the target vector
        /// </summary>
        /// <param name="vector">The target vector</param>
        float MeanSquaredDistance(IFloatVector vector);

        /// <summary>
        /// Calculates the squared euclidean distance between the current and the target vector
        /// </summary>
        /// <param name="vector">The target vector</param>
        float SquaredEuclidean(IFloatVector vector);

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
        /// Normalizes (in place) the values of the current vector
        /// </summary>
        /// <param name="type">The type of normalisation</param>
        void Normalize(NormalizationType type);

        /// <summary>
        /// Returns the softmax function (without in place modification) applied to the current vector
        /// https://en.wikipedia.org/wiki/Softmax_function
        /// </summary>
        IFloatVector Softmax();

        /// <summary>
        /// Returns the jacobian matrix of the softmax derivative
        /// </summary>
        /// <returns></returns>
        IFloatMatrix SoftmaxDerivative();

        /// <summary>
        /// Returns a vector of distances between the current and target vectors
        /// </summary>
        /// <param name="data">The list of target vectors</param>
        /// <param name="distance">The distance metric</param>
        /// <returns>A vector in which each element n is the distance between the current and the nth target vector</returns>
        IFloatVector FindDistances(IFloatVector[] data, DistanceMetric distance);

        /// <summary>
        /// Returns the distance between the current and the target vector
        /// </summary>
        /// <param name="other">The target vector</param>
        /// <param name="distance">The distance metric</param>
        float FindDistance(IFloatVector other, DistanceMetric distance);

        /// <summary>
        /// Returns a vector of the cosine distance between the current and target vectors
        /// </summary>
        /// <param name="data">The list of target vectors</param>
        /// <param name="dataNorm">A buffer to hold the norms of the target vectors</param>
        /// <returns>A vector in which each element n is the cosine distance between the current and the nth target vector</returns>
        IFloatVector CosineDistance(IFloatVector[] data, ref float[]? dataNorm);

        /// <summary>
        /// Returns a vector (without in place modification) in which each element is the natural log of each element in the current vector
        /// </summary>
        IFloatVector Log();

        /// <summary>
        /// Returns the sigmoid function (without in place modification) applied to the current vector
        /// </summary>
        IFloatVector Sigmoid();

        /// <summary>
        /// Fast conversion to matrix (internal buffer is used directly)
        /// </summary>
        /// <param name="rows">The number of rows in the matrix</param>
        /// <param name="columns">The number of columns in the matrix</param>
        IFloatMatrix ReshapeAsMatrix(uint rows, uint columns);

        /// <summary>
        /// Converts the vector to a 3D tensor
        /// </summary>
        /// <param name="rows">Number of rows in each matrix</param>
        /// <param name="columns">Number of columns in matrix</param>
        /// <param name="depth">Number of matrices</param>
        /// <returns></returns>
        I3DFloatTensor ReshapeAs3DTensor(uint rows, uint columns, uint depth);

        /// <summary>
        /// Converts the vector to a 4D tensor
        /// </summary>
        /// <param name="rows">Number of rows in each matrix</param>
        /// <param name="columns">Number of columns in matrix</param>
        /// <param name="depth">Number of matrices</param>
        /// <param name="count">Number of 3D tensors</param>
        /// <returns></returns>
        I4DFloatTensor ReshapeAs4DTensor(uint rows, uint columns, uint depth, uint count);

        /// <summary>
        /// Splits the vector into a list of vectors
        /// </summary>
        /// <param name="blockCount">The number of sub vectors to split into</param>
        IFloatVector[] Split(uint blockCount);

        /// <summary>
        /// Rotates values in the vector (both horizontally and vertically within blocks)
        /// </summary>
        /// <param name="blockCount"></param>
        void RotateInPlace(uint blockCount = 1);

        /// <summary>
        /// Returns a reversed copy of the vector's values
        /// </summary>
        /// <returns></returns>
        IFloatVector Reverse();

        /// <summary>
        /// Returns the value at the specified index
        /// </summary>
        /// <param name="index">The index of the vector to return</param>
        /// <returns></returns>
        float GetAt(uint index);

        /// <summary>
        /// Updates the value at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        void SetAt(uint index, float value);

        /// <summary>
        /// Checks if every value in the vector is finite (not NaN or positive/negative infinity)
        /// </summary>
        /// <returns></returns>
        bool IsEntirelyFinite();

        /// <summary>
        /// Rounds each value to either upper (if >= mid) or lower
        /// </summary>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        /// <param name="mid"></param>
        void RoundInPlace(float lower = 0f, float upper = 1f, float mid = 0.5f);
    }

    /// <summary>
    /// Returns an indexable vector (in which elements can be directly indexed)
    /// </summary>
    public interface IIndexableFloatVector : IFloatVector
    {
        /// <summary>
        /// Returns an element at the specified index
        /// </summary>
        /// <param name="index">The index to retrieve</param>
        float this[uint index] { get; set; }

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
        IIndexableFloatVector Append(params float[] data);
    }

    /// <summary>
    /// A matrix
    /// </summary>
    public interface IFloatMatrix : IDisposable, IHaveLinearAlgebraProvider
    {
        /// <summary>
        /// Checks if the matrix has not been disposed
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Multiplies the current vector (without in place modification) with the target matrix
        /// </summary>
        /// <param name="matrix">The target matrix</param>
        IFloatMatrix Multiply(IFloatMatrix matrix);

        /// <summary>
        /// The number of columns
        /// </summary>
        uint ColumnCount { get; }

        /// <summary>
        /// The number of rows
        /// </summary>
        uint RowCount { get; }

        /// <summary>
        /// Returns a column as a vector
        /// </summary>
        /// <param name="index">The column index</param>
        IFloatVector Column(uint index);

        /// <summary>
        /// Returns the matrix diagonal as a vector
        /// </summary>
        IFloatVector Diagonal();

        /// <summary>
        /// Returns a row as a vector
        /// </summary>
        /// <param name="index">The row index</param>
        IFloatVector Row(uint index);

        /// <summary>
        /// Returns the current matrix (without in place modification) added to the target matrix
        /// </summary>
        /// <param name="matrix">The target matrix</param>
        IFloatMatrix Add(IFloatMatrix matrix);

        /// <summary>
        /// Returns the current matrix  (without in place modification) minus the target matrix
        /// </summary>
        /// <param name="matrix">The target matrix</param>
        IFloatMatrix Subtract(IFloatMatrix matrix);

        /// <summary>
        /// Returns the pointwise product of the current matrix (without in place modification) with the target matrix
        /// </summary>
        /// <param name="matrix">The target matrix</param>
        IFloatMatrix PointwiseMultiply(IFloatMatrix matrix);

        /// <summary>
        /// Returns the current matrix (without in place modification) and multipled with the transposed target matrix
        /// </summary>
        /// <param name="matrix">The target matrix</param>
        IFloatMatrix TransposeAndMultiply(IFloatMatrix matrix);

        /// <summary>
        /// Returns the transpose of the current matrix (without in place modification) multipled with the target matrix
        /// </summary>
        /// <param name="matrix"></param>
        IFloatMatrix TransposeThisAndMultiply(IFloatMatrix matrix);

        /// <summary>
        /// Returns a vector that contains the sum of the elements in each row of the current matrix
        /// </summary>
        IFloatVector RowSums();

        /// <summary>
        /// Returns a vector that contains the sum of the elements in each column of the current matrix
        /// </summary>
        IFloatVector ColumnSums();

        /// <summary>
        /// Returns the transpose of the current matrix
        /// </summary>
        IFloatMatrix Transpose();

        /// <summary>
        /// Multiplies (in place) each element of the matrix by a scalar
        /// </summary>
        /// <param name="scalar">The scalar to multiply each element</param>
        void Multiply(float scalar);

        /// <summary>
        /// Returns the product of the current matrix (without in place modification) with the target vector
        /// </summary>
        /// <param name="vector">The target vector</param>
        IFloatMatrix Multiply(IFloatVector vector);

        /// <summary>
        /// Adds the target matrix to the current matrix (in place)
        /// </summary>
        /// <param name="matrix">The target matrix</param>
        /// <param name="coefficient1">A coefficient to multiply each element of the current matrix</param>
        /// <param name="coefficient2">A coefficient to multipy each element of the target matrix</param>
        void AddInPlace(IFloatMatrix matrix, float coefficient1 = 1.0f, float coefficient2 = 1.0f);

        /// <summary>
        /// Subtracts the target matrix from the current matrix (in place)
        /// </summary>
        /// <param name="matrix">The target matrix</param>
        /// <param name="coefficient1">A coefficient to multiply each element of the current matrix</param>
        /// <param name="coefficient2">A coefficient to multipy each element of the target matrix</param>
        void SubtractInPlace(IFloatMatrix matrix, float coefficient1 = 1.0f, float coefficient2 = 1.0f);

        /// <summary>
        /// Returns a new matrix with the sigmoid function applied to each element
        /// </summary>
        IFloatMatrix SigmoidActivation();

        /// <summary>
        /// Returns a new matrix with the sigmoid derivative of each element
        /// </summary>
        IFloatMatrix SigmoidDerivative();

        /// <summary>
        /// Returns a new matrix with the tanh function applied to each element
        /// </summary>
        IFloatMatrix TanhActivation();

        /// <summary>
        /// Returns a new matrix with the tanh derivative of each element
        /// </summary>
        IFloatMatrix TanhDerivative();

        /// <summary>
        /// Returns a new matrix with the softmax function applied to each row of the matrix
        /// </summary>
        IFloatMatrix SoftmaxActivation();

        /// <summary>
        /// Adds the target vector to each row of the current matrix (in place)
        /// </summary>
        /// <param name="vector">The target vector</param>
        void AddToEachRow(IFloatVector vector);

        /// <summary>
        /// Adds the target vector to each column of the current matrix (in place)
        /// </summary>
        /// <param name="vector">The target vector</param>
        void AddToEachColumn(IFloatVector vector);

        /// <summary>
        /// Converts the current matrix
        /// </summary>
        Matrix<float> Data { get; set; }

        /// <summary>
        /// Converts the matrix to an indexable matrix
        /// </summary>
        IIndexableFloatMatrix AsIndexable();

        /// <summary>
        /// Returns a new matrix from a subset of the current matrix's rows
        /// </summary>
        /// <param name="rowIndexes">The list of row indices</param>
        IFloatMatrix GetNewMatrixFromRows(IEnumerable<uint> rowIndexes);

        /// <summary>
        /// Returns a new matrix from a subset of the current matrix's columns
        /// </summary>
        /// <param name="columnIndexes">The list of column indices</param>
        IFloatMatrix GetNewMatrixFromColumns(IEnumerable<uint> columnIndexes);

        /// <summary>
        /// Set to zero the specified rows in the current matrix
        /// </summary>
        /// <param name="indexes">The list of row indices</param>
        void ClearRows(IEnumerable<uint> indexes);

        /// <summary>
        /// Set to zero the specified columns in the current matrix
        /// </summary>
        /// <param name="indexes">The list of column indices</param>
        void ClearColumns(IEnumerable<uint> indexes);

        /// <summary>
        /// Returns the RELU function applied to each element of the current matrix
        /// </summary>
        IFloatMatrix ReluActivation();

        /// <summary>
        /// Returns the RELU derivative of each element in the current matrix
        /// </summary>
        IFloatMatrix ReluDerivative();

        /// <summary>
        /// Returns the leaky RELU function applied to each element in the current matrix
        /// </summary>
        IFloatMatrix LeakyReluActivation();

        /// <summary>
        /// Returns the leaky RELU derivative of each element in the current matrix
        /// </summary>
        IFloatMatrix LeakyReluDerivative();

        /// <summary>
        /// Creates a copy of the current matrix
        /// </summary>
        IFloatMatrix Clone();

        /// <summary>
        /// Sets each element to zero
        /// </summary>
        void Clear();

        /// <summary>
        /// Returns the square root of each element in the current matrix
        /// </summary>
        IFloatMatrix Sqrt();

        /// <summary>
        /// Returns each element raised to specified power
        /// </summary>
        /// <param name="power">The power to apply to each element</param>
        IFloatMatrix Pow(float power);

        /// <summary>
        /// Returns the current matrix (not modified in place) divided by the target matrix
        /// </summary>
        /// <param name="matrix">The target matrix</param>
        IFloatMatrix PointwiseDivide(IFloatMatrix matrix);

        /// <summary>
        /// L1 Regularisation applied to each element of the current matrix (in place)
        /// </summary>
        /// <param name="coefficient">The L1 coefficient</param>
        void L1Regularisation(float coefficient);

        /// <summary>
        /// Returns a vector of the L2 norms of each column
        /// </summary>
        IFloatVector ColumnL2Norm();

        /// <summary>
        /// Returns a vector of the L2 norms of each row
        /// </summary>
        IFloatVector RowL2Norm();

        /// <summary>
        /// Pointwise divide each row by the target vector (in place)
        /// </summary>
        /// <param name="vector">The target vector</param>
        void PointwiseDivideRows(IFloatVector vector);

        /// <summary>
        /// Pointwise divide each column by the target vector (in place)
        /// </summary>
        /// <param name="vector">The target vector</param>
        void PointwiseDivideColumns(IFloatVector vector);

        /// <summary>
        /// Constrain each value within the specified min and max values (in place)
        /// </summary>
        /// <param name="min">The minimum allowed value</param>
        /// <param name="max">The maximum allowed value</param>
        void Constrain(float min, float max);

        /// <summary>
        /// Returns a segment from a row of the current matrix
        /// </summary>
        /// <param name="rowIndex">The row index</param>
        /// <param name="columnIndex">The start index to return</param>
        /// <param name="length">The number of elements to return</param>
        IFloatVector GetRowSegment(uint rowIndex, uint columnIndex, uint length);

        /// <summary>
        /// Returns a segment from a column of the current matrix
        /// </summary>
        /// <param name="columnIndex">The column index</param>
        /// <param name="rowIndex">The start index to return</param>
        /// <param name="length">The number of elements to return</param>
        IFloatVector GetColumnSegment(uint columnIndex, uint rowIndex, uint length);

        /// <summary>
        /// Returns a new matrix with the columns of the target matrix appended to each column of the current matrix
        /// </summary>
        /// <param name="bottom">The target matrix</param>
        IFloatMatrix ConcatColumns(IFloatMatrix bottom);

        /// <summary>
        /// Returns a new matrix with the rows of the target matrix appended to each row of the current matrix
        /// </summary>
        /// <param name="right">The target matrix</param>
        IFloatMatrix ConcatRows(IFloatMatrix right);

        /// <summary>
        /// Splits the rows of the current matrix into two matrices
        /// </summary>
        /// <param name="columnIndex">The column index at which to split</param>
        (IFloatMatrix Left, IFloatMatrix Right) SplitAtColumn(uint columnIndex);

        /// <summary>
        /// Splits the columns of the current matrix into two matrices
        /// </summary>
        /// <param name="rowIndex">The row index at which to split</param>
        (IFloatMatrix Top, IFloatMatrix Bottom) SplitAtRow(uint rowIndex);

        /// <summary>
        /// Singular value decomposition
        /// </summary>
        (IFloatMatrix U, IFloatVector S, IFloatMatrix VT) Svd();

        /// <summary>
        /// Fast conversion to vector (the internal buffer is not modified)
        /// </summary>
        IFloatVector ReshapeAsVector();

        /// <summary>
        /// Reshapes the matrix to a 3D tensor, treating each column as a depth slice in the new 3D tensor
        /// </summary>
        /// <param name="rows">Row count of each sub matrix</param>
        /// <param name="columns">Column count of each sub matrix</param>
        /// <returns></returns>
        I3DFloatTensor ReshapeAs3DTensor(uint rows, uint columns);

        /// <summary>
        /// Converts the matrix to a 4D tensor, treating each column as a 3D tensor
        /// </summary>
        /// <param name="rows">Row count of each sub matrix</param>
        /// <param name="columns">Column count of each sub matrix</param>
        /// <param name="depth">Depth of each 3D tensor</param>
        /// <returns></returns>
        I4DFloatTensor ReshapeAs4DTensor(uint rows, uint columns, uint depth);

        /// <summary>
        /// Returns the value at the specified row and column index
        /// </summary>
        /// <param name="row">Row index</param>
        /// <param name="column">Column index</param>
        /// <returns></returns>
        float GetAt(uint row, uint column);

        /// <summary>
        /// Updates the value at the specified row and column index
        /// </summary>
        /// <param name="row">Row index</param>
        /// <param name="column">Column index</param>
        /// <param name="value">Value to set</param>
        void SetAt(uint row, uint column, float value);

        /// <summary>
        /// Returns the columns of the matrix as vectors
        /// </summary>
        /// <returns></returns>
        IFloatVector[] ColumnVectors();

        /// <summary>
        /// Returns the rows of the matrix as vectors
        /// </summary>
        /// <returns></returns>
        IFloatVector[] RowVectors();
    }

    /// <summary>
    /// A matrix whose elements can be indexed directly
    /// </summary>
    public interface IIndexableFloatMatrix : IFloatMatrix
    {
        /// <summary>
        /// Returns an element from the current matrix
        /// </summary>
        /// <param name="row">Row index</param>
        /// <param name="column">Column index</param>
        float this[uint row, uint column] { get; set; }

        /// <summary>
        /// Returns the rows of the current matrix as vectors
        /// </summary>
        IEnumerable<IIndexableFloatVector> Rows { get; }

        /// <summary>
        /// Returns the columns of the current matrix as vectors
        /// </summary>
        IEnumerable<IIndexableFloatVector> Columns { get; }

        /// <summary>
        /// Returns each element in the current matrix as enumerable
        /// </summary>
        IEnumerable<float> Values { get; }

        /// <summary>
        /// Mutates each element of the current matrix
        /// </summary>
        /// <param name="mutator">The function to apply to each element</param>
        /// <returns></returns>
        IIndexableFloatMatrix Map(Func<float, float> mutator);

        /// <summary>
        /// Mutates each element of the current matrix
        /// </summary>
        /// <param name="mutator">The function to apply to each element (rowIndex: uint, columnIndex: uint, value: float) => float</param>
        /// <returns></returns>
        IIndexableFloatMatrix MapIndexed(Func<uint, uint, float, float> mutator);

        /// <summary>
        /// Returns the matrix as xml
        /// </summary>
        string AsXml { get; }
    }

    /// <summary>
    /// A list of matrices
    /// </summary>
    public interface I3DFloatTensor : IDisposable, IHaveLinearAlgebraProvider
    {
        /// <summary>
        /// The number of rows in each matrix
        /// </summary>
        uint RowCount { get; }

        /// <summary>
        /// The number of columns in each matrix
        /// </summary>
        uint ColumnCount { get; }

        /// <summary>
        /// The number of matrices
        /// </summary>
        uint Depth { get; }

        /// <summary>
        /// Converts the current tensor
        /// </summary>
        Tensor3D<float> Data { get; set; }

        /// <summary>
        /// Returns a matrix at the specified depth
        /// </summary>
        /// <param name="depth">The depth to query</param>
        /// <returns></returns>
        IFloatMatrix GetMatrixAt(uint depth);

        /// <summary>
        /// Returns an indexable 3D tensor
        /// </summary>
        /// <returns></returns>
        IIndexable3DFloatTensor AsIndexable();

        /// <summary>
        /// Adds padding to each matrix
        /// </summary>
        /// <param name="padding">The padding (both vertical and horizontal)</param>
        /// <returns>A new tensor</returns>
        I3DFloatTensor AddPadding(uint padding);

        /// <summary>
        /// Removes padding from each matrix
        /// </summary>
        /// <param name="padding">The padding to remove</param>
        /// <returns>A new tensor</returns>
        I3DFloatTensor RemovePadding(uint padding);

        /// <summary>
        /// Performs a convolution on each source matrix
        /// </summary>
        /// <param name="filterWidth">The filter width</param>
        /// <param name="filterHeight">The filter height</param>
        /// <param name="xStride">Filter x stride</param>
        /// <param name="yStride">Filter y stride</param>
        /// <returns></returns>
        IFloatMatrix Im2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride);

        /// <summary>
        /// Converts the tensor to a vector
        /// </summary>
        /// <returns></returns>
        IFloatVector ReshapeAsVector();

        /// <summary>
        /// Converts the tensor to a matrix (each depth slice becomes a column in the new matrix)
        /// </summary>
        /// <returns></returns>
        IFloatMatrix ReshapeAsMatrix();

        /// <summary>
        /// Reshapes the 3D tensor into a 4D tensor (the current depth becomes the count of 3D tensors and columns becomes the new depth)
        /// </summary>
        /// <param name="rows">Rows in each 4D tensor</param>
        /// <param name="columns">Columns in each 4D tensor</param>
        I4DFloatTensor ReshapeAs4DTensor(uint rows, uint columns);

        /// <summary>
        /// Performs a max pooling operation on the tensor
        /// </summary>
        /// <param name="filterWidth">The pooling filter width</param>
        /// <param name="filterHeight">The pooling filter height</param>
        /// <param name="xStride">Filter x stride</param>
        /// <param name="yStride">Filter y stride</param>
        /// <param name="saveIndices">True to save the indices for a future reverse max pool operation</param>
        /// <returns>A max pooled tensor</returns>
        (I3DFloatTensor Result, I3DFloatTensor? Indices) MaxPool(uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices);

        /// <summary>
        /// Reverses a max pooling operation
        /// </summary>
        /// <param name="outputRows">Input rows</param>
        /// <param name="outputColumns">Input columns</param>
        /// <param name="indices">A tensor that contains the indices of each maximum value that was found per filter</param>
        /// <param name="filterWidth">Width of each filter</param>
        /// <param name="filterHeight">Height of each filter</param>
        /// <param name="xStride">Filter x stride</param>
        /// <param name="yStride">Filter y stride</param>
        I3DFloatTensor ReverseMaxPool(I3DFloatTensor indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride);

        /// <summary>
        /// Reverses a im2col operation
        /// </summary>
        /// <param name="filter">The rotated filters</param>
        /// <param name="outputRows">Rows of the input tensor</param>
        /// <param name="outputColumns">Columns of the input tensor</param>
        /// <param name="outputDepth">Depth of the input tensor</param>
        /// <param name="filterHeight">Height of each filter</param>
        /// <param name="filterWidth">Width of each filter</param>
        /// <param name="xStride">Filter x stride</param>
        /// <param name="yStride">Filter y stride</param>
        /// <returns></returns>
        I3DFloatTensor ReverseIm2Col(IFloatMatrix filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride);

        /// <summary>
        /// Adds each depth slice into a single matrix
        /// </summary>
        /// <returns></returns>
        IFloatMatrix CombineDepthSlices();

        /// <summary>
        /// Adds the other tensor to the current tensor
        /// </summary>
        /// <param name="tensor">Tensor to add</param>
        void AddInPlace(I3DFloatTensor tensor);

        /// <summary>
        /// Multiplies the tensor with the other matrix
        /// </summary>
        /// <param name="matrix">Matrix to multiply with</param>
        /// <returns></returns>
        I3DFloatTensor Multiply(IFloatMatrix matrix);

        /// <summary>
        /// Adds the vector to each row of the tensor
        /// </summary>
        /// <param name="vector">Vector to add to each row</param>
        void AddToEachRow(IFloatVector vector);

        /// <summary>
        /// Transpose each sub matrix in the current tensor before multiplying it with each each sub tensor (converted to a matrix)
        /// </summary>
        /// <param name="tensor">Tensor to multiply with</param>
        I3DFloatTensor TransposeThisAndMultiply(I4DFloatTensor tensor);
    }

    /// <summary>
    /// A 3D tensor that can be directly indexed
    /// </summary>
    public interface IIndexable3DFloatTensor : I3DFloatTensor
    {
        /// <summary>
        /// Returns a value from the tensor
        /// </summary>
        /// <param name="row">The row to query</param>
        /// <param name="column">The column to query</param>
        /// <param name="depth">The depth to query</param>
        float this[uint row, uint column, uint depth] { get; set; }

        /// <summary>
        /// Gets a list of the indexable matrices
        /// </summary>
        IIndexableFloatMatrix[] Matrix { get; }

        /// <summary>
        /// Returns the matrix as xml
        /// </summary>
        string AsXml { get; }
    }

    /// <summary>
    /// A list of 3D tensors
    /// </summary>
    public interface I4DFloatTensor : IDisposable, IHaveLinearAlgebraProvider
    {
        /// <summary>
        /// The number of rows in each 3D tensor
        /// </summary>
        uint RowCount { get; }

        /// <summary>
        /// The number of columns in each 3D tensor
        /// </summary>
        uint ColumnCount { get; }

        /// <summary>
        /// The depth of each 3D tensor
        /// </summary>
        uint Depth { get; }

        /// <summary>
        /// The count of 3D tensors
        /// </summary>
        uint Count { get; }

        /// <summary>
        /// Returns the tensor at the specified index
        /// </summary>
        /// <param name="index">The index to query</param>
        I3DFloatTensor GetTensorAt(uint index);

        /// <summary>
        /// Returns an indexable list of 3D tensors
        /// </summary>
        /// <returns></returns>
        IIndexable4DFloatTensor AsIndexable();

        /// <summary>
        /// Adds padding to the 4D tensor
        /// </summary>
        /// <param name="padding">Padding to add to the left, top, right and bottom edges of the tensor</param>
        /// <returns>A new tensor with the padding added</returns>
        I4DFloatTensor AddPadding(uint padding);

        /// <summary>
        /// Removes padding from the 4D tensor
        /// </summary>
        /// <param name="padding">Padding to remove from the left, top, right and bottom edges of the tensor</param>
        /// <returns>A new tensor with the padding removed</returns>
        I4DFloatTensor RemovePadding(uint padding);

        /// <summary>
        /// Applies a max pooling operation to the current tensor
        /// </summary>
        /// <param name="filterWidth">Max pool filter width</param>
        /// <param name="filterHeight">Max pool filter height</param>
        /// <param name="xStride">Filter x stride</param>
        /// <param name="yStride">Filter y stride</param>
        /// <param name="saveIndices">True to save the indices for a future reverse pool operation</param>
        (I4DFloatTensor Result, I4DFloatTensor? Indices) MaxPool(uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices);

        /// <summary>
        /// Reverses a max pool operation
        /// </summary>
        /// <param name="outputRows">Input tensor rows</param>
        /// <param name="outputColumns">Input tensor columns</param>
        /// <param name="indices">Tensor of indices from MaxPool operation</param>
        /// <param name="filterWidth">Max pool filter width</param>
        /// <param name="filterHeight">Max pool filter height</param>
        /// <param name="xStride">Filter x stride</param>
        /// <param name="yStride">Filter y stride</param>
        /// <returns></returns>
        I4DFloatTensor ReverseMaxPool(I4DFloatTensor indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride);

        /// <summary>
        /// Applies the convolutional filter to each 3D tensor, producing a 3D tensor which can be multipled by the filter matrix
        /// </summary>
        /// <param name="filterWidth">Filter width</param>
        /// <param name="filterHeight">Filter height</param>
        /// <param name="xStride">Filter x stride</param>
        /// <param name="yStride">Filter y stride</param>
        /// <returns></returns>
        I3DFloatTensor Im2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride);

        /// <summary>
        /// Reverse a previously applied im2Col
        /// </summary>
        /// <param name="filter">List of filters that have been rotated 180 degrees</param>
        /// <param name="outputRows">Rows of the input 4D tensor</param>
        /// <param name="outputColumns">Columns of the input 4D tensor</param>
        /// <param name="outputDepth">Depth of the input 4D tensor</param>
        /// <param name="filterWidth">Filter width</param>
        /// <param name="filterHeight">Filter height</param>
        /// <param name="xStride">Filter x stride</param>
        /// <param name="yStride">Filter y stride</param>
        /// <returns></returns>
        I4DFloatTensor ReverseIm2Col(IFloatMatrix filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride);

        /// <summary>
        /// Converts the tensor to a vector
        /// </summary>
        /// <returns></returns>
        IFloatVector ReshapeAsVector();

        /// <summary>
        /// Converts the tensor to a matrix (each 3D tensor becomes a column in the new matrix)
        /// </summary>
        /// <returns></returns>
        IFloatMatrix ReshapeAsMatrix();

        /// <summary>
        /// Converts the current tensor
        /// </summary>
        Tensor3D<float>[] Data { get; }
    }

    /// <summary>
    /// A 4D tensor that can be directly indexed
    /// </summary>
    public interface IIndexable4DFloatTensor : I4DFloatTensor
    {
        /// <summary>
        /// Returns a value from the tensor
        /// </summary>
        /// <param name="row">The row to query</param>
        /// <param name="column">The column to query</param>
        /// <param name="depth">The depth to query</param>
        /// <param name="index">The tensor index to query</param>
        float this[uint row, uint column, uint depth, uint index] { get; set; }

        /// <summary>
        /// Gets a list of the indexable matrices
        /// </summary>
        IIndexable3DFloatTensor[] Tensors { get; }

        /// <summary>
        /// Returns the matrix as xml
        /// </summary>
        string AsXml { get; }
    }
}
