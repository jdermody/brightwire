using BrightWire.Connectionist;
using BrightWire.Models;
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
        /// <returns>A tuple of {min, max}</returns>
        Tuple<float, float> GetMinMax();

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
        /// <returns>A tuple of {left, right}</returns>
        Tuple<IMatrix, IMatrix> SplitRows(int position);

        /// <summary>
        /// Splits the columns of the current matrix into two matrices
        /// </summary>
        /// <param name="position">The row index at which to split</param>
        /// <returns>A tuple of {top, bottom}</returns>
        Tuple<IMatrix, IMatrix> SplitColumns(int position);

        /// <summary>
        /// Returns the inverse of the current matrix
        /// </summary>
        IMatrix Inverse();
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
        IMatrix Backpropagate(IMatrix errorSignal, ITrainingContext context, bool calculateOutput, INeuralNetworkUpdateAccumulator updates = null);
        IMatrix Backpropagate(IMatrix input, IMatrix output, IMatrix errorSignal, ITrainingContext context, bool calculateOutput, INeuralNetworkUpdateAccumulator updates = null);
    }

    /// <summary>
    /// Layer activation type
    /// </summary>
    public enum ActivationType
    {
        None = 0,
        Relu,
        LeakyRelu,
        Sigmoid,
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
        Standard,
        Dropout,
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

        ITrainingDataProvider CreateTrainingDataProvider(IIndexableDataTable table, int classColumnIndex);
        ITrainingDataProvider CreateTrainingDataProvider(IReadOnlyList<Tuple<float[], float[]>> data);
        ISequentialTrainingDataProvider CreateSequentialTrainingDataProvider(IReadOnlyList<Tuple<float[], float[]>[]> data);

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
        void BeforeFeedForward(ISequentialMiniBatch miniBatch, int sequenceIndex, List<IMatrix> context);
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
        /// <returns></returns>
        Tuple<float[], float[]> Train(ISequentialTrainingDataProvider trainingData, float[] forwardMemory, float[] backwardMemory, int numEpochs, IRecurrentTrainingContext context);

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
        Euclidean,
        Cosine,
        Manhattan,
        MeanSquared,
        SquaredEuclidean
    }

    /// <summary>
    /// Mini batch
    /// </summary>
    public interface IMiniBatch : IDisposable
    {
        IMatrix Input { get; }
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
        /// An array of sequence size tuples where each tuple is { sequence-length, count of samples }
        /// </summary>
        Tuple<int, int>[] Length { get; }

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
        float GetWeight(int inputSize, int outputSize, int i, int j);
        float GetBias();
    }

    /// <summary>
    /// Executes a feed forward network
    /// </summary>
    public interface IStandardExecution : IDisposable
    {
        IVector Execute(float[] inputData);
        IVector Execute(IVector inputData);
        IMatrix Execute(IMatrix inputData);
        IVector Execute(IVector inputData, int depth);
    }

    /// <summary>
    /// Output from recurrent execution
    /// </summary>
    public interface IRecurrentOutput
    {
        IIndexableVector Output { get; }
        IIndexableVector Memory { get; }
    }

    /// <summary>
    /// Executes a bidirectional recurrent network
    /// </summary>
    public interface IBidirectionalRecurrentExecution : IDisposable
    {
        IReadOnlyList<IRecurrentOutput> Execute(IReadOnlyList<float[]> inputData);
        IReadOnlyList<IRecurrentOutput> Execute(IReadOnlyList<IVector> inputData);
    }

    /// <summary>
    /// Executes a recurrent network
    /// </summary>
    public interface IRecurrentExecution : IBidirectionalRecurrentExecution
    {
        IRecurrentOutput ExecuteSingle(float[] data, float[] memory);
        float[] InitialMemory { get; }
    }

    public interface IDisposableMatrixExecutionLine : IDisposable
    {
        IMatrix Current { get; }
        void Assign(IMatrix matrix);
        void Replace(IMatrix matrix);
        IMatrix Pop();
    }

    public interface IRecurrentLayerExecution : IDisposable
    {
        void Activate(List<IDisposableMatrixExecutionLine> curr);
    }

    public enum RecurrentLayerType
    {
        FeedForward,
        SimpleRecurrent,
        Lstm
    }

    public interface IFeedForwardTrainingManager : IDisposable
    {
        INeuralNetworkTrainer Trainer { get; }
        void Train(ITrainingDataProvider trainingData, int numEpochs, ITrainingContext context);
    }

    public interface IRecurrentTrainingManager : IDisposable
    {
        INeuralNetworkRecurrentBatchTrainer Trainer { get; }
        void Train(ISequentialTrainingDataProvider trainingData, int numEpochs, ITrainingContext context, IRecurrentTrainingContext recurrentContext = null);
        float[] Memory { get; }
    }

    public interface IBidirectionalRecurrentTrainingManager : IDisposable
    {
        ILinearAlgebraProvider LinearAlgebraProvider { get; }
        void Train(ISequentialTrainingDataProvider trainingData, int numEpochs, ITrainingContext context, IRecurrentTrainingContext recurrentContext = null);
        Tuple<float[], float[]> Memory { get; }
    }

    public enum ErrorMetricType
    {
        None,
        OneHot,
        RMSE,
        BinaryClassification,
        CrossEntropy,
        Quadratic
    }

    public interface IErrorMetric
    {
        float Compute(IIndexableVector output, IIndexableVector expectedOutput);
        bool HigherIsBetter { get; }
        bool DisplayAsPercentage { get; }
        IMatrix CalculateDelta(IMatrix input, IMatrix expectedOutput);
    }

    public enum ColumnType
    {
        Null = 0,
        String = 1,
        Double,
        Float,
        Long,
        Byte,
        Int,
        Date,
        Boolean
    }

    public interface IRow
    {
        IReadOnlyList<object> Data { get; }
        T GetField<T>(int index);
    }

    public interface IColumn
    {
        string Name { get; }
        ColumnType Type { get; }
        int NumDistinct { get; }
        bool IsContinuous { get; set; }
    }

    public interface IDataTable
    {
        int RowCount { get; }
        int ColumnCount { get; }
        IReadOnlyList<IColumn> Columns { get; }
        void Process(IRowProcessor rowProcessor);
        IIndexableDataTable Index(Stream output = null);
        IDataTableAnalysis Analysis { get; }
        void Process(Func<IRow, int, bool> processor);
    }

    public interface IIndexableDataTable : IDataTable
    {
        IReadOnlyList<IRow> GetSlice(int offset, int count);
        IReadOnlyList<IRow> GetRows(IEnumerable<int> rowIndex);
        Tuple<IIndexableDataTable, IIndexableDataTable> Split(int? randomSeed = null, double trainPercentage = 0.8, bool shuffle = true, Stream output1 = null, Stream output2 = null);
        IIndexableDataTable Normalise(NormalisationType normalisationType, Stream output = null);
        IReadOnlyList<IVector> GetNumericRows(ILinearAlgebraProvider lap, IEnumerable<int> columns = null);
        IReadOnlyList<IVector> GetNumericColumns(ILinearAlgebraProvider lap, IEnumerable<int> columns = null);
        string[] GetDiscreteColumn(int columnIndex);
        float[] GetNumericColumn(int columnIndex);
        IIndexableDataTable Bag(int? count = null, Stream output = null, int? randomSeed = null);
        IReadOnlyList<float[]> GetNumericRows(IEnumerable<int> columns = null);
    }

    public interface IRowProcessor
    {
        bool Process(IRow row);
    }

    public interface IColumnInfo
    {
        int ColumnIndex { get; }
        IEnumerable<object> DistinctValues { get; }
        int? NumDistinct { get; }
    }

    public interface IStringColumnInfo : IColumnInfo
    {
        int MinLength { get; }
        int MaxLength { get; }
        string MostCommonString { get; }
    }

    public interface INumericColumnInfo : IColumnInfo
    {
        double Min { get; }
        double Max { get; }
        double Mean { get; }
        double? StdDev { get; }
        double? Median { get; }
        double? Mode { get; }
        double L1Norm { get; }
        double L2Norm { get; }
    }

    public interface IFrequencyColumnInfo : IColumnInfo
    {
        IEnumerable<KeyValuePair<string, ulong>> Frequency { get; }
    }

    public interface IDataTableAnalysis
    {
        IEnumerable<IColumnInfo> ColumnInfo { get; }
    }
}
