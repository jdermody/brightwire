using System;
using System.Collections.Generic;
using System.Numerics;
using BrightData.Helper;
using BrightData.LinearAlgebra;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData
{
    /// <summary>
    /// Gives access to a linear algebra provider
    /// </summary>
    public interface IHaveLinearAlgebraProvider
    {
        /// <summary>
        /// Linear algebra provider
        /// </summary>
        LinearAlgebraProvider LinearAlgebraProvider { get; }
    }

    /// <summary>
    /// Indicates that the type can set a linear algebra provider
    /// </summary>
    public interface ISetLinearAlgebraProvider
    {
        /// <summary>
        /// Property to set the linear algebra provider
        /// </summary>
        LinearAlgebraProvider LinearAlgebraProvider { set; }

        /// <summary>
        /// Linear algebra provider factory
        /// </summary>
        Func<LinearAlgebraProvider> LinearAlgebraProviderFactory { set; }
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
    /// Read only tensor segment
    /// </summary>
    public interface IReadOnlyNumericSegment<T> : ICountReferences, IDisposable, IHaveSize, IHaveSpanOf<float>
        where T : unmanaged, INumber<T>
    {
        /// <summary>
        /// Segment type
        /// </summary>
        string SegmentType { get; }

        /// <summary>
        /// Returns a value at the index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        T this[int index] { get; }

        /// <summary>
        /// Returns a value at the index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        T this[uint index] { get; }

        /// <summary>
        /// Returns a value at the index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        T this[long index] { get; }

        /// <summary>
        /// Returns a value at the index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        T this[ulong index] { get; }

        /// <summary>
        /// Creates a new array from the segment
        /// </summary>
        /// <returns></returns>
        T[] ToNewArray();

        /// <summary>
        /// Iterates all values in the segment
        /// </summary>
        IEnumerable<T> Values { get; }

        /// <summary>
        /// Copies this segment to another segment
        /// </summary>
        /// <param name="segment">Segment to copy to</param>
        /// <param name="sourceOffset">Index within this segment to copy from</param>
        /// <param name="targetOffset">Index within other segment to replace from</param>
        void CopyTo(INumericSegment<T> segment, uint sourceOffset = 0, uint targetOffset = 0);

        /// <summary>
        /// Copies this segment to a span
        /// </summary>
        /// <param name="destination">Destination span</param>
        void CopyTo(Span<T> destination);

        /// <summary>
        /// Copies to a pointer
        /// </summary>
        /// <param name="destination">Pointer to memory to copy to</param>
        /// <param name="sourceOffset">Index within this segment to copy from</param>
        /// <param name="stride">Increment after each copy</param>
        /// <param name="count">Number of elements to copy</param>
        unsafe void CopyTo(T* destination, int sourceOffset, int stride, int count);

        /// <summary>
        /// Tries to return a contiguous span from the current segment if possible
        /// </summary>
        /// <returns></returns>
        IHaveReadOnlyContiguousSpan<float>? Contiguous { get; }

        /// <summary>
        /// True if the segment wraps another segment
        /// </summary>
        bool IsWrapper { get; }
    }

    /// <summary>
    /// A segment of a float tensor
    /// </summary>
    public interface INumericSegment<T> : IReadOnlyNumericSegment<T>
        where T: unmanaged, INumber<T>
    {
        /// <summary>
        /// Returns a value at the index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        new T this[int index] { get; set; }

        /// <summary>
        /// Returns a value at the index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        new T this[uint index] { get; set; }

        /// <summary>
        /// Returns a value at the index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        new T this[long index] { get; set; }

        /// <summary>
        /// Returns a value at the index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        new T this[ulong index] { get; set; }

        /// <summary>
        /// Copies from the span into the segment
        /// </summary>
        /// <param name="span">Span to copy from</param>
        /// <param name="targetOffset">Index into this segment to replace from</param>
        void CopyFrom(ReadOnlySpan<T> span, uint targetOffset = 0);

        /// <summary>
        /// Sets each value within the segment to zero
        /// </summary>
        void Clear();

        /// <summary>
        /// Returns the underlying array used by the segment (if available)
        /// </summary>
        /// <returns>Array, segment offset and segment stride</returns>
        (T[]? Array, uint Offset, uint Stride) GetUnderlyingArray();
    }

    /// <summary>
    /// Indicates that there is an underlying tensor segment
    /// </summary>
    public interface IHaveReadOnlyTensorSegment<T>
        where T: unmanaged, INumber<T>
    {
        /// <summary>
        /// Underlying tensor segment
        /// </summary>
        IReadOnlyNumericSegment<T> ReadOnlySegment { get; }
    }

    /// <summary>
    /// Indicates that there is an underlying tensor segment
    /// </summary>
    public interface IHaveTensorSegment<T>
        where T: unmanaged, INumber<T>
    {
        /// <summary>
        /// Underlying tensor segment
        /// </summary>
        INumericSegment<T> Segment { get; }
    }

    /// <summary>
    /// Indicates that the type has a contiguous read only float span
    /// </summary>
    public interface IHaveReadOnlyContiguousSpan<T>
    {
        /// <summary>
        /// A read only span of floats
        /// </summary>
        ReadOnlySpan<T> ReadOnlySpan { get; }
    }

    /// <summary>
    /// Vector that cannot be modified
    /// </summary>
    public interface IReadOnlyVector : IHaveSpanOf<float>, IHaveSize, IAmSerializable, IHaveReadOnlyTensorSegment<float>
    {
        /// <summary>
        /// Returns a value at the index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        float this[int index] { get; }

        /// <summary>
        /// Returns a value at the index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        float this[uint index] { get; }

        /// <summary>
        /// Creates a new mutable vector that is a copy of this vector
        /// </summary>
        /// <param name="lap">Linear algebra provider</param>
        /// <returns></returns>
        IVector Create(LinearAlgebraProvider lap);
    }

    /// <summary>
    /// Matrix dimensions
    /// </summary>
    public interface IHaveMatrixDimensions : IHaveSize
    {
        /// <summary>
        /// Number of rows
        /// </summary>
        uint RowCount { get; }

        /// <summary>
        /// Number of columns
        /// </summary>
        uint ColumnCount { get; }
    }

    /// <summary>
    /// Matrix that cannot be modified
    /// </summary>
    public interface IReadOnlyMatrix : IHaveSpanOf<float>, IAmSerializable, IHaveReadOnlyTensorSegment<float>, IHaveMatrixDimensions
    {
        /// <summary>
        /// Returns a value from the matrix
        /// </summary>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        float this[int rowY, int columnX] { get; }

        /// <summary>
        /// Returns a value from the matrix
        /// </summary>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        float this[uint rowY, uint columnX] { get; }

        /// <summary>
        /// Returns a row from the matrix
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        IReadOnlyVector GetRow(uint rowIndex);

        /// <summary>
        /// Returns a column from the matrix
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        IReadOnlyVector GetColumn(uint columnIndex);

        /// <summary>
        /// Returns all rows as an array
        /// </summary>
        /// <returns></returns>
        IReadOnlyVector[] AllRows();

        /// <summary>
        /// Returns all columns as an array
        /// </summary>
        /// <returns></returns>
        IReadOnlyVector[] AllColumns();

        /// <summary>
        /// Creates a new mutable matrix that is a copy of this matrix
        /// </summary>
        /// <param name="lap">Linear algebra provider</param>
        /// <returns></returns>
        IMatrix Create(LinearAlgebraProvider lap);
    }

    /// <summary>
    /// 3D tensor dimensions
    /// </summary>
    public interface IHaveTensor3DDimensions : IHaveMatrixDimensions
    {
        /// <summary>
        /// Number of matrices
        /// </summary>
        uint Depth { get; }

        /// <summary>
        /// Rows * Columns
        /// </summary>
        uint MatrixSize { get; }
    }

    /// <summary>
    /// 3D tensor that cannot be modified
    /// </summary>
    public interface IReadOnlyTensor3D : IHaveSpanOf<float>, IAmSerializable, IHaveReadOnlyTensorSegment<float>, IHaveTensor3DDimensions
    {
        /// <summary>
        /// Returns a value from the 3D tensor
        /// </summary>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        float this[int depth, int rowY, int columnX] { get; }

        /// <summary>
        /// Returns a value from the 3D tensor
        /// </summary>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        float this[uint depth, uint rowY, uint columnX] { get; }

        /// <summary>
        /// Returns a matrix from the 3D tensor
        /// </summary>
        /// <param name="index">Matrix index</param>
        /// <returns></returns>
        IReadOnlyMatrix GetMatrix(uint index);

        /// <summary>
        /// Returns all matrices as an array
        /// </summary>
        /// <returns></returns>
        IReadOnlyMatrix[] AllMatrices();

        /// <summary>
        /// Creates a new mutable tensor that is a copy of this tensor
        /// </summary>
        /// <param name="lap">Linear algebra provider</param>
        /// <returns></returns>
        ITensor3D Create(LinearAlgebraProvider lap);
    }

    /// <summary>
    /// 4D tensor dimensions
    /// </summary>
    public interface IHaveTensor4DDimensions : IHaveTensor3DDimensions
    {
        /// <summary>
        /// Number of 3D tensors
        /// </summary>
        uint Count { get; }

        /// <summary>
        /// MatrixSize * Depth
        /// </summary>
        uint TensorSize { get; }
    }

    /// <summary>
    /// 4D tensor that cannot be modified
    /// </summary>
    public interface IReadOnlyTensor4D : IHaveSpanOf<float>, IAmSerializable, IHaveReadOnlyTensorSegment<float>, IHaveTensor4DDimensions
    {
        /// <summary>
        /// Returns a value from the 4D tensor
        /// </summary>
        /// <param name="count">3D tensor index</param>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        float this[int count, int depth, int rowY, int columnX] { get; }

        /// <summary>
        /// Returns a value from the 4D tensor
        /// </summary>
        /// <param name="count">3D tensor index</param>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        float this[uint count, uint depth, uint rowY, uint columnX] { get; }

        /// <summary>
        /// Returns a 3D tensor from the 4D tensor
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        IReadOnlyTensor3D GetTensor3D(uint index);

        /// <summary>
        /// Returns all tensors as an array
        /// </summary>
        /// <returns></returns>
        IReadOnlyTensor3D[] AllTensors();

        /// <summary>
        /// Creates a new mutable tensor that is a copy of this tensor
        /// </summary>
        /// <param name="lap">Linear algebra provider</param>
        /// <returns></returns>
        ITensor4D Create(LinearAlgebraProvider lap);
    }

    /// <summary>
    /// Untyped tensor interface - vector, matrix, 3D tensor etc
    /// </summary>
    public interface ITensor : IDisposable, IHaveLinearAlgebraProvider, IHaveSpanOf<float>, IHaveSize, IAmSerializable, IHaveReadOnlyTensorSegment<float>, IHaveTensorSegment<float>
    {
        /// <summary>
        /// Underlying bright data context
        /// </summary>
        BrightDataContext Context { get; }

        /// <summary>
        /// Reshapes to a vector
        /// </summary>
        /// <returns></returns>
        IVector Reshape();

        /// <summary>
        /// Reshapes to a matrix
        /// </summary>
        /// <param name="rows">Row count of each matrix (one parameter is optional null)</param>
        /// <param name="columns">Column count of each matrix (one parameter is optional null)</param>
        /// <returns></returns>
        IMatrix Reshape(uint? rows, uint? columns);

        /// <summary>
        /// Reshapes to a 3D tensor
        /// </summary>
        /// <param name="depth">Number of matrices (one parameter is optional null)</param>
        /// <param name="rows">Number of rows in each matrix (one parameter is optional null)</param>
        /// <param name="columns">Number of columns in each matrix (one parameter is optional null)</param>
        /// <returns></returns>
        ITensor3D Reshape(uint? depth, uint? rows, uint? columns);

        /// <summary>
        /// Reshapes to a 4D tensor
        /// </summary>
        /// <param name="count">Number of 3D tensors (one parameter is optional null)</param>
        /// <param name="depth">Number of matrices in each 3D tensor (one parameter is optional null)</param>
        /// <param name="rows">Number of rows in each matrix (one parameter is optional null)</param>
        /// <param name="columns">Number of columns in each matrix (one parameter is optional null)</param>
        /// <returns></returns>
        ITensor4D Reshape(uint? count, uint? depth, uint? rows, uint? columns);

        /// <summary>
        /// Sets all values to zero
        /// </summary>
        void Clear();

        /// <summary>
        /// Creates a copy of this tensor
        /// </summary>
        /// <returns></returns>
        ITensor Clone();

        /// <summary>
        /// Total count of all values
        /// </summary>
        uint TotalSize { get; }

        /// <summary>
        /// Tensor shape - for a vector the array will have a single element, for a matrix it will be [columns, rows], a 3D tensor will be [columns, rows, depth] etc
        /// </summary>
        uint[] Shape { get; }

        /// <summary>
        /// Adds a tensor to this tensor (the result will be stored in this tensor)
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        void AddInPlace(ITensor tensor);

        /// <summary>
        /// Adds a tensor to this tensor (the result will be stored in this tensor)
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        /// <param name="coefficient1">Value to multiply each element of this tensor</param>
        /// <param name="coefficient2">Value to multiply each element of the other tensor</param>
        void AddInPlace(ITensor tensor, float coefficient1, float coefficient2);

        /// <summary>
        /// Adds a value to this tensor (the result will be stored in this tensor)
        /// </summary>
        /// <param name="scalar">Value to add</param>
        void AddInPlace(float scalar);

        /// <summary>
        /// Multiplies a value to this tensor (the result will be stored in this tensor)
        /// </summary>
        /// <param name="scalar">Value to multiply</param>
        void MultiplyInPlace(float scalar);

        /// <summary>
        /// Subtracts a tensor from this tensor (the result will be stored in this tensor)
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        void SubtractInPlace(ITensor tensor);

        /// <summary>
        /// Subtracts a tensor from this tensor (the result will be stored in this tensor)
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        /// <param name="coefficient1">Value to multiply each element of this tensor</param>
        /// <param name="coefficient2">Value to multiply each element of the other tensor</param>
        void SubtractInPlace(ITensor tensor, float coefficient1, float coefficient2);

        /// <summary>
        /// Multiplies each value in this tensor with the corresponding value in the other tensor (the result will be stored in this tensor)
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        void PointwiseMultiplyInPlace(ITensor tensor);

        /// <summary>
        /// Divides each value in this tensor with the corresponding value in the other tensor (the result will be stored in this tensor)
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        void PointwiseDivideInPlace(ITensor tensor);

        /// <summary>
        /// Computes the dot product of this tensor with another tensor (of same length)
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        /// <returns></returns>
        float DotProduct(ITensor tensor);

        /// <summary>
        /// Modifies this tensor so that no value is less than or greater than supplied parameters
        /// </summary>
        /// <param name="minValue">Minimum value to allow (optional)</param>
        /// <param name="maxValue">Maximum value to allow (optional)</param>
        void ConstrainInPlace(float? minValue, float? maxValue);

        /// <summary>
        /// Finds the average value of this tensor
        /// </summary>
        /// <returns></returns>
        float Average();

        /// <summary>
        /// Returns the L1 norm of this tensor (manhattan distance)
        /// </summary>
        /// <returns></returns>
        float L1Norm();

        /// <summary>
        /// Returns the L2 norm of this tensor (euclidean norm)
        /// </summary>
        /// <returns></returns>
        float L2Norm();

        /// <summary>
        /// Checks if the tensor is entirely finite (does not contain NAN or Infinity)
        /// </summary>
        /// <returns></returns>
        bool IsEntirelyFinite();

        /// <summary>
        /// Calculates the cosine distance between this and another tensor
        /// </summary>
        /// <param name="other">Other tensor</param>
        /// <returns></returns>
        float CosineDistance(ITensor other);

        /// <summary>
        /// Calculates the euclidean distance between this and another tensor
        /// </summary>
        /// <param name="other">Other tensor</param>
        /// <returns></returns>
        float EuclideanDistance(ITensor other);

        /// <summary>
        /// Calculates the mean squared distance between this and another tensor
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        float MeanSquaredDistance(ITensor other);

        /// <summary>
        /// Calculates the squared euclidean distance between this and another tensor
        /// </summary>
        /// <param name="other">Other tensor</param>
        /// <returns></returns>
        float SquaredEuclideanDistance(ITensor other);

        /// <summary>
        /// Calculates the manhattan distance between this and another tensor
        /// </summary>
        /// <param name="other">Other tensor</param>
        /// <returns></returns>
        float ManhattanDistance(ITensor other);

        /// <summary>
        /// Calculates the standard deviation of this tensor
        /// </summary>
        /// <param name="mean">Existing mean of tensor if available (otherwise it will be calculated)</param>
        /// <returns></returns>
        float StdDev(float? mean);

        /// <summary>
        /// Rounds each value in this tensor to either the lower or upper parameter (the result will be stored in this tensor)
        /// </summary>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        void RoundInPlace(float lower, float upper);

        /// <summary>
        /// Applies a mapping function to each value of this tensor (the result will be stored in this tensor)
        /// </summary>
        /// <param name="mutator"></param>
        void MapInPlace(Func<float, float> mutator);

        /// <summary>
        /// Applies L1 regularization to this tensor (the result will be stored in this tensor)
        /// </summary>
        /// <param name="coefficient"></param>
        void L1RegularisationInPlace(float coefficient);

        /// <summary>
        /// Sums all values of this tensor
        /// </summary>
        /// <returns></returns>
        float Sum();
    }

    /// <summary>
    /// Typed tensor interface - vector, matrix, 3D tensor etc
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITensor<out T> : ITensor
        where T: ITensor
    {
        /// <summary>
        /// Creates a clone of this tensor
        /// </summary>
        /// <returns></returns>
        new T Clone();

        /// <summary>
        /// Adds a tensor to this tensor
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        /// <returns></returns>
        T Add(ITensor tensor);

        /// <summary>
        /// Adds a tensor to this tensor
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        /// <param name="coefficient1">Coefficient to multiply each value in this tensor</param>
        /// <param name="coefficient2">Coefficient to multiply each value in the other tensor</param>
        /// <returns></returns>
        T Add(ITensor tensor, float coefficient1, float coefficient2);

        /// <summary>
        /// Adds a value to each element in this tensor
        /// </summary>
        /// <param name="scalar">Value to add</param>
        /// <returns></returns>
        T Add(float scalar);

        /// <summary>
        /// Multiplies a value to each element in this tensor
        /// </summary>
        /// <param name="scalar">Value to multiply</param>
        /// <returns></returns>
        T Multiply(float scalar);

        /// <summary>
        /// Subtracts another tensor from this tensor
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        /// <returns></returns>
        T Subtract(ITensor tensor);

        /// <summary>
        /// Subtracts another tensor from this tensor
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        /// <param name="coefficient1">Coefficient to multiply each value in this tensor</param>
        /// <param name="coefficient2">Coefficient to multiply each value in the other tensor</param>
        /// <returns></returns>
        T Subtract(ITensor tensor, float coefficient1, float coefficient2);

        /// <summary>
        /// Multiplies each value in this tensor with the corresponding value in another tensor
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        /// <returns></returns>
        T PointwiseMultiply(ITensor tensor);

        /// <summary>
        /// Divides each value in this tensor with the corresponding value in another tensor
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        /// <returns></returns>
        T PointwiseDivide(ITensor tensor);

        /// <summary>
        /// Returns the square root of each value in this tensor
        /// </summary>
        /// <returns></returns>
        T Sqrt();

        /// <summary>
        /// Reverses the order of the elements in this tensor
        /// </summary>
        /// <returns></returns>
        T Reverse();

        /// <summary>
        /// Splits this tensor into multiple contiguous tensors
        /// </summary>
        /// <param name="blockCount">Number of blocks</param>
        /// <returns></returns>
        IEnumerable<T> Split(uint blockCount);

        /// <summary>
        /// Computes the absolute value of each value in this tensor
        /// </summary>
        /// <returns></returns>
        T Abs();

        /// <summary>
        /// Computes the natural logarithm of each value in this tensor
        /// </summary>
        /// <returns></returns>
        T Log();

        /// <summary>
        /// Computes the exponent of each value in this tensor
        /// </summary>
        /// <returns></returns>
        T Exp();

        /// <summary>
        /// Computes the square of each value in this tensor
        /// </summary>
        /// <returns></returns>
        T Squared();

        /// <summary>
        /// Computes the sigmoid function of each value in this tensor
        /// </summary>
        /// <returns></returns>
        T Sigmoid();

        /// <summary>
        /// Computes the sigmoid derivative for each value in this tensor
        /// </summary>
        /// <returns></returns>
        T SigmoidDerivative();

        /// <summary>
        /// Computes the hyperbolic tangent of each value in this tensor
        /// </summary>
        /// <returns></returns>
        T Tanh();

        /// <summary>
        /// Computes the derivative of the hyperbolic tangent for each value in this tensor
        /// </summary>
        /// <returns></returns>
        T TanhDerivative();

        /// <summary>
        /// Computes the RELU activation for each value in this tensor
        /// https://en.wikipedia.org/wiki/Rectifier_(neural_networks)
        /// </summary>
        /// <returns></returns>
        T Relu();

        /// <summary>
        /// Computes the RELU derivative of each value in this tensor
        /// https://en.wikipedia.org/wiki/Rectifier_(neural_networks)
        /// </summary>
        /// <returns></returns>
        T ReluDerivative();

        /// <summary>
        /// Computes the Leaky RELU action for each value in this tensor
        /// https://en.wikipedia.org/wiki/Rectifier_(neural_networks)
        /// </summary>
        /// <returns></returns>
        T LeakyRelu();

        /// <summary>
        /// Computes the Leaky RELU derivative for each value in this tensor
        /// https://en.wikipedia.org/wiki/Rectifier_(neural_networks)
        /// </summary>
        /// <returns></returns>
        T LeakyReluDerivative();

        /// <summary>
        /// Computes the softmax of each value in this tensor
        /// </summary>
        /// <returns></returns>
        T Softmax();

        /// <summary>
        /// Computes the softmax derivative of each value in this tensor
        /// </summary>
        /// <returns></returns>
        IMatrix SoftmaxDerivative();

        /// <summary>
        /// Raises each element in this tensor by power
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        T Pow(float power);

        /// <summary>
        /// Returns a new tensor with the values specified in indices
        /// </summary>
        /// <param name="indices">Indices to return in new tensor</param>
        /// <returns></returns>
        T CherryPick(uint[] indices);

        /// <summary>
        /// Applies a mapping function to this tensor
        /// </summary>
        /// <param name="mutator">Mapping function</param>
        /// <returns></returns>
        T Map(Func<float, float> mutator);
    }

    /// <summary>
    /// Vector interface
    /// </summary>
    public interface IVector : ITensor<IVector>, IReadOnlyVector
    {
        /// <summary>
        /// Returns a value at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        new float this[int index] { get; set; }

        /// <summary>
        /// Returns a value at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        new float this[uint index] { get; set; }

        /// <summary>
        /// Returns a value at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        float this[long index] { get; set; }

        /// <summary>
        /// Returns a value at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        float this[ulong index] { get; set; }

        /// <summary>
        /// Applies a mapping function that also accepts the vector index
        /// </summary>
        /// <param name="mutator"></param>
        /// <returns></returns>
        IVector MapIndexed(Func<uint, float, float> mutator);

        /// <summary>
        /// Applies a mapping function that also accepts the vector index (vector will be modified in place)
        /// </summary>
        /// <param name="mutator"></param>
        void MapIndexedInPlace(Func<uint, float, float> mutator);

        /// <summary>
        /// Returns all values in an array
        /// </summary>
        /// <returns></returns>
        float[] ToArray();

        /// <summary>
        /// Clones the vector
        /// </summary>
        /// <returns></returns>
        new IVector Clone();
    }

    /// <summary>
    /// Matrix interface
    /// </summary>
    public interface IMatrix : ITensor<IMatrix>, IReadOnlyMatrix
    {
        /// <summary>
        /// Returns a value from the matrix
        /// </summary>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        float this[int rowY, int columnX] { get; set; }

        /// <summary>
        /// Returns a value from the matrix
        /// </summary>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        float this[uint rowY, uint columnX] { get; set; }

        /// <summary>
        /// Returns a value from the matrix
        /// </summary>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        float this[long rowY, long columnX] { get; set; }

        /// <summary>
        /// Returns a value from the matrix
        /// </summary>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        float this[ulong rowY, ulong columnX] { get; set; }

        /// <summary>
        /// Returns a row from the matrix
        /// </summary>
        /// <param name="index">Row index</param>
        /// <returns></returns>
        INumericSegment<float> Row(uint index);

        /// <summary>
        /// Returns a column from the matrix
        /// </summary>
        /// <param name="index">Column index</param>
        /// <returns></returns>
        INumericSegment<float> Column(uint index);

        /// <summary>
        /// Returns a row as a span
        /// </summary>
        /// <param name="rowY">Row index</param>
        /// <param name="temp">Temporary buffer in which to write the contiguous row values</param>
        /// <returns></returns>
        ReadOnlySpan<float> GetRowSpan(uint rowY, ref SpanOwner<float> temp);

        /// <summary>
        /// Returns a column as a span
        /// </summary>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        ReadOnlySpan<float> GetColumnSpan(uint columnX);

        /// <summary>
        /// Returns a row as a vector
        /// </summary>
        /// <param name="rowY">Row index</param>
        /// <returns></returns>
        IVector GetRowVector(uint rowY);

        /// <summary>
        /// Returns a column as a vector
        /// </summary>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        IVector GetColumnVector(uint columnX);

        /// <summary>
        /// Returns the transpose of this matrix
        /// </summary>
        /// <returns></returns>
        IMatrix Transpose();

        /// <summary>
        /// Multiply this matrix with another matrix (matrix multiplication)
        /// </summary>
        /// <param name="other">Other matrix</param>
        /// <returns></returns>
        IMatrix Multiply(IMatrix other);

        /// <summary>
        /// Transpose the other matrix and then multiply with this matrix
        /// </summary>
        /// <param name="other">Other matrix</param>
        /// <returns></returns>
        IMatrix TransposeAndMultiply(IMatrix other);

        /// <summary>
        /// Transpose this matrix and then multiply with another matrix
        /// </summary>
        /// <param name="other">Other matrix</param>
        /// <returns></returns>
        IMatrix TransposeThisAndMultiply(IMatrix other);

        /// <summary>
        /// Returns the diagonal of this matrix 
        /// </summary>
        /// <returns></returns>
        IVector GetDiagonal();

        /// <summary>
        /// Returns the sum of all rows in this matrix
        /// </summary>
        /// <returns></returns>
        IVector RowSums();

        /// <summary>
        /// Returns the sum of all columns in this matrix
        /// </summary>
        /// <returns></returns>
        IVector ColumnSums();

        /// <summary>
        /// Multiplies this matrix with a vector
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        IVector Multiply(IVector vector);

        /// <summary>
        /// Splits this matrix into two matrices from a column index
        /// </summary>
        /// <param name="columnIndex">Column index at which to split</param>
        /// <returns></returns>
        (IMatrix Left, IMatrix Right) SplitAtColumn(uint columnIndex);

        /// <summary>
        /// Splits this matrix into two matrices from a row index
        /// </summary>
        /// <param name="rowIndex">Row index at which to split</param>
        /// <returns></returns>
        (IMatrix Top, IMatrix Bottom) SplitAtRow(uint rowIndex);

        /// <summary>
        /// Concatenates this matrix with another matrix (column counts must agree)
        /// </summary>
        /// <param name="bottom"></param>
        /// <returns></returns>
        IMatrix ConcatBelow(IMatrix bottom);

        /// <summary>
        /// Concatenates this matrix with another matrix (row counts must agree)
        /// </summary>
        /// <param name="right"></param>
        /// <returns></returns>
        IMatrix ConcatRight(IMatrix right);
        
        /// <summary>
        /// Applies an indexed mapping function to this matrix
        /// </summary>
        /// <param name="mutator"></param>
        /// <returns></returns>
        IMatrix MapIndexed(Func<uint, uint, float, float> mutator);

        /// <summary>
        /// Applies an indexed mapping function to this matrix (matrix will be modified in place)
        /// </summary>
        /// <param name="mutator"></param>
        void MapIndexedInPlace(Func<uint, uint, float, float> mutator);

        /// <summary>
        /// Computes the singular value decomposition of this matrix
        /// https://en.wikipedia.org/wiki/Singular_value_decomposition
        /// </summary>
        /// <returns></returns>
        (IMatrix U, IVector S, IMatrix VT) Svd();

        /// <summary>
        /// Creates a new matrix from the specified rows of this matrix
        /// </summary>
        /// <param name="rowIndices">Row indices</param>
        /// <returns></returns>
        IMatrix GetNewMatrixFromRows(IEnumerable<uint> rowIndices);

        /// <summary>
        /// Creates a new matrix from the specified columns of this matrix
        /// </summary>
        /// <param name="columnIndices">Column indices</param>
        /// <returns></returns>
        IMatrix GetNewMatrixFromColumns(IEnumerable<uint> columnIndices);

        /// <summary>
        /// Adds a tensor segment to each row of this matrix (matrix will be modified in place)
        /// </summary>
        /// <param name="segment"></param>
        void AddToEachRow(INumericSegment<float> segment);

        /// <summary>
        /// Adds a tensor segment to each column of this matrix (matrix will be modified in place)
        /// </summary>
        /// <param name="segment"></param>
        void AddToEachColumn(INumericSegment<float> segment);

        /// <summary>
        /// Multiplies each row of this matrix with a tensor segment (matrix will be modified in place)
        /// </summary>
        /// <param name="segment"></param>
        void MultiplyEachRowWith(INumericSegment<float> segment);

        /// <summary>
        /// Multiplies each column of this matrix with a tensor segment (matrix will be modified in place)
        /// </summary>
        /// <param name="segment"></param>
        void MultiplyEachColumnWith(INumericSegment<float> segment);

        /// <summary>
        /// Computes the per row software of this matrix
        /// </summary>
        /// <returns></returns>
        INumericSegment<float>[] SoftmaxPerRow();

        /// <summary>
        /// Computes the per row softmax derivative of this matrix
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        INumericSegment<float>[] SoftmaxDerivativePerRow(INumericSegment<float>[] rows);

        /// <summary>
        /// Clones the matrix
        /// </summary>
        /// <returns></returns>
        new IMatrix Clone();

        /// <summary>
        /// Returns a row as a read only vector
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        IReadOnlyVector GetRowAsReadOnly(uint rowIndex);

        /// <summary>
        /// Returns a column as a read only vector
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        IReadOnlyVector GetColumnAsReadOnly(uint columnIndex);

        /// <summary>
        /// Returns all rows as read only vectors
        /// </summary>
        /// <param name="makeCopy">True to make a copy of each row</param>
        /// <returns></returns>
        IReadOnlyVector[] AllRowsAsReadOnly(bool makeCopy);

        /// <summary>
        /// Returns all columns as read only vectors
        /// </summary>
        /// <param name="makeCopy">True to make a copy of each column</param>
        /// <returns></returns>
        IReadOnlyVector[] AllColumnsAsReadOnly(bool makeCopy);
    }

    /// <summary>
    /// 3D tensor - a block of matrices
    /// </summary>
    public interface ITensor3D : ITensor<ITensor3D>, IReadOnlyTensor3D
    {
        /// <summary>
        /// Returns a value from this 3D tensor
        /// </summary>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        float this[int depth, int rowY, int columnX] { get; set; }

        /// <summary>
        /// Returns a value from this 3D tensor
        /// </summary>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        float this[uint depth, uint rowY, uint columnX] { get; set; }

        /// <summary>
        /// Returns a value from this 3D tensor
        /// </summary>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        float this[long depth, long rowY, long columnX] { get; set; }

        /// <summary>
        /// Returns a value from this 3D tensor
        /// </summary>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        float this[ulong depth, ulong rowY, ulong columnX] { get; set; }

        /// <summary>
        /// Returns a matrix from the tensor
        /// </summary>
        /// <param name="index">Matrix index</param>
        /// <returns></returns>
        IMatrix GetMatrix(uint index);

        /// <summary>
        /// Creates a new 3D tensor with a "padding" of zeroes around the edge of each matrix
        /// </summary>
        /// <param name="padding">Size of padding</param>
        /// <returns></returns>
        ITensor3D AddPadding(uint padding);

        /// <summary>
        /// Removes previously added "padding" from the edge of each matrix
        /// </summary>
        /// <param name="padding">Size of padding</param>
        /// <returns></returns>
        ITensor3D RemovePadding(uint padding);

        /// <summary>
        /// Image to column (convolution operator)
        /// </summary>
        /// <param name="filterWidth">Width of each filter</param>
        /// <param name="filterHeight">Height of each filter</param>
        /// <param name="xStride">Horizontal stride</param>
        /// <param name="yStride">Vertical stride</param>
        /// <returns></returns>
        IMatrix Im2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride);

        /// <summary>
        /// Reverses a previous image to column operation (convolution)
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="outputRows"></param>
        /// <param name="outputColumns"></param>
        /// <param name="outputDepth"></param>
        /// <param name="filterWidth"></param>
        /// <param name="filterHeight"></param>
        /// <param name="xStride"></param>
        /// <param name="yStride"></param>
        /// <returns></returns>
        ITensor3D ReverseIm2Col(IMatrix filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride);

        /// <summary>
        /// Computes a max pooling operation
        /// </summary>
        /// <param name="filterWidth"></param>
        /// <param name="filterHeight"></param>
        /// <param name="xStride"></param>
        /// <param name="yStride"></param>
        /// <param name="saveIndices"></param>
        /// <returns></returns>
        (ITensor3D Result, ITensor3D? Indices) MaxPool(uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices);

        /// <summary>
        /// Reverses a max pooling operation
        /// </summary>
        /// <param name="indices"></param>
        /// <param name="outputRows"></param>
        /// <param name="outputColumns"></param>
        /// <param name="filterWidth"></param>
        /// <param name="filterHeight"></param>
        /// <param name="xStride"></param>
        /// <param name="yStride"></param>
        /// <returns></returns>
        ITensor3D ReverseMaxPool(ITensor3D indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride);

        /// <summary>
        /// Adds all matrices into one new matrix
        /// </summary>
        /// <returns></returns>
        IMatrix AddAllMatrices();

        /// <summary>
        /// Multiply each matrix individually by another matrix
        /// </summary>
        /// <param name="matrix">Other matrix</param>
        /// <returns></returns>
        ITensor3D MultiplyEachMatrixBy(IMatrix matrix);
        
        /// <summary>
        /// Transpose another matrix and multiply each matrix individually by the result
        /// </summary>
        /// <param name="matrix">Other matrix</param>
        /// <returns></returns>
        ITensor3D TransposeAndMultiplyEachMatrixBy(IMatrix matrix);
        
        /// <summary>
        /// Adds a vector to each row of each matrix (tensor will be modified in place)
        /// </summary>
        /// <param name="vector"></param>
        void AddToEachRow(IVector vector);

        /// <summary>
        /// Adds a vector to each column of each matrix (tensor will be modified in place)
        /// </summary>
        /// <param name="vector"></param>
        void AddToEachColumn(IVector vector);

        /// <summary>
        /// Multiplies this matrix with a 4D tensor
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        ITensor3D Multiply(ITensor4D other);

        /// <summary>
        /// Transposes the 4D matrix and multiplies this tensor with the result
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        ITensor3D TransposeAndMultiply(ITensor4D other);

        /// <summary>
        /// Transposes this tensor and multiply the result with another 4D tensor
        /// </summary>
        /// <param name="other">Other tensor</param>
        /// <returns></returns>
        ITensor3D TransposeThisAndMultiply(ITensor4D other);

        /// <summary>
        /// Clones the tensor
        /// </summary>
        /// <returns></returns>
        new ITensor3D Clone();

        /// <summary>
        /// Returns a sub matrix
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        IReadOnlyMatrix GetMatrixAsReadOnly(uint index);

        /// <summary>
        /// Returns all sub matrices
        /// </summary>
        /// <returns></returns>
        IReadOnlyMatrix[] AllMatricesAsReadOnly();
    }

    /// <summary>
    /// 4D tensor - a block of 3D tensors
    /// </summary>
    public interface ITensor4D : ITensor<ITensor4D>, IReadOnlyTensor4D
    {
        /// <summary>
        /// Returns a value from this 4D tensor
        /// </summary>
        /// <param name="count">3D tensor index</param>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        float this[int count, int depth, int rowY, int columnX] { get; set; }

        /// <summary>
        /// Returns a value from this 4D tensor
        /// </summary>
        /// <param name="count">3D tensor index</param>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        float this[uint count, uint depth, uint rowY, uint columnX] { get; set; }

        /// <summary>
        /// Returns a value from this 4D tensor
        /// </summary>
        /// <param name="count">3D tensor index</param>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        float this[long count, long depth, long rowY, long columnX] { get; set; }

        /// <summary>
        /// Returns a value from this 4D tensor
        /// </summary>
        /// <param name="count">3D tensor index</param>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        float this[ulong count, ulong depth, ulong rowY, ulong columnX] { get; set; }

        /// <summary>
        /// Returns a 3D tensor
        /// </summary>
        /// <param name="index">3D tensor index</param>
        /// <returns></returns>
        ITensor3D GetTensor(uint index);

        /// <summary>
        /// Adds padding to each 3D tensor
        /// </summary>
        /// <param name="padding">Size of padding</param>
        /// <returns></returns>
        ITensor4D AddPadding(uint padding);

        /// <summary>
        /// Removes padding from each 3D tensor
        /// </summary>
        /// <param name="padding">Size of padding</param>
        /// <returns></returns>
        ITensor4D RemovePadding(uint padding);

        /// <summary>
        /// Max pooling operation
        /// </summary>
        /// <param name="filterWidth"></param>
        /// <param name="filterHeight"></param>
        /// <param name="xStride"></param>
        /// <param name="yStride"></param>
        /// <param name="saveIndices"></param>
        /// <returns></returns>
        (ITensor4D Result, ITensor4D? Indices) MaxPool(uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices);

        /// <summary>
        /// Reverse max pooling operation
        /// </summary>
        /// <param name="indices"></param>
        /// <param name="outputRows"></param>
        /// <param name="outputColumns"></param>
        /// <param name="filterWidth"></param>
        /// <param name="filterHeight"></param>
        /// <param name="xStride"></param>
        /// <param name="yStride"></param>
        /// <returns></returns>
        ITensor4D ReverseMaxPool(ITensor4D indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride);

        /// <summary>
        /// Image to column (convolution operator)
        /// </summary>
        /// <param name="filterWidth"></param>
        /// <param name="filterHeight"></param>
        /// <param name="xStride"></param>
        /// <param name="yStride"></param>
        /// <returns></returns>
        ITensor3D Im2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride);

        /// <summary>
        /// Reverse of image to column (convolution operator)
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="outputRows"></param>
        /// <param name="outputColumns"></param>
        /// <param name="outputDepth"></param>
        /// <param name="filterWidth"></param>
        /// <param name="filterHeight"></param>
        /// <param name="xStride"></param>
        /// <param name="yStride"></param>
        /// <returns></returns>
        ITensor4D ReverseIm2Col(IMatrix filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride);

        /// <summary>
        /// Computes the sum of all columns
        /// </summary>
        /// <returns></returns>
        IVector ColumnSums();
        
        /// <summary>
        /// Computes the sum of all rows
        /// </summary>
        /// <returns></returns>
        IVector RowSums();

        /// <summary>
        /// Clones the tensor
        /// </summary>
        /// <returns></returns>
        new ITensor4D Clone();

        /// <summary>
        /// Returns a sub tensor
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        IReadOnlyTensor3D GetTensorAsReadOnly(uint index);

        /// <summary>
        /// Returns all sub tensors
        /// </summary>
        /// <returns></returns>
        IReadOnlyTensor3D[] AllTensorsAsReadOnly();
    }
}
