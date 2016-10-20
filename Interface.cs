using BrightWire.Connectionist;
using BrightWire.Models;
using BrightWire.Models.Simple;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BrightWire
{
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
        /// <param name="vectorData">The list of columns in the matrix</param>
        IMatrix Create(IList<IIndexableVector> vectorData);

        /// <summary>
        /// Creates a matrix
        /// </summary>
        /// <param name="matrix">The matrix to use as the initial values</param>
        IMatrix Create(IIndexableMatrix matrix);

        /// <summary>
        /// Creates a matrix
        /// </summary>
        /// <param name="data">The list of rows in the new matrix</param>
        IMatrix CreateMatrix(IReadOnlyList<FloatArray> data);

        /// <summary>
        /// Creates a vector
        /// </summary>
        /// <param name="data">The data in the vector</param>
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
        IMatrix CreateDiagonal(float[] values);

        /// <summary>
        /// Neural network factory
        /// </summary>
        INeuralNetworkFactory NN { get; }

        /// <summary>
        /// Creates a save point in the allocation history
        /// </summary>
        void PushLayer();

        /// <summary>
        /// Releases all allocated memory since the last save point
        /// </summary>
        void PopLayer();
    }

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
        MinMax GetMinMax();

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
        /// Returns a vector of distances between the current and target vectors
        /// </summary>
        /// <param name="data">The list of target vectors</param>
        /// <param name="distance">The distance metric</param>
        /// <returns>A vector in which each element n is the distance between the current and the nth target vector</returns>
        IVector FindDistances(IReadOnlyList<IVector> data, DistanceMetric distance);

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
        /// Not currently implemented
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
        /// Converts the current matrix to an array of protobuf arrays
        /// </summary>
        FloatArray[] Data { get; set; }

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
        /// <param name="index">The row index</param>
        /// <param name="columnIndex">The start index to return</param>
        /// <param name="length">The number of elements to return</param>
        IVector GetRowSegment(int index, int columnIndex, int length);

        /// <summary>
        /// Returns a segment from a column of the current matrix
        /// </summary>
        /// <param name="index">The column index</param>
        /// <param name="rowIndex">The start index to return</param>
        /// <param name="length">The number of elements to return</param>
        IVector GetColumnSegment(int index, int rowIndex, int length);

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
        /// <param name="position">The column index at which to split</param>
        RowSplit SplitRows(int position);

        /// <summary>
        /// Splits the columns of the current matrix into two matrices
        /// </summary>
        /// <param name="position">The row index at which to split</param>
        ColumnSplit SplitColumns(int position);

        /// <summary>
        /// Returns the inverse of the current matrix
        /// </summary>
        IMatrix Inverse();

        /// <summary>
        /// Singular value decomposition
        /// </summary>
        SingularValueDecomposition Svd();
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
    }

    /// <summary>
    /// Activation function
    /// </summary>
    public interface IActivationFunction
    {
        /// <summary>
        /// Returns the activation function applied to the input matrix
        /// </summary>
        /// <param name="data">The input matrix</param>
        /// <returns></returns>
        IMatrix Calculate(IMatrix data);

        /// <summary>
        /// Returns the derivative of the activation function
        /// </summary>
        /// <param name="layerOutput">The layer output (not activation)</param>
        /// <param name="errorSignal">The error from previous layer</param>
        /// <returns></returns>
        IMatrix Derivative(IMatrix layerOutput, IMatrix errorSignal);

        /// <summary>
        /// Apply the activation function to a single input
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        float Calculate(float val);

        /// <summary>
        /// The activation function type
        /// </summary>
        ActivationType Type { get; }
    }

    /// <summary>
    /// A layer of a neural network
    /// </summary>
    public interface INeuralNetworkLayer : IDisposable
    {
        /// <summary>
        /// The number of input neurons
        /// </summary>
        int InputSize { get; }

        /// <summary>
        /// The number of output neurons
        /// </summary>
        int OutputSize { get; }

        /// <summary>
        /// A descriptor of the current layer
        /// </summary>
        LayerDescriptor Descriptor { get; }

        /// <summary>
        /// The layer bias
        /// </summary>
        IVector Bias { get; }

        /// <summary>
        /// The layer connections
        /// </summary>
        IMatrix Weight { get; }

        /// <summary>
        /// The activation function
        /// </summary>
        IActivationFunction Activation { get; }

        /// <summary>
        /// Multiples the input with the connections and adds the layer bias
        /// </summary>
        /// <param name="input">The layer input</param>
        IMatrix Execute(IMatrix input);

        /// <summary>
        /// Executes the layer and applies the activation function
        /// </summary>
        /// <param name="input">The layer input</param>
        IMatrix Activate(IMatrix input);

        /// <summary>
        /// Updates the connections and bias
        /// </summary>
        /// <param name="biasDelta">The change to make to the bias</param>
        /// <param name="weightDelta">The change to make to each connection</param>
        /// <param name="weightCoefficient">A scalar to multiply each connection</param>
        /// <param name="learningRate">A scalar to apply against the deltas</param>
        void Update(IMatrix biasDelta, IMatrix weightDelta, float weightCoefficient, float learningRate);

        /// <summary>
        /// Reads and writes the layer into protobuf format
        /// </summary>
        NetworkLayer LayerInfo { get; set; }
    }

    /// <summary>
    /// A bidirectional neural network layer
    /// </summary>
    public interface INeuralNetworkBidirectionalLayer : IDisposable
    {
        /// <summary>
        /// The forward layer
        /// </summary>
        INeuralNetworkRecurrentLayer Forward { get; }

        /// <summary>
        /// The backward layer
        /// </summary>
        INeuralNetworkRecurrentLayer Backward { get; }

        /// <summary>
        /// Reads and writes the layer into protobuf format
        /// </summary>
        BidirectionalLayer LayerInfo { get; set; }
    }

    /// <summary>
    /// A layer within a recurrent neural network
    /// </summary>
    public interface INeuralNetworkRecurrentLayer : IDisposable
    {
        /// <summary>
        /// False if the layer is a feed forward layer within the recurrent network
        /// </summary>
        bool IsRecurrent { get; }

        /// <summary>
        /// Executes the recurrent layer
        /// </summary>
        /// <param name="curr">The sequence of inputs</param>
        /// <param name="backpropagate">True to calculate errors for backpropagation</param>
        /// <returns>Backpropagation data</returns>
        INeuralNetworkRecurrentBackpropagation Execute(List<IMatrix> curr, bool backpropagate);

        /// <summary>
        /// Reads and writes the layer into protobuf format
        /// </summary>
        RecurrentLayer LayerInfo { get; set; }
    }

    /// <summary>
    /// Stored recurrent backpropagation data
    /// </summary>
    public interface INeuralNetworkRecurrentBackpropagation
    {
        /// <summary>
        /// Apply the backpropagation
        /// </summary>
        /// <param name="errorSignal">Error signal from previous layer</param>
        /// <param name="context">Training context</param>
        /// <param name="calculateOutput">True to calculate error signal</param>
        /// <param name="updateAccumulator">Destination for the network updates</param>
        /// <returns>Error signal (if calculateOutput is true)</returns>
        IMatrix Execute(IMatrix errorSignal, ITrainingContext context, bool calculateOutput, INeuralNetworkUpdateAccumulator updateAccumulator);
    }

    /// <summary>
    /// Records a list of updates to apply to layers of a neural network.
    /// The updates will be applied when the accumulator is disposed
    /// </summary>
    public interface INeuralNetworkUpdateAccumulator : IDisposable
    {
        /// <summary>
        /// Record an update against a layer
        /// </summary>
        /// <param name="updater">The layer to update</param>
        /// <param name="bias">The bias delta</param>
        /// <param name="weights">The connections delta</param>
        void Record(INeuralNetworkLayerUpdater updater, IMatrix bias, IMatrix weights);

        /// <summary>
        /// Returns the training context
        /// </summary>
        ITrainingContext Context { get; }

        /// <summary>
        /// Records a named matrix that can later be retrieved
        /// </summary>
        /// <param name="name">The name of the data to retrieve</param>
        /// <returns></returns>
        IMatrix GetData(string name);

        /// <summary>
        /// Records a named matrix that can later be retrieved
        /// </summary>
        /// <param name="name">The name to store against</param>
        /// <param name="data">The data to store</param>
        void SetData(string name, IMatrix data);

        /// <summary>
        /// Clears stored data
        /// </summary>
        void Clear();
    }

    /// <summary>
    /// Applies changes to a neural network layer
    /// </summary>
    public interface INeuralNetworkLayerUpdater : IDisposable
    {
        /// <summary>
        /// The layer to update
        /// </summary>
        INeuralNetworkLayer Layer { get; }

        /// <summary>
        /// Apply an update against the layer
        /// </summary>
        /// <param name="biasDelta">The bias delta</param>
        /// <param name="weightDelta">The connections delta</param>
        /// <param name="context">The training context</param>
        void Update(IMatrix biasDelta, IMatrix weightDelta, ITrainingContext context);
    }

    /// <summary>
    /// A recurrent training context
    /// </summary>
    public interface IRecurrentTrainingContext
    {
        /// <summary>
        /// The training context
        /// </summary>
        ITrainingContext TrainingContext { get; }

        /// <summary>
        /// Adds a filter that will be called as the network executes
        /// </summary>
        /// <param name="filter">The filter to add</param>
        void AddFilter(INeuralNetworkRecurrentTrainerFilter filter);

        /// <summary>
        /// Executes the recurrent network
        /// </summary>
        /// <param name="miniBatch">The mini batch of input data</param>
        /// <param name="memory">The initial network memory</param>
        /// <param name="onT">Callback to be called at each item in the sequence</param>
        void ExecuteForward(ISequentialMiniBatch miniBatch, float[] memory, Action<int, List<IMatrix>> onT);

        /// <summary>
        /// Executes the bidirectional network
        /// </summary>
        /// <param name="miniBatch">The mini batch of input data</param>
        /// <param name="layers">The layers of the bidirectional network</param>
        /// <param name="memoryForward">The initial memory to apply in the forward direction</param>
        /// <param name="memoryBackward">The initial memory to apply in the backward direction</param>
        /// <param name="padding"></param>
        /// <param name="updateStack">The stack of updates</param>
        /// <param name="onFinished">Callback when finished</param>
        void ExecuteBidirectional(
            ISequentialMiniBatch miniBatch,
            IReadOnlyList<INeuralNetworkBidirectionalLayer> layers,
            float[] memoryForward,
            float[] memoryBackward,
            int padding,
            Stack<Tuple<Stack<Tuple<INeuralNetworkRecurrentBackpropagation, INeuralNetworkRecurrentBackpropagation>>, IMatrix, IMatrix, ISequentialMiniBatch, int>> updateStack,
            Action<List<IIndexableVector[]>, List<IMatrix>> onFinished
        );
    }

    /// <summary>
    /// Neural network training context 
    /// </summary>
    public interface ITrainingContext
    {
        /// <summary>
        /// The training cost function
        /// </summary>
        IErrorMetric ErrorMetric { get; }

        /// <summary>
        /// The training rate (learning rate)
        /// </summary>
        float TrainingRate { get; }

        /// <summary>
        /// Count of training samples
        /// </summary>
        int TrainingSamples { get; }

        /// <summary>
        /// The current epoch
        /// </summary>
        int CurrentEpoch { get; }

        /// <summary>
        /// The last training error
        /// </summary>
        double LastTrainingError { get; }

        /// <summary>
        /// How many milliseconds it took to train the last epoch
        /// </summary>
        long EpochMilliseconds { get; }

        /// <summary>
        /// How many seconds it took to train the last epoch
        /// </summary>
        double EpochSeconds { get; }

        /// <summary>
        /// The size of the minibatch
        /// </summary>
        int MiniBatchSize { get; }

        /// <summary>
        /// If training should continue
        /// </summary>
        bool ShouldContinue { get; }

        /// <summary>
        /// Where to log the training as XML
        /// </summary>
        XmlWriter Logger { get; set; }

        /// <summary>
        /// Initialise the training context to zero
        /// </summary>
        void Reset();

        /// <summary>
        /// Called when an epoch is starting
        /// </summary>
        /// <param name="trainingSamples">The number of training samples in the epoch</param>
        void StartEpoch(int trainingSamples);

        /// <summary>
        /// Called when an epoch has ended
        /// </summary>
        /// <param name="trainingError">The training error from the epoch</param>
        void EndEpoch(double trainingError);

        /// <summary>
        /// Called when a mini batch has ended
        /// </summary>
        void EndBatch();

        /// <summary>
        /// Called when a recurrent epoch has completed
        /// </summary>
        /// <param name="trainingError">The training error from the epoch</param>
        /// <param name="context">The recurrent training context</param>
        void EndRecurrentEpoch(double trainingError, IRecurrentTrainingContext context);

        /// <summary>
        /// Writes the score to the console
        /// </summary>
        /// <param name="score">The score</param>
        /// <param name="asPercentage">True if the score is a percentage</param>
        /// <param name="flag">True to show a flag next to the output</param>
        void WriteScore(double score, bool asPercentage, bool flag = false);

        /// <summary>
        /// Divides the current training rate by 3
        /// </summary>
        void ReduceTrainingRate();

        /// <summary>
        /// Schedules a change to a new training rate at the specified epoch
        /// </summary>
        /// <param name="atEpoch">The epoch to introduce the new training rate</param>
        /// <param name="newTrainingRate">The new training rate</param>
        void ScheduleTrainingRateChange(int atEpoch, float newTrainingRate);

        /// <summary>
        /// Event called when an epoch is complete
        /// </summary>
        event Action<ITrainingContext> EpochComplete;

        /// <summary>
        /// Event called when a recurrent epoch is complete
        /// </summary>
        event Action<ITrainingContext, IRecurrentTrainingContext> RecurrentEpochComplete;
    }

    /// <summary>
    /// Neural network layer trainer
    /// </summary>
    public interface INeuralNetworkLayerTrainer : IDisposable
    {
        /// <summary>
        /// The layer updated
        /// </summary>
        INeuralNetworkLayerUpdater LayerUpdater { get; }

        /// <summary>
        /// Execute forwards
        /// </summary>
        /// <param name="input">Mini batch input</param>
        /// <param name="storeForBackpropagation">True to store backpropagation data</param>
        /// <returns>The network output</returns>
        IMatrix FeedForward(IMatrix input, bool storeForBackpropagation);

        /// <summary>
        /// Backpropagate
        /// </summary>
        /// <param name="errorSignal"></param>
        /// <param name="context"></param>
        /// <param name="calculateOutput"></param>
        /// <param name="updates"></param>
        /// <returns></returns>
        IMatrix Backpropagate(IMatrix errorSignal, ITrainingContext context, bool calculateOutput, INeuralNetworkUpdateAccumulator updates = null);

        /// <summary>
        /// Backpropagate
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="errorSignal"></param>
        /// <param name="context"></param>
        /// <param name="calculateOutput"></param>
        /// <param name="updates"></param>
        /// <returns></returns>
        IMatrix Backpropagate(IMatrix input, IMatrix output, IMatrix errorSignal, ITrainingContext context, bool calculateOutput, INeuralNetworkUpdateAccumulator updates = null);
    }

    /// <summary>
    /// Layer activation type
    /// </summary>
    public enum ActivationType
    {
        /// <summary>
        /// No activation function
        /// </summary>
        None = 0,

        /// <summary>
        /// RELU activation: f(x) = max(0, x)
        /// </summary>
        Relu,

        /// <summary>
        /// Leaky RELU activation: f(x) = (x > 0) ? x : 0.01x
        /// </summary>
        LeakyRelu,

        /// <summary>
        /// Sigmoid activation function
        /// </summary>
        Sigmoid,

        /// <summary>
        /// TANH activation function
        /// </summary>
        Tanh,
        //Softmax
    }

    /// <summary>
    /// Layer weight initialisation
    /// </summary>
    public enum WeightInitialisationType
    {
        /// <summary>
        /// Gaussian distribution
        /// </summary>
        Gaussian,

        /// <summary>
        /// Xavier initialisation: http://andyljones.tumblr.com/post/110998971763/an-explanation-of-xavier-initialization
        /// </summary>
        Xavier,

        /// <summary>
        /// Identity matrix: https://arxiv.org/abs/1504.00941
        /// </summary>
        Identity,

        /// <summary>
        /// Identity matrix of 0.1
        /// </summary>
        Identity1,

        /// <summary>
        /// Identity matrix of 0.01
        /// </summary>
        Identity01,

        /// <summary>
        /// Identity matrix of 0.001
        /// </summary>
        Identity001
    }

    /// <summary>
    /// Layer regularisation type
    /// </summary>
    public enum RegularisationType
    {
        /// <summary>
        /// No regularisation
        /// </summary>
        None,

        /// <summary>
        /// L2 regularisation
        /// </summary>
        L2,

        /// <summary>
        /// L1 regularisation
        /// </summary>
        L1
    }

    /// <summary>
    /// Gradient descent optimisation: http://sebastianruder.com/optimizing-gradient-descent/
    /// </summary>
    public enum WeightUpdateType
    {
        /// <summary>
        /// No optimisation
        /// </summary>
        Simple,

        /// <summary>
        /// Momentum
        /// </summary>
        Momentum,

        /// <summary>
        /// Nesterov Momentum
        /// </summary>
        NesterovMomentum,

        /// <summary>
        /// Adagrad
        /// </summary>
        Adagrad,

        /// <summary>
        /// RMSprop
        /// </summary>
        RMSprop,

        /// <summary>
        /// Adam
        /// </summary>
        Adam
    }

    /// <summary>
    /// Layer trainer type: http://www.matthewzeiler.com/pubs/icml2013/icml2013.pdf
    /// </summary>
    public enum LayerTrainerType
    {
        /// <summary>
        /// Standard layer trainer
        /// </summary>
        Standard,

        /// <summary>
        /// Dropout based layer trainer
        /// </summary>
        Dropout,

        /// <summary>
        /// Drop Connection layer trainer
        /// </summary>
        DropConnect
    }

    /// <summary>
    /// Creates neural networks
    /// </summary>
    public interface INeuralNetworkFactory
    {
        /// <summary>
        /// The associated linear algebra provider
        /// </summary>
        ILinearAlgebraProvider LinearAlgebraProvider { get; }

        /// <summary>
        /// Creates a training data provider from a data table
        /// </summary>
        /// <param name="table">The training data</param>
        ITrainingDataProvider CreateTrainingDataProvider(IDataTable table);

        /// <summary>
        /// Creates a training data provider from a list of training examples
        /// </summary>
        /// <param name="data">The training data</param>
        ITrainingDataProvider CreateTrainingDataProvider(IReadOnlyList<TrainingExample> data);

        /// <summary>
        /// Creates a sequential training data provider
        /// </summary>
        /// <param name="data">A list of sequences of training examples</param>
        ISequentialTrainingDataProvider CreateSequentialTrainingDataProvider(IReadOnlyList<TrainingExample[]> data);

        /// <summary>
        /// Creates a sparse sequential training data provider
        /// </summary>
        /// <param name="data">A list of tuple sequences of {input, output}</param>
        //ISequentialTrainingDataProvider CreateSequentialTrainingDataProvider(IReadOnlyList<Tuple<Dictionary<uint, float>, Dictionary<uint, float>>[]> data);

        /// <summary>
        /// Creates a feed forward network from a saved model
        /// </summary>
        /// <param name="network">The saved model</param>
        IStandardExecution CreateFeedForward(FeedForwardNetwork network);

        /// <summary>
        /// Creates a recurrent network from a saved model
        /// </summary>
        /// <param name="network">The saved model</param>
        IRecurrentExecution CreateRecurrent(RecurrentNetwork network);

        /// <summary>
        /// Creates a bidirectional network from a saved model
        /// </summary>
        /// <param name="network">The saved model</param>
        /// <returns></returns>
        IBidirectionalRecurrentExecution CreateBidirectional(BidirectionalNetwork network);

        /// <summary>
        /// Gets an implementation of an activation function
        /// </summary>
        /// <param name="activation">The type of activation function</param>
        IActivationFunction GetActivation(ActivationType activation);

        /// <summary>
        /// Gets an implementation of a weight initialisation strategy
        /// </summary>
        /// <param name="type">The type of weight initialisation strategy</param>
        IWeightInitialisation GetWeightInitialisation(WeightInitialisationType type);

        /// <summary>
        /// Creates a training context
        /// </summary>
        /// <param name="learningRate">The initial training rate</param>
        /// <param name="batchSize">The mini batch size</param>
        /// <param name="errorMetric">The cost function</param>
        ITrainingContext CreateTrainingContext(float learningRate, int batchSize, IErrorMetric errorMetric);

        /// <summary>
        /// Creates a training context
        /// </summary>
        /// <param name="learningRate">The initial training rate</param>
        /// <param name="batchSize">The mini batch size</param>
        /// <param name="errorMetric">The cost function</param>
        ITrainingContext CreateTrainingContext(float learningRate, int batchSize, ErrorMetricType errorMetric);

        /// <summary>
        /// Creates a recurrent training context
        /// </summary>
        /// <param name="trainingContext">The training context</param>
        IRecurrentTrainingContext CreateRecurrentTrainingContext(ITrainingContext trainingContext);

        /// <summary>
        /// Creates a neural network layer
        /// </summary>
        /// <param name="inputSize">The input size</param>
        /// <param name="outputSize">The output size</param>
        /// <param name="descriptor">Layer parameters</param>
        INeuralNetworkLayer CreateLayer(
            int inputSize,
            int outputSize,
            LayerDescriptor descriptor
        );

        /// <summary>
        /// Creates a recurrent neural network layer
        /// </summary>
        /// <param name="inputSize">The input size</param>
        /// <param name="outputSize">The output size</param>
        /// <param name="descriptor">Layer parameters</param>
        INeuralNetworkRecurrentLayer CreateSimpleRecurrentLayer(
            int inputSize,
            int outputSize,
            LayerDescriptor descriptor
        );

        /// <summary>
        /// Creates a feed forward layer in a recurrent neural network
        /// </summary>
        /// <param name="inputSize">The input size</param>
        /// <param name="outputSize">The output size</param>
        /// <param name="descriptor">Layer parameters</param>
        INeuralNetworkRecurrentLayer CreateFeedForwardRecurrentLayer(
            int inputSize,
            int outputSize,
            LayerDescriptor descriptor
        );

        /// <summary>
        /// Creates a LSTM recurrent layer
        /// </summary>
        /// <param name="inputSize">The input size</param>
        /// <param name="outputSize">The output size</param>
        /// <param name="descriptor">Layer parameters</param>
        INeuralNetworkRecurrentLayer CreateLstmRecurrentLayer(
            int inputSize,
            int outputSize,
            LayerDescriptor descriptor
        );

        /// <summary>
        /// Create a bidirectional recurrent layer
        /// </summary>
        /// <param name="forward">The forward layer</param>
        /// <param name="backward">The backward layer</param>
        /// <returns></returns>
        INeuralNetworkBidirectionalLayer CreateBidirectionalLayer(
            INeuralNetworkRecurrentLayer forward,
            INeuralNetworkRecurrentLayer backward = null
        );

        /// <summary>
        /// Creates a mini batch neural network trainer
        /// </summary>
        /// <param name="layer">The list of layers</param>
        /// <param name="calculateTrainingError">True if the training error should be calculated</param>
        INeuralNetworkTrainer CreateBatchTrainer(
            IReadOnlyList<INeuralNetworkLayerTrainer> layer,
            bool calculateTrainingError = true
        );

        /// <summary>
        /// Creates a mini batch neural network trainer
        /// </summary>
        /// <param name="descriptor">Layer parameters</param>
        /// <param name="layerSizes">Layer inputs and outputs - passing 1, 2, 3 creates two layers {1, 2} and {2, 3}</param>
        /// <returns></returns>
        INeuralNetworkTrainer CreateBatchTrainer(
            LayerDescriptor descriptor,
            params int[] layerSizes
        );

        /// <summary>
        /// Creates a mini batch trainer for a recurrent neural network
        /// </summary>
        /// <param name="layer">The list of layers</param>
        /// <param name="calculateTrainingError">True if the training error should be calculated</param>
        /// <returns></returns>
        INeuralNetworkRecurrentBatchTrainer CreateRecurrentBatchTrainer(
            IReadOnlyList<INeuralNetworkRecurrentLayer> layer,
            bool calculateTrainingError = true
        );

        /// <summary>
        /// Creates a mini batch trainer for a bidirectional recurrent neural network
        /// </summary>
        /// <param name="layer">The list of layers</param>
        /// <param name="calculateTrainingError">True if the training error should be calculated</param>
        /// <param name="padding"></param>
        /// <returns></returns>
        INeuralNetworkBidirectionalBatchTrainer CreateBidirectionalBatchTrainer(
            IReadOnlyList<INeuralNetworkBidirectionalLayer> layer,
            bool calculateTrainingError = true,
            int padding = 0
        );

        /// <summary>
        /// Creates a neural network updater
        /// </summary>
        /// <param name="inputSize">The input size</param>
        /// <param name="outputSize">The output size</param>
        /// <param name="descriptor">Layer parameters</param>
        /// <returns></returns>
        INeuralNetworkLayerUpdater CreateUpdater(
            int inputSize,
            int outputSize,
            LayerDescriptor descriptor
        );

        /// <summary>
        /// Creates a neural network updater
        /// </summary>
        /// <param name="layer">The layer to update</param>
        /// <param name="descriptor">The layer parameters</param>
        /// <returns></returns>
        INeuralNetworkLayerUpdater CreateUpdater(
            INeuralNetworkLayer layer,
            LayerDescriptor descriptor
        );

        /// <summary>
        /// Creates a neural network layer trainer
        /// </summary>
        /// <param name="inputSize">The input size</param>
        /// <param name="outputSize">The output size</param>
        /// <param name="descriptor">The layer parameters</param>
        /// <returns></returns>
        INeuralNetworkLayerTrainer CreateTrainer(
            int inputSize,
            int outputSize,
            LayerDescriptor descriptor
        );

        /// <summary>
        /// Creates a feed forward training manager
        /// </summary>
        /// <param name="trainer">The layer trainer</param>
        /// <param name="dataFile">Path to write model</param>
        /// <param name="testData">Test data provider</param>
        /// <param name="autoAdjustOnNoChangeCount">Pass a value to reduce the training rate after specified count of no improvement epochs</param>
        /// <returns></returns>
        IFeedForwardTrainingManager CreateFeedForwardManager(
            INeuralNetworkTrainer trainer,
            string dataFile,
            ITrainingDataProvider testData,
            int? autoAdjustOnNoChangeCount = null
        );

        /// <summary>
        /// Creates a recurrent training manager
        /// </summary>
        /// <param name="trainer">The layer trainer</param>
        /// <param name="dataFile">Path to write model</param>
        /// <param name="testData">Test data provider</param>
        /// <param name="memorySize">The size of the recurrent memory</param>
        /// <param name="autoAdjustOnNoChangeCount">Pass a value to reduce the training rate after specified count of no improvement epochs</param>
        /// <returns></returns>
        IRecurrentTrainingManager CreateRecurrentManager(
            INeuralNetworkRecurrentBatchTrainer trainer,
            string dataFile,
            ISequentialTrainingDataProvider testData,
            int memorySize,
            int? autoAdjustOnNoChangeCount = null
        );

        /// <summary>
        /// Creates a bidirectional training manager
        /// </summary>
        /// <param name="trainer">The layer trainer</param>
        /// <param name="dataFile">Path to write model</param>
        /// <param name="testData">Test data provider</param>
        /// <param name="memorySize">The size of the recurrent memory</param>
        /// <param name="autoAdjustOnNoChangeCount">Pass a value to reduce the training rate after specified count of no improvement epochs</param>
        /// <returns></returns>
        IBidirectionalRecurrentTrainingManager CreateBidirectionalManager(
            INeuralNetworkBidirectionalBatchTrainer trainer,
            string dataFile,
            ISequentialTrainingDataProvider testData,
            int memorySize,
            int? autoAdjustOnNoChangeCount = null
        );
    }

    /// <summary>
    /// The output from feed forward training
    /// </summary>
    public interface IFeedForwardOutput
    {
        /// <summary>
        /// The output from the network
        /// </summary>
        IIndexableVector Output { get; }

        /// <summary>
        /// The output that was specified in the training data
        /// </summary>
        IIndexableVector ExpectedOutput { get; }
    }

    /// <summary>
    /// A neural network trainer
    /// </summary>
    public interface INeuralNetworkTrainer : IDisposable
    {
        /// <summary>
        /// The list of layers
        /// </summary>
        IReadOnlyList<INeuralNetworkLayerTrainer> Layer { get; }

        /// <summary>
        /// Calculates the current cost (obtained from the error metric)
        /// </summary>
        /// <param name="data">The test data</param>
        /// <param name="trainingContext">The training context</param>
        float CalculateCost(ITrainingDataProvider data, ITrainingContext trainingContext);

        /// <summary>
        /// Trains the network for the specified number of epochs
        /// </summary>
        /// <param name="trainingData">The training data</param>
        /// <param name="numEpochs">Number of epochs to train for</param>
        /// <param name="context">The training context</param>
        void Train(ITrainingDataProvider trainingData, int numEpochs, ITrainingContext context);

        /// <summary>
        /// Executes the network
        /// </summary>
        /// <param name="data">The test data</param>
        /// <returns>A list of feed forward outputs</returns>
        IReadOnlyList<IFeedForwardOutput> Execute(ITrainingDataProvider data);

        /// <summary>
        /// Reads and writes the protobuf model of the network
        /// </summary>
        FeedForwardNetwork NetworkInfo { get; set; }

        /// <summary>
        /// Executes the network up to the nth layer (where n is less than the total number of layers)
        /// </summary>
        /// <param name="data">The test data</param>
        /// <param name="layerDepth">The layer index to execute to</param>
        /// <returns>A list of feed forward outputs</returns>
        IEnumerable<IIndexableVector[]> ExecuteToLayer(ITrainingDataProvider data, int layerDepth);
    }

    /// <summary>
    /// Recurrent neural network output
    /// </summary>
    public interface IRecurrentExecutionResults : IFeedForwardOutput
    {
        /// <summary>
        /// The input memory to the recurrent neural network
        /// </summary>
        IIndexableVector Memory { get; }
    }

    /// <summary>
    /// Optional filter on recurrent neural network training
    /// </summary>
    public interface INeuralNetworkRecurrentTrainerFilter
    {
        /// <summary>
        /// Called before the network feeds forward
        /// </summary>
        /// <param name="miniBatch"></param>
        /// <param name="sequenceIndex"></param>
        /// <param name="context"></param>
        void BeforeFeedForward(ISequentialMiniBatch miniBatch, int sequenceIndex, List<IMatrix> context);

        /// <summary>
        /// Called after the network completes backpropagation
        /// </summary>
        /// <param name="miniBatch"></param>
        /// <param name="sequenceIndex"></param>
        /// <param name="errorSignal"></param>
        void AfterBackPropagation(ISequentialMiniBatch miniBatch, int sequenceIndex, IMatrix errorSignal);
    }

    /// <summary>
    /// Recurrent batch trainer
    /// </summary>
    public interface INeuralNetworkRecurrentBatchTrainer : IDisposable
    {
        /// <summary>
        /// The list of layers
        /// </summary>
        IReadOnlyList<INeuralNetworkRecurrentLayer> Layer { get; }

        /// <summary>
        /// The linear algebra provider
        /// </summary>
        ILinearAlgebraProvider LinearAlgebraProvider { get; }

        /// <summary>
        /// Calculates the current network cost (against the error metric)
        /// </summary>
        /// <param name="data">The test data</param>
        /// <param name="memory">The input memory</param>
        /// <param name="context">The training context</param>
        /// <returns></returns>
        float CalculateCost(ISequentialTrainingDataProvider data, float[] memory, IRecurrentTrainingContext context);

        /// <summary>
        /// Trains the network
        /// </summary>
        /// <param name="trainingData">The training data</param>
        /// <param name="memory">The input memory</param>
        /// <param name="numEpochs">Number of epochs to train for</param>
        /// <param name="context">The training context</param>
        /// <returns></returns>
        float[] Train(ISequentialTrainingDataProvider trainingData, float[] memory, int numEpochs, IRecurrentTrainingContext context);

        /// <summary>
        /// Train on a single mini batch
        /// </summary>
        /// <param name="miniBatch">The mini batch</param>
        /// <param name="memory">The input memory</param>
        /// <param name="context">The training context</param>
        /// <param name="beforeBackProp">Callback before backpropagation</param>
        /// <param name="afterBackProp">Callback after backpropagation</param>
        void TrainOnMiniBatch(ISequentialMiniBatch miniBatch, float[] memory, IRecurrentTrainingContext context, Action<IMatrix> beforeBackProp, Action<IMatrix> afterBackProp);

        /// <summary>
        /// Execute the network
        /// </summary>
        /// <param name="trainingData">The test data</param>
        /// <param name="memory">The input memory</param>
        /// <param name="context">The training context</param>
        IReadOnlyList<IRecurrentExecutionResults[]> Execute(ISequentialTrainingDataProvider trainingData, float[] memory, IRecurrentTrainingContext context);

        /// <summary>
        /// Executes a single step of the network
        /// </summary>
        /// <param name="input">The input data</param>
        /// <param name="memory">The network memory</param>
        IRecurrentExecutionResults ExecuteSingleStep(float[] input, float[] memory);

        /// <summary>
        /// Read and write the network as a protobuf model
        /// </summary>
        RecurrentNetwork NetworkInfo { get; set; }
    }

    /// <summary>
    /// Bidirectional batch trainer
    /// </summary>
    public interface INeuralNetworkBidirectionalBatchTrainer : IDisposable
    {
        /// <summary>
        /// The list of bidirectional layers
        /// </summary>
        IReadOnlyList<INeuralNetworkBidirectionalLayer> Layer { get; }

        /// <summary>
        /// The current network cost
        /// </summary>
        /// <param name="data">The test data</param>
        /// <param name="forwardMemory">The forward input memory</param>
        /// <param name="backwardMemory">The backward input memory</param>
        /// <param name="context">The training context</param>
        float CalculateCost(ISequentialTrainingDataProvider data, float[] forwardMemory, float[] backwardMemory, IRecurrentTrainingContext context);

        /// <summary>
        /// Executes the network
        /// </summary>
        /// <param name="trainingData">The test data</param>
        /// <param name="forwardMemory">The forward input memory</param>
        /// <param name="backwardMemory">The backward input memory</param>
        /// <param name="context"></param>
        IReadOnlyList<IRecurrentExecutionResults[]> Execute(ISequentialTrainingDataProvider trainingData, float[] forwardMemory, float[] backwardMemory, IRecurrentTrainingContext context);

        /// <summary>
        /// Trains the network
        /// </summary>
        /// <param name="trainingData"></param>
        /// <param name="forwardMemory"></param>
        /// <param name="backwardMemory"></param>
        /// <param name="numEpochs"></param>
        /// <param name="context"></param>
        /// <returns>The trained input memory</returns>
        BidirectionalMemory Train(ISequentialTrainingDataProvider trainingData, float[] forwardMemory, float[] backwardMemory, int numEpochs, IRecurrentTrainingContext context);

        /// <summary>
        /// Read and write the network as a protobuf model
        /// </summary>
        BidirectionalNetwork NetworkInfo { get; set; }
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
    /// Mini batch of training samples
    /// </summary>
    public interface IMiniBatch : IDisposable
    {
        /// <summary>
        /// A matrix with rows for each sample within the batch
        /// </summary>
        IMatrix Input { get; }

        /// <summary>
        /// A matrix with rows for each expected output (associated with each input row)
        /// </summary>
        IMatrix ExpectedOutput { get; }
    }

    /// <summary>
    /// Provides training data
    /// </summary>
    public interface ITrainingDataProvider
    {
        /// <summary>
        /// Gets the specified rows as a mini batch
        /// </summary>
        /// <param name="rows">The list of row indices</param>
        IMiniBatch GetTrainingData(IReadOnlyList<int> rows);

        /// <summary>
        /// The number of training samples
        /// </summary>
        int Count { get; }

        /// <summary>
        /// The training data input size
        /// </summary>
        int InputSize { get; }

        /// <summary>
        /// The training data output size
        /// </summary>
        int OutputSize { get; }
    }

    /// <summary>
    /// Maps one hot encoded vectors back to classification labels
    /// </summary>
    public interface IDataTableTrainingDataProvider
    {
        /// <summary>
        /// Returns the classification label
        /// </summary>
        /// <param name="columnIndex">The data table column index</param>
        /// <param name="vectorIndex">The one hot vector index</param>
        string GetOutputLabel(int columnIndex, int vectorIndex);
    }

    /// <summary>
    /// A sequence of mini batches
    /// </summary>
    public interface ISequentialMiniBatch : IDisposable
    {
        /// <summary>
        /// The size of the sequence
        /// </summary>
        int SequenceLength { get; }

        /// <summary>
        /// The mini batch size
        /// </summary>
        int BatchSize { get; }

        /// <summary>
        /// The sequence of mini batches
        /// </summary>
        IMatrix[] Input { get; }

        /// <summary>
        /// The rows that create the mini batch
        /// </summary>
        int[] CurrentRows { get; }

        /// <summary>
        /// Gets the expected output at position k
        /// </summary>
        /// <param name="output">The actual output</param>
        /// <param name="k">The sequence index</param>
        IMatrix GetExpectedOutput(IReadOnlyList<IMatrix> output, int k);
    }

    /// <summary>
    /// Provides sequential training data
    /// </summary>
    public interface ISequentialTrainingDataProvider
    {
        /// <summary>
        /// Gets a sequences of mini batches
        /// </summary>
        /// <param name="sequenceLength">The size of the sequence</param>
        /// <param name="rows">The row indices that form the mini batch</param>
        ISequentialMiniBatch GetTrainingData(int sequenceLength, IReadOnlyList<int> rows);

        /// <summary>
        /// An array of sequence information (length of sequence and count of samples of that size)
        /// </summary>
        SequenceInfo[] Length { get; }

        /// <summary>
        /// Total number of training samples
        /// </summary>
        int Count { get; }

        /// <summary>
        /// The input size
        /// </summary>
        int InputSize { get; }

        /// <summary>
        /// The output size
        /// </summary>
        int OutputSize { get; }
    }

    /// <summary>
    /// Weight initialisation strategy
    /// </summary>
    public interface IWeightInitialisation
    {
        /// <summary>
        /// Gets an initial value for a weight matrix
        /// </summary>
        /// <param name="inputSize">The number of input neurons</param>
        /// <param name="outputSize">The number of output nuerons</param>
        /// <param name="i">The ith index in the matrix</param>
        /// <param name="j">The jth index in the matrix</param>
        /// <returns></returns>
        float GetWeight(int inputSize, int outputSize, int i, int j);

        /// <summary>
        /// Gets an initial value for a bias vector
        /// </summary>
        /// <returns></returns>
        float GetBias();
    }

    /// <summary>
    /// Executes a feed forward network
    /// </summary>
    public interface IStandardExecution : IDisposable
    {
        /// <summary>
        /// Executes the network
        /// </summary>
        /// <param name="inputData"></param>
        /// <returns></returns>
        IVector Execute(float[] inputData);

        /// <summary>
        /// Executes the network
        /// </summary>
        /// <param name="inputData"></param>
        /// <returns></returns>
        IVector Execute(IVector inputData);

        /// <summary>
        /// Executes the network
        /// </summary>
        /// <param name="inputData"></param>
        /// <returns></returns>
        IMatrix Execute(IMatrix inputData);

        /// <summary>
        /// Executes the network to a specified layer
        /// </summary>
        /// <param name="inputData"></param>
        /// <param name="depth">The layer depth at which to stop execution</param>
        /// <returns></returns>
        IVector Execute(IVector inputData, int depth);
    }

    /// <summary>
    /// Output from recurrent execution
    /// </summary>
    public interface IRecurrentOutput
    {
        /// <summary>
        /// The network output
        /// </summary>
        IIndexableVector Output { get; }

        /// <summary>
        /// The output memory
        /// </summary>
        IIndexableVector Memory { get; }
    }

    /// <summary>
    /// Executes a bidirectional recurrent network
    /// </summary>
    public interface IBidirectionalRecurrentExecution : IDisposable
    {
        /// <summary>
        /// Executes the network
        /// </summary>
        /// <param name="inputData"></param>
        /// <returns></returns>
        IReadOnlyList<IRecurrentOutput> Execute(IReadOnlyList<float[]> inputData);

        /// <summary>
        /// Executes the network
        /// </summary>
        /// <param name="inputData"></param>
        /// <returns></returns>
        IReadOnlyList<IRecurrentOutput> Execute(IReadOnlyList<IVector> inputData);
    }

    /// <summary>
    /// Executes a recurrent network
    /// </summary>
    public interface IRecurrentExecution : IBidirectionalRecurrentExecution
    {
        /// <summary>
        /// Executes a single item in a sequence
        /// </summary>
        /// <param name="data">The input data</param>
        /// <param name="memory">The input memory</param>
        /// <returns></returns>
        IRecurrentOutput ExecuteSingle(float[] data, float[] memory);

        /// <summary>
        /// Gets the initial network memory
        /// </summary>
        float[] InitialMemory { get; }
    }

    internal interface IDisposableMatrixExecutionLine : IDisposable
    {
        IMatrix Current { get; }
        void Assign(IMatrix matrix);
        void Replace(IMatrix matrix);
        IMatrix Pop();
    }

    internal interface IRecurrentLayerExecution : IDisposable
    {
        void Activate(List<IDisposableMatrixExecutionLine> curr);
    }

    /// <summary>
    /// Recurrent layer type
    /// </summary>
    public enum RecurrentLayerType
    {
        /// <summary>
        /// A feed forward layer within the recurrent network
        /// </summary>
        FeedForward,

        /// <summary>
        /// A Elman style recurrent network layer
        /// </summary>
        SimpleRecurrent,

        /// <summary>
        /// An LSTM recurrent network layer
        /// </summary>
        Lstm
    }

    /// <summary>
    /// Trains a feed forward network
    /// </summary>
    public interface IFeedForwardTrainingManager : IDisposable
    {
        /// <summary>
        /// The network
        /// </summary>
        INeuralNetworkTrainer Trainer { get; }

        /// <summary>
        /// Trains the network
        /// </summary>
        /// <param name="trainingData">Training data</param>
        /// <param name="numEpochs">Number of epochs to train for</param>
        /// <param name="context">The training context</param>
        void Train(ITrainingDataProvider trainingData, int numEpochs, ITrainingContext context);
    }

    /// <summary>
    /// Trains a recurrent network
    /// </summary>
    public interface IRecurrentTrainingManager : IDisposable
    {
        /// <summary>
        /// The network
        /// </summary>
        INeuralNetworkRecurrentBatchTrainer Trainer { get; }

        /// <summary>
        /// Trains the network
        /// </summary>
        /// <param name="trainingData">The training data</param>
        /// <param name="numEpochs">Number of epochs to train for</param>
        /// <param name="context">The training context</param>
        /// <param name="recurrentContext">The recurrent training context</param>
        void Train(ISequentialTrainingDataProvider trainingData, int numEpochs, ITrainingContext context, IRecurrentTrainingContext recurrentContext = null);

        /// <summary>
        /// The current memory
        /// </summary>
        float[] Memory { get; }
    }

    /// <summary>
    /// Trains a bidirectional network
    /// </summary>
    public interface IBidirectionalRecurrentTrainingManager : IDisposable
    {
        /// <summary>
        /// The network to train
        /// </summary>
        INeuralNetworkBidirectionalBatchTrainer Trainer { get; }

        /// <summary>
        /// Trains the network
        /// </summary>
        /// <param name="trainingData">The training data</param>
        /// <param name="numEpochs">Number of epochs to train for</param>
        /// <param name="context">The training context</param>
        /// <param name="recurrentContext">The recurrent training context</param>
        void Train(ISequentialTrainingDataProvider trainingData, int numEpochs, ITrainingContext context, IRecurrentTrainingContext recurrentContext = null);

        /// <summary>
        /// The current memory { forward, backward }
        /// </summary>
        BidirectionalMemory Memory { get; }
    }

    /// <summary>
    /// Error metric or cost function
    /// </summary>
    public enum ErrorMetricType
    {
        /// <summary>
        /// No cost function
        /// </summary>
        None,

        /// <summary>
        /// Compares the indexes of the maximum values
        /// </summary>
        OneHot,

        /// <summary>
        /// Root Mean Squared Error
        /// </summary>
        RMSE,

        /// <summary>
        /// Rounds values to 1 or 0
        /// </summary>
        BinaryClassification,

        /// <summary>
        /// Cross entropy
        /// </summary>
        CrossEntropy,

        /// <summary>
        /// Quadratic
        /// </summary>
        Quadratic
    }

    /// <summary>
    /// Error metric or cost function
    /// </summary>
    public interface IErrorMetric
    {
        /// <summary>
        /// Computes a score from two vectors
        /// </summary>
        /// <param name="output">The actual output</param>
        /// <param name="expectedOutput">The expected output</param>
        /// <returns></returns>
        float Compute(IIndexableVector output, IIndexableVector expectedOutput);

        /// <summary>
        /// True if a higher score is better than a low score
        /// </summary>
        bool HigherIsBetter { get; }

        /// <summary>
        /// True if the score is a percentage
        /// </summary>
        bool DisplayAsPercentage { get; }

        /// <summary>
        /// Calculates the difference between outputs and expected outputs
        /// </summary>
        /// <param name="input"></param>
        /// <param name="expectedOutput"></param>
        /// <returns></returns>
        IMatrix CalculateDelta(IMatrix input, IMatrix expectedOutput);
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
        /// String values
        /// </summary>
        String = 1,

        /// <summary>
        /// Double values
        /// </summary>
        Double,

        /// <summary>
        /// Float values
        /// </summary>
        Float,

        /// <summary>
        /// Long values
        /// </summary>
        Long,

        /// <summary>
        /// Byte values
        /// </summary>
        Byte,
        
        /// <summary>
        /// Integer values
        /// </summary>
        Int,

        /// <summary>
        /// Date values
        /// </summary>
        Date,

        /// <summary>
        /// Boolean values
        /// </summary>
        Boolean
    }

    /// <summary>
    /// A data table row
    /// </summary>
    public interface IRow
    {
        /// <summary>
        /// The data within the row
        /// </summary>
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
        IDataTableAnalysis Analysis { get; }
        //void Process(Func<IRow, int, bool> processor);

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
        TableSplit Split(int? randomSeed = null, double firstSize = 0.8, bool shuffle = true, Stream output1 = null, Stream output2 = null);

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
        IDataTable Normalise(Normalisation normalisationModel, Stream output = null);

        /// <summary>
        /// Builds a normalisation model from the table that can be used to normalise data to the same scale
        /// </summary>
        /// <param name="normalisationType">The type of normalisation</param>
        Normalisation GetNormalisationModel(NormalisationType normalisationType);

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
        IReadOnlyList<RowClassification> Classify(IRowClassifier classifier);

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
        IEnumerable<TableFold> Fold(int k, int? randomSeed = null, bool shuffle = true);

        /// <summary>
        /// Writes the index data to the specified stream
        /// </summary>
        /// <param name="stream">The stream to hold the index data</param>
        void WriteIndexTo(Stream stream);

        /// <summary>
        /// For each classification label - duplicate each data table except for the classification column which is converted to a boolean (true for each matching example)
        /// </summary>
        /// <returns></returns>
        IReadOnlyList<BinaryClassification> ConvertToBinaryClassification();
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
    }

    /// <summary>
    /// Trainer for linear regression models
    /// </summary>
    public interface ILinearRegressionTrainer
    {
        /// <summary>
        /// Attempt to solve the model using matrix inversion (only applicable for small sets of training data)
        /// </summary>
        /// <returns></returns>
        LinearRegression Solve();

        /// <summary>
        /// Solves the model using gradient descent
        /// </summary>
        /// <param name="iterations">Number of training epochs</param>
        /// <param name="learningRate">The training rate</param>
        /// <param name="lambda">Regularisation lambda</param>
        /// <param name="costCallback">Callback with current cost - False to stop training</param>
        /// <returns>A trained model</returns>
        LinearRegression GradientDescent(int iterations, float learningRate, float lambda = 0.1f, Func<float, bool> costCallback = null);

        /// <summary>
        /// Computes the cost of the specified parameters
        /// </summary>
        /// <param name="theta">The model parameters</param>
        /// <param name="lambda">Regularisation lambda</param>
        float ComputeCost(IVector theta, float lambda);
    }

    /// <summary>
    /// Linear regression predictor
    /// </summary>
    public interface ILinearRegressionPredictor : IDisposable
    {
        /// <summary>
        /// Predicts a value from input data
        /// </summary>
        /// <param name="vals">The input data</param>
        float Predict(params float[] vals);

        /// <summary>
        /// Predicts a value from input data
        /// </summary>
        /// <param name="vals">The input data</param>
        float Predict(IReadOnlyList<float> vals);

        /// <summary>
        /// Bulk value prediction
        /// </summary>
        /// <param name="input">List of data to predict</param>
        /// <returns>List of predictions</returns>
        float[] Predict(IReadOnlyList<IReadOnlyList<float>> input);
    }

    /// <summary>
    /// A logistic regression trainer
    /// </summary>
    public interface ILogisticRegressionTrainer
    {
        /// <summary>
        /// Trains a model using gradient descent
        /// </summary>
        /// <param name="iterations">Number of training epochs</param>
        /// <param name="learningRate">The training rate</param>
        /// <param name="lambda">Regularisation lambda</param>
        /// <param name="costCallback">Callback with current cost - False to stop training</param>
        /// <returns></returns>
        LogisticRegression GradientDescent(int iterations, float learningRate, float lambda = 0.1f, Func<float, bool> costCallback = null);

        /// <summary>
        /// Computes the cost of the specified parameters
        /// </summary>
        /// <param name="theta">The model parameters</param>
        /// <param name="lambda">Regularisation lambda</param>
        /// <returns></returns>
        float ComputeCost(IVector theta, float lambda);
    }

    /// <summary>
    /// Logistic regression classifier
    /// </summary>
    public interface ILogisticRegressionClassifier : IDisposable
    {
        /// <summary>
        /// Outputs a value from 0 to 1
        /// </summary>
        /// <param name="vals">Input data</param>
        float Predict(params float[] vals);

        /// <summary>
        /// Outputs a value from 0 to 1
        /// </summary>
        /// <param name="vals">Input data</param>
        float Predict(IReadOnlyList<float> vals);

        /// <summary>
        /// Outputs a list of values from 0 to 1 for each input data
        /// </summary>
        /// <param name="input">Input data</param>
        float[] Predict(IReadOnlyList<IReadOnlyList<float>> input);
    }

    /// <summary>
    /// Random projection
    /// </summary>
    public interface IRandomProjection : IDisposable
    {
        /// <summary>
        /// The size to reduce to
        /// </summary>
        int Size { get; }

        /// <summary>
        /// The transformation matrix
        /// </summary>
        IMatrix Matrix { get; }

        /// <summary>
        /// Reduces a vector
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        IVector Compute(IVector vector);

        /// <summary>
        /// Reduces a matrix
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        IMatrix Compute(IMatrix matrix);
    }

    /// <summary>
    /// A classifer that uses a bagged list of features
    /// </summary>
    public interface IIndexBasedClassifier
    {
        /// <summary>
        /// Classifies the input data
        /// </summary>
        /// <param name="featureIndexList">Input data</param>
        /// <returns>A ranked list of classifications</returns>
        IEnumerable<string> Classify(IReadOnlyList<uint> featureIndexList);
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
    }

    /// <summary>
    /// Markov model trainer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMarkovModelTrainer<T>
    {
        /// <summary>
        /// Adds a sequence of items to the trainer
        /// </summary>
        /// <param name="items"></param>
        void Add(IEnumerable<T> items);
    }

    /// <summary>
    /// Markov model trainer (window size 2)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMarkovModelTrainer2<T> : IMarkovModelTrainer<T>
    {
        /// <summary>
        /// Gets all current observations
        /// </summary>
        MarkovModel2<T> Build();
    }

    /// <summary>
    /// Markov model trainer (window size 3)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMarkovModelTrainer3<T> : IMarkovModelTrainer<T>
    {
        /// <summary>
        /// Gets all current observations
        /// </summary>
        MarkovModel3<T> Build();
    }
}
