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
    public interface IHaveLinearAlgebraProvider<T> where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        /// <summary>
        /// Linear algebra provider
        /// </summary>
        LinearAlgebraProvider<T> LinearAlgebraProvider { get; }
    }

    /// <summary>
    /// Indicates that the type can set a linear algebra provider
    /// </summary>
    public interface ISetLinearAlgebraProvider<T> where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        /// <summary>
        /// Property to set the linear algebra provider
        /// </summary>
        LinearAlgebraProvider<T> LinearAlgebraProvider { set; }

        /// <summary>
        /// Linear algebra provider factory
        /// </summary>
        Func<LinearAlgebraProvider<T>> LinearAlgebraProviderFactory { set; }
    }

    /// <summary>
    /// Distance metrics
    /// </summary>
    public enum DistanceMetric
    {
        /// <summary>
        /// Euclidean Distance - https://en.wikipedia.org/wiki/Euclidean_distance
        /// </summary>
        Euclidean,

        /// <summary>
        /// Cosine Distance Metric - https://en.wikipedia.org/wiki/Cosine_similarity
        /// </summary>
        Cosine,

        /// <summary>
        /// Manhattan Distance - https://en.wikipedia.org/wiki/Taxicab_geometry
        /// </summary>
        Manhattan,

        /// <summary>
        /// Means Square Error
        /// </summary>
        MeanSquared,

        /// <summary>
        /// Square Euclidean - https://en.wikipedia.org/wiki/Euclidean_distance#Squared_Euclidean_distance
        /// </summary>
        SquaredEuclidean,

        /// <summary>
        /// Angular distance - https://en.wikipedia.org/wiki/Angular_distance
        /// </summary>
        Angular,

        /// <summary>
        /// Inner product distance - https://en.wikipedia.org/wiki/Inner_product_space
        /// </summary>
        InnerProductSpace
    }

    /// <summary>
    /// A read only segment of numeric values - might be contiguous or a wrapper around a contiguous block
    /// </summary>
    public interface IReadOnlyNumericSegment<T> : ICountReferences, IDisposable, IHaveSize, IHaveSpanOf<T>
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
        IHaveReadOnlyContiguousMemory<T>? Contiguous { get; }

        /// <summary>
        /// True if the segment wraps another segment
        /// </summary>
        bool IsWrapper { get; }
    }

    /// <summary>
    /// An editable segment of numeric values
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
    /// Indicates that the type has contiguous read only memory
    /// </summary>
    public interface IHaveReadOnlyContiguousMemory<T>
    {
        /// <summary>
        /// A read only span
        /// </summary>
        ReadOnlySpan<T> ReadOnlySpan { get; }

        /// <summary>
        /// Read only contiguous memory
        /// </summary>
        ReadOnlyMemory<T> ContiguousMemory { get; }
    }

    /// <summary>
    /// Read only tensor
    /// </summary>
    public interface IReadOnlyTensor : IHaveSize, IAmSerializable
    {
        /// <summary>
        /// Checks if the tensor is entirely finite (does not contain NAN or Infinity)
        /// </summary>
        /// <returns></returns>
        bool IsEntirelyFinite();
    }

    /// <summary>
    /// Typed read only tensor
    /// </summary>
    public interface IReadOnlyTensor<T> : IReadOnlyTensor, IHaveSpanOf<T>, IHaveReadOnlyTensorSegment<T> 
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        /// <summary>
        /// Returns all values in an array
        /// </summary>
        /// <returns></returns>
        T[] ToArray();

        /// <summary>
        /// Returns the min and max values and their indices
        /// </summary>
        /// <returns></returns>
        (T Min, T Max, uint MinIndex, uint MaxIndex) GetMinAndMaxValues();

        /// <summary>
        /// Sums all values of this tensor
        /// </summary>
        /// <returns></returns>
        T Sum();

        /// <summary>
        /// Finds the average value of this tensor
        /// </summary>
        /// <returns></returns>
        T Average();

        /// <summary>
        /// Returns the L1 norm of this tensor (manhattan distance)
        /// </summary>
        /// <returns></returns>
        T L1Norm();

        /// <summary>
        /// Returns the L2 norm of this tensor (euclidean norm)
        /// </summary>
        /// <returns></returns>
        T L2Norm();

        /// <summary>
        /// Calculates the standard deviation of this tensor
        /// </summary>
        /// <param name="mean">Existing mean of tensor if available (otherwise it will be calculated)</param>
        /// <returns></returns>
        T StdDev(T? mean);

        /// <summary>
        /// Finds cosine distance (0 for perpendicular, 1 for orthogonal, 2 for opposite) between this and another tensor
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        T CosineDistance(IReadOnlyTensor<T> other);

        /// <summary>
        /// Finds the euclidean distance between this and another tensor
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        T EuclideanDistance(IReadOnlyTensor<T> other);

        /// <summary>
        /// Finds the manhattan distance between this and another tensor
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        T ManhattanDistance(IReadOnlyTensor<T> other);

        /// <summary>
        /// Finds the mean squared distance between this and another tensor
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        T MeanSquaredDistance(IReadOnlyTensor<T> other);

        /// <summary>
        /// Finds the squared euclidean distance between this and another tensor
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        T SquaredEuclideanDistance(IReadOnlyTensor<T> other);

        /// <summary>
        /// Finds the distance between this and another tensor
        /// </summary>
        /// <param name="other"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        T FindDistance(IReadOnlyTensor<T> other, DistanceMetric distance);

        /// <summary>
        /// Computes the dot product of this tensor with another tensor (of same length)
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        /// <returns></returns>
        T DotProduct(ITensor<T> tensor);

        /// <summary>
        /// Enumerates the values in the tensor
        /// </summary>
        IEnumerable<T> Values { get; }
    }

    /// <summary>
    /// Read only tensor type (vector, matrix etc.)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TT"></typeparam>
    public interface IReadOnlyTensorType<T, out TT> : IReadOnlyTensor<T>
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
        where TT : IReadOnlyTensor<T>
    {
        /// <summary>
        /// Adds a tensor to this tensor and returns the result
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        TT Add(IReadOnlyTensor<T> other);

        /// <summary>
        /// Adds a tensor to this tensor and returns the result with each value multiplied by coefficients
        /// </summary>
        /// <param name="other"></param>
        /// <param name="coefficient1"></param>
        /// <param name="coefficient2"></param>
        /// <returns></returns>
        TT Add(IReadOnlyTensor<T> other, T coefficient1, T coefficient2);

        /// <summary>
        /// Adds a value to each element of the tensor
        /// </summary>
        /// <param name="scalar"></param>
        /// <returns></returns>
        TT Add(T scalar);

        /// <summary>
        /// Multiplies a scalar to each element of the tensor
        /// </summary>
        /// <param name="scalar"></param>
        /// <returns></returns>
        TT Multiply(T scalar);

        /// <summary>
        /// Subtracts another tensor from this tensor and returns the result
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        TT Subtract(IReadOnlyTensor<T> other);

        /// <summary>
        /// Subtracts another tensor from this tensor and returns the result with each value multiplied by coefficients
        /// </summary>
        /// <param name="other"></param>
        /// <param name="coefficient1"></param>
        /// <param name="coefficient2"></param>
        /// <returns></returns>
        TT Subtract(IReadOnlyTensor<T> other, T coefficient1, T coefficient2);

        /// <summary>
        /// Multiplies each element of this tensor with the corresponding value in another tensor with the same size
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        TT PointwiseMultiply(IReadOnlyTensor<T> other);

        /// <summary>
        /// Divides each element of this tensor with the corresponding value in another tensor with the same size
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        TT PointwiseDivide(IReadOnlyTensor<T> other);

        /// <summary>
        /// Returns the square root of each value in this tensor
        /// </summary>
        /// <returns></returns>
        TT Sqrt(T? adjustment = null);

        /// <summary>
        /// Reverses the order of the elements in this tensor
        /// </summary>
        /// <returns></returns>
        TT Reverse();

        /// <summary>
        /// Computes the absolute value of each value in this tensor
        /// </summary>
        /// <returns></returns>
        TT Abs();

        /// <summary>
        /// Computes the natural logarithm of each value in this tensor
        /// </summary>
        /// <returns></returns>
        TT Log();

        /// <summary>
        /// Computes the exponent of each value in this tensor
        /// </summary>
        /// <returns></returns>
        TT Exp();

        /// <summary>
        /// Raises each element in this tensor by power
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        TT Pow(T power);

        /// <summary>
        /// Computes the square of each value in this tensor
        /// </summary>
        /// <returns></returns>
        TT Squared();

        /// <summary>
        /// Computes the sigmoid function of each value in this tensor
        /// </summary>
        /// <returns></returns>
        TT Sigmoid();

        /// <summary>
        /// Computes the sigmoid derivative for each value in this tensor
        /// </summary>
        /// <returns></returns>
        TT SigmoidDerivative();

        /// <summary>
        /// Computes the hyperbolic tangent of each value in this tensor
        /// </summary>
        /// <returns></returns>
        TT Tanh();

        /// <summary>
        /// Computes the derivative of the hyperbolic tangent for each value in this tensor
        /// </summary>
        /// <returns></returns>
        TT TanhDerivative();

        /// <summary>
        /// Computes the RELU activation for each value in this tensor
        /// https://en.wikipedia.org/wiki/Rectifier_(neural_networks)
        /// </summary>
        /// <returns></returns>
        TT Relu();

        /// <summary>
        /// Computes the RELU derivative of each value in this tensor
        /// https://en.wikipedia.org/wiki/Rectifier_(neural_networks)
        /// </summary>
        /// <returns></returns>
        TT ReluDerivative();

        /// <summary>
        /// Computes the Leaky RELU action for each value in this tensor
        /// https://en.wikipedia.org/wiki/Rectifier_(neural_networks)
        /// </summary>
        /// <returns></returns>
        TT LeakyRelu();

        /// <summary>
        /// Computes the Leaky RELU derivative for each value in this tensor
        /// https://en.wikipedia.org/wiki/Rectifier_(neural_networks)
        /// </summary>
        /// <returns></returns>
        TT LeakyReluDerivative();

        /// <summary>
        /// Computes the softmax of each value in this tensor
        /// </summary>
        /// <returns></returns>
        TT Softmax();

        /// <summary>
        /// Computes the softmax derivative of each value in this tensor
        /// </summary>
        /// <returns></returns>
        TT SoftmaxDerivative(int rowCount);

        /// <summary>
        /// Returns a new tensor with the values specified in indices
        /// </summary>
        /// <param name="indices">Indices to return in new tensor</param>
        /// <returns></returns>
        TT CherryPick(params uint[] indices);

        /// <summary>
        /// Applies a mapping function to this tensor
        /// </summary>
        /// <param name="mutator">Mapping function</param>
        /// <returns></returns>
        TT Map(Func<T, T> mutator);

        /// <summary>
        /// Applies a mapping function that also accepts the index of the value
        /// </summary>
        /// <param name="mutator"></param>
        /// <returns></returns>
        TT MapIndexed(Func<uint, T, T> mutator);
    }

    /// <summary>
    /// Vector that cannot be modified
    /// </summary>
    public interface IReadOnlyVector<T> : IReadOnlyTensorType<T, IReadOnlyVector<T>>
        where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
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
        /// Creates a new mutable vector that is a copy of this vector
        /// </summary>
        /// <param name="lap">Linear algebra provider</param>
        /// <returns></returns>
        IVector<T> Create(LinearAlgebraProvider<T> lap);
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
    public interface IReadOnlyMatrix<T> : IReadOnlyTensorType<T, IReadOnlyMatrix<T>>, IHaveMatrixDimensions 
        where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        /// <summary>
        /// Returns a value from the matrix
        /// </summary>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        T this[int rowY, int columnX] { get; }

        /// <summary>
        /// Returns a value from the matrix
        /// </summary>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        T this[uint rowY, uint columnX] { get; }

        /// <summary>
        /// Returns a row from the matrix
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        IReadOnlyNumericSegment<T> GetReadOnlyRow(uint rowIndex);

        /// <summary>
        /// Returns a column from the matrix
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        IReadOnlyNumericSegment<T> GetReadOnlyColumn(uint columnIndex);

        /// <summary>
        /// Returns a row as a span
        /// </summary>
        /// <param name="rowY">Row index</param>
        /// <param name="temp">Temporary buffer in which to write the contiguous row values</param>
        /// <returns></returns>
        ReadOnlySpan<T> GetRowSpan(uint rowY, ref SpanOwner<T> temp);

        /// <summary>
        /// Returns a column as a span
        /// </summary>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        ReadOnlySpan<T> GetColumnSpan(uint columnX);

        /// <summary>
        /// Creates a new mutable matrix that is a copy of this matrix
        /// </summary>
        /// <param name="lap">Linear algebra provider</param>
        /// <returns></returns>
        IMatrix<T> Create(LinearAlgebraProvider<T> lap);

        /// <summary>
        /// Returns the transpose of this matrix
        /// </summary>
        /// <returns></returns>
        IReadOnlyMatrix<T> Transpose();
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
    public interface IReadOnlyTensor3D<T> : IReadOnlyTensorType<T, IReadOnlyTensor3D<T>>, IHaveTensor3DDimensions 
        where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        /// <summary>
        /// Returns a value from the 3D tensor
        /// </summary>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        T this[int depth, int rowY, int columnX] { get; }

        /// <summary>
        /// Returns a value from the 3D tensor
        /// </summary>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        T this[uint depth, uint rowY, uint columnX] { get; }

        /// <summary>
        /// Returns a matrix from the 3D tensor
        /// </summary>
        /// <param name="index">Matrix index</param>
        /// <returns></returns>
        IReadOnlyMatrix<T> GetMatrix(uint index);

        /// <summary>
        /// Creates a new mutable tensor that is a copy of this tensor
        /// </summary>
        /// <param name="lap">Linear algebra provider</param>
        /// <returns></returns>
        ITensor3D<T> Create(LinearAlgebraProvider<T> lap);
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
    public interface IReadOnlyTensor4D<T> : IReadOnlyTensorType<T, IReadOnlyTensor4D<T>>, IHaveTensor4DDimensions 
        where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        /// <summary>
        /// Returns a value from the 4D tensor
        /// </summary>
        /// <param name="count">3D tensor index</param>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        T this[int count, int depth, int rowY, int columnX] { get; }

        /// <summary>
        /// Returns a value from the 4D tensor
        /// </summary>
        /// <param name="count">3D tensor index</param>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        T this[uint count, uint depth, uint rowY, uint columnX] { get; }

        /// <summary>
        /// Returns a 3D tensor from the 4D tensor
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        IReadOnlyTensor3D<T> GetTensor(uint index);

        /// <summary>
        /// Creates a new mutable tensor that is a copy of this tensor
        /// </summary>
        /// <param name="lap">Linear algebra provider</param>
        /// <returns></returns>
        ITensor4D<T> Create(LinearAlgebraProvider<T> lap);
    }

    /// <summary>
    /// Mutable tensor
    /// </summary>
    public interface ITensor : IDisposable, IHaveBrightDataContext
    {
        /// <summary>
        /// Sets all values to zero
        /// </summary>
        void Clear();

        /// <summary>
        /// Total count of all values
        /// </summary>
        uint TotalSize { get; }

        /// <summary>
        /// Tensor shape - for a vector the array will have a single element, for a matrix it will be [columns, rows], a 3D tensor will be [columns, rows, depth] etc.
        /// </summary>
        uint[] Shape { get; }
    }

    /// <summary>
    /// Typed tensor interface - vector, matrix, 3D tensor etc.
    /// </summary>
    public interface ITensor<T> : ITensor, IReadOnlyTensor<T>, IHaveLinearAlgebraProvider<T>, IHaveTensorSegment<T>
        where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        /// <summary>
        /// Reshapes to a vector
        /// </summary>
        /// <returns></returns>
        IVector<T> Reshape();

        /// <summary>
        /// Reshapes to a matrix
        /// </summary>
        /// <param name="rows">Row count of each matrix (one parameter is optional null)</param>
        /// <param name="columns">Column count of each matrix (one parameter is optional null)</param>
        /// <returns></returns>
        IMatrix<T> Reshape(uint? rows, uint? columns);

        /// <summary>
        /// Reshapes to a 3D tensor
        /// </summary>
        /// <param name="depth">Number of matrices (one parameter is optional null)</param>
        /// <param name="rows">Number of rows in each matrix (one parameter is optional null)</param>
        /// <param name="columns">Number of columns in each matrix (one parameter is optional null)</param>
        /// <returns></returns>
        ITensor3D<T> Reshape(uint? depth, uint? rows, uint? columns);

        /// <summary>
        /// Reshapes to a 4D tensor
        /// </summary>
        /// <param name="count">Number of 3D tensors (one parameter is optional null)</param>
        /// <param name="depth">Number of matrices in each 3D tensor (one parameter is optional null)</param>
        /// <param name="rows">Number of rows in each matrix (one parameter is optional null)</param>
        /// <param name="columns">Number of columns in each matrix (one parameter is optional null)</param>
        /// <returns></returns>
        ITensor4D<T> Reshape(uint? count, uint? depth, uint? rows, uint? columns);

        /// <summary>
        /// Creates a copy of this tensor
        /// </summary>
        /// <returns></returns>
        ITensor<T> Clone();

        /// <summary>
        /// Adds a tensor to this tensor (the result will be stored in this tensor)
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        void AddInPlace(ITensor<T> tensor);

        /// <summary>
        /// Adds a tensor to this tensor (the result will be stored in this tensor)
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        /// <param name="coefficient1">Value to multiply each element of this tensor</param>
        /// <param name="coefficient2">Value to multiply each element of the other tensor</param>
        void AddInPlace(ITensor<T> tensor, T coefficient1, T coefficient2);

        /// <summary>
        /// Adds a value to this tensor (the result will be stored in this tensor)
        /// </summary>
        /// <param name="scalar">Value to add</param>
        void AddInPlace(T scalar);

        /// <summary>
        /// Multiplies a value to this tensor (the result will be stored in this tensor)
        /// </summary>
        /// <param name="scalar">Value to multiply</param>
        void MultiplyInPlace(T scalar);

        /// <summary>
        /// Subtracts a tensor from this tensor (the result will be stored in this tensor)
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        void SubtractInPlace(ITensor<T> tensor);

        /// <summary>
        /// Subtracts a tensor from this tensor (the result will be stored in this tensor)
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        /// <param name="coefficient1">Value to multiply each element of this tensor</param>
        /// <param name="coefficient2">Value to multiply each element of the other tensor</param>
        void SubtractInPlace(ITensor<T> tensor, T coefficient1, T coefficient2);

        /// <summary>
        /// Multiplies each value in this tensor with the corresponding value in the other tensor (the result will be stored in this tensor)
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        void PointwiseMultiplyInPlace(ITensor<T> tensor);

        /// <summary>
        /// Divides each value in this tensor with the corresponding value in the other tensor (the result will be stored in this tensor)
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        void PointwiseDivideInPlace(ITensor<T> tensor);

        /// <summary>
        /// Modifies this tensor so that no value is less than or greater than supplied parameters
        /// </summary>
        /// <param name="minValue">Minimum value to allow (optional)</param>
        /// <param name="maxValue">Maximum value to allow (optional)</param>
        void ConstrainInPlace(T? minValue, T? maxValue);

        /// <summary>
        /// Rounds each value in this tensor to either the lower or upper parameter (the result will be stored in this tensor)
        /// </summary>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        void RoundInPlace(T lower, T upper);

        /// <summary>
        /// Applies a mapping function to each value of this tensor (the result will be stored in this tensor)
        /// </summary>
        /// <param name="mutator"></param>
        void MapInPlace(Func<T, T> mutator);

        /// <summary>
        /// Applies a mapping function that also accepts the vector index (vector will be modified in place)
        /// </summary>
        /// <param name="mutator"></param>
        void MapIndexedInPlace(Func<uint, T, T> mutator);

        /// <summary>
        /// Applies L1 regularization to this tensor (the result will be stored in this tensor)
        /// </summary>
        /// <param name="coefficient"></param>
        void L1RegularisationInPlace(T coefficient);
    }

    /// <summary>
    /// Typed tensor interface - vector, matrix, 3D tensor etc.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TT"></typeparam>
    /// <typeparam name="RTT"></typeparam>
    public interface ITensorType<T, out RTT, out TT> : ITensor<T>, IReadOnlyTensorType<T, RTT>
        where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
        where RTT: IReadOnlyTensor<T>
        where TT: ITensor<T>
    {
        /// <summary>
        /// Creates a clone of this tensor
        /// </summary>
        /// <returns></returns>
        new TT Clone();

        /// <summary>
        /// Adds a tensor to this tensor
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        /// <returns></returns>
        TT Add(ITensor<T> tensor);

        /// <summary>
        /// Adds a tensor to this tensor
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        /// <param name="coefficient1">Coefficient to multiply each value in this tensor</param>
        /// <param name="coefficient2">Coefficient to multiply each value in the other tensor</param>
        /// <returns></returns>
        TT Add(ITensor<T> tensor, T coefficient1, T coefficient2);

        /// <summary>
        /// Adds a value to each element in this tensor
        /// </summary>
        /// <param name="scalar">Value to add</param>
        /// <returns></returns>
        new TT Add(T scalar);

        /// <summary>
        /// Multiplies a value to each element in this tensor
        /// </summary>
        /// <param name="scalar">Value to multiply</param>
        /// <returns></returns>
        new TT Multiply(T scalar);

        /// <summary>
        /// Subtracts another tensor from this tensor
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        /// <returns></returns>
        TT Subtract(ITensor<T> tensor);

        /// <summary>
        /// Subtracts another tensor from this tensor
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        /// <param name="coefficient1">Coefficient to multiply each value in this tensor</param>
        /// <param name="coefficient2">Coefficient to multiply each value in the other tensor</param>
        /// <returns></returns>
        TT Subtract(ITensor<T> tensor, T coefficient1, T coefficient2);

        /// <summary>
        /// Multiplies each value in this tensor with the corresponding value in another tensor
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        /// <returns></returns>
        TT PointwiseMultiply(ITensor<T> tensor);

        /// <summary>
        /// Divides each value in this tensor with the corresponding value in another tensor
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        /// <returns></returns>
        TT PointwiseDivide(ITensor<T> tensor);

        /// <summary>
        /// Returns the square root of each value in this tensor
        /// </summary>
        /// <returns></returns>
        TT Sqrt();

        /// <summary>
        /// Reverses the order of the elements in this tensor
        /// </summary>
        /// <returns></returns>
        new TT Reverse();

        /// <summary>
        /// Splits this tensor into multiple contiguous tensors
        /// </summary>
        /// <param name="blockCount">Number of blocks</param>
        /// <returns></returns>
        IEnumerable<TT> Split(uint blockCount);

        /// <summary>
        /// Computes the absolute value of each value in this tensor
        /// </summary>
        /// <returns></returns>
        new TT Abs();

        /// <summary>
        /// Computes the natural logarithm of each value in this tensor
        /// </summary>
        /// <returns></returns>
        new TT Log();

        /// <summary>
        /// Computes the exponent of each value in this tensor
        /// </summary>
        /// <returns></returns>
        new TT Exp();

        /// <summary>
        /// Computes the square of each value in this tensor
        /// </summary>
        /// <returns></returns>
        new TT Squared();

        /// <summary>
        /// Computes the sigmoid function of each value in this tensor
        /// </summary>
        /// <returns></returns>
        new TT Sigmoid();

        /// <summary>
        /// Computes the sigmoid derivative for each value in this tensor
        /// </summary>
        /// <returns></returns>
        new TT SigmoidDerivative();

        /// <summary>
        /// Computes the hyperbolic tangent of each value in this tensor
        /// </summary>
        /// <returns></returns>
        new TT Tanh();

        /// <summary>
        /// Computes the derivative of the hyperbolic tangent for each value in this tensor
        /// </summary>
        /// <returns></returns>
        new TT TanhDerivative();

        /// <summary>
        /// Computes the RELU activation for each value in this tensor
        /// https://en.wikipedia.org/wiki/Rectifier_(neural_networks)
        /// </summary>
        /// <returns></returns>
        new TT Relu();

        /// <summary>
        /// Computes the RELU derivative of each value in this tensor
        /// https://en.wikipedia.org/wiki/Rectifier_(neural_networks)
        /// </summary>
        /// <returns></returns>
        new TT ReluDerivative();

        /// <summary>
        /// Computes the Leaky RELU action for each value in this tensor
        /// https://en.wikipedia.org/wiki/Rectifier_(neural_networks)
        /// </summary>
        /// <returns></returns>
        new TT LeakyRelu();

        /// <summary>
        /// Computes the Leaky RELU derivative for each value in this tensor
        /// https://en.wikipedia.org/wiki/Rectifier_(neural_networks)
        /// </summary>
        /// <returns></returns>
        new TT LeakyReluDerivative();

        /// <summary>
        /// Computes the softmax of each value in this tensor
        /// </summary>
        /// <returns></returns>
        new TT Softmax();

        /// <summary>
        /// Computes the softmax derivative of each value in this tensor
        /// </summary>
        /// <returns></returns>
        IMatrix<T> SoftmaxDerivative();

        /// <summary>
        /// Raises each element in this tensor by power
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        new TT Pow(T power);

        /// <summary>
        /// Returns a new tensor with the values specified in indices
        /// </summary>
        /// <param name="indices">Indices to return in new tensor</param>
        /// <returns></returns>
        new TT CherryPick(params uint[] indices);

        /// <summary>
        /// Applies a mapping function to this tensor
        /// </summary>
        /// <param name="mutator">Mapping function</param>
        /// <returns></returns>
        new TT Map(Func<T, T> mutator);

        /// <summary>
        /// Applies a mapping function that also accepts the index of the value
        /// </summary>
        /// <param name="mutator"></param>
        /// <returns></returns>
        new TT MapIndexed(Func<uint, T, T> mutator);
    }

    /// <summary>
    /// Vector interface
    /// </summary>
    public interface IVector<T> : ITensorType<T, IReadOnlyVector<T>, IVector<T>>, IReadOnlyVector<T>
        where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        /// <summary>
        /// Returns a value at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        new T this[int index] { get; set; }

        /// <summary>
        /// Returns a value at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        new T this[uint index] { get; set; }

        /// <summary>
        /// Returns a value at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        T this[long index] { get; set; }

        /// <summary>
        /// Returns a value at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        T this[ulong index] { get; set; }

        /// <summary>
        /// Clones the vector
        /// </summary>
        /// <returns></returns>
        new IVector<T> Clone();
    }

    /// <summary>
    /// Matrix interface
    /// </summary>
    public interface IMatrix<T> : ITensorType<T, IReadOnlyMatrix<T>, IMatrix<T>>, IReadOnlyMatrix<T>
        where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        /// <summary>
        /// Returns a value from the matrix
        /// </summary>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        new T this[int rowY, int columnX] { get; set; }

        /// <summary>
        /// Returns a value from the matrix
        /// </summary>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        new T this[uint rowY, uint columnX] { get; set; }

        /// <summary>
        /// Returns a value from the matrix
        /// </summary>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        T this[long rowY, long columnX] { get; set; }

        /// <summary>
        /// Returns a value from the matrix
        /// </summary>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        T this[ulong rowY, ulong columnX] { get; set; }

        /// <summary>
        /// Returns a row from the matrix
        /// </summary>
        /// <param name="index">Row index</param>
        /// <returns></returns>
        INumericSegment<T> GetRow(uint index);

        /// <summary>
        /// Returns a column from the matrix
        /// </summary>
        /// <param name="index">Column index</param>
        /// <returns></returns>
        INumericSegment<T> GetColumn(uint index);

        /// <summary>
        /// Returns the transpose of this matrix
        /// </summary>
        /// <returns></returns>
        new IMatrix<T> Transpose();

        /// <summary>
        /// Multiply this matrix with another matrix (matrix multiplication)
        /// </summary>
        /// <param name="other">Other matrix</param>
        /// <returns></returns>
        IMatrix<T> Multiply(IMatrix<T> other);

        /// <summary>
        /// Transpose the other matrix and then multiply with this matrix
        /// </summary>
        /// <param name="other">Other matrix</param>
        /// <returns></returns>
        IMatrix<T> TransposeAndMultiply(IMatrix<T> other);

        /// <summary>
        /// Transpose this matrix and then multiply with another matrix
        /// </summary>
        /// <param name="other">Other matrix</param>
        /// <returns></returns>
        IMatrix<T> TransposeThisAndMultiply(IMatrix<T> other);

        /// <summary>
        /// Returns the diagonal of this matrix 
        /// </summary>
        /// <returns></returns>
        IVector<T> GetDiagonal();

        /// <summary>
        /// Returns the sum of all rows in this matrix
        /// </summary>
        /// <returns></returns>
        IVector<T> RowSums();

        /// <summary>
        /// Returns the sum of all columns in this matrix
        /// </summary>
        /// <returns></returns>
        IVector<T> ColumnSums();

        /// <summary>
        /// Multiplies this matrix with a vector
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        IVector<T> Multiply(IVector<T> vector);

        /// <summary>
        /// Splits this matrix into two matrices from a column index
        /// </summary>
        /// <param name="columnIndex">Column index at which to split</param>
        /// <returns></returns>
        (IMatrix<T> Left, IMatrix<T> Right) SplitAtColumn(uint columnIndex);

        /// <summary>
        /// Splits this matrix into two matrices from a row index
        /// </summary>
        /// <param name="rowIndex">Row index at which to split</param>
        /// <returns></returns>
        (IMatrix<T> Top, IMatrix<T> Bottom) SplitAtRow(uint rowIndex);

        /// <summary>
        /// Concatenates this matrix with another matrix (column counts must agree)
        /// </summary>
        /// <param name="bottom"></param>
        /// <returns></returns>
        IMatrix<T> ConcatBelow(IMatrix<T> bottom);

        /// <summary>
        /// Concatenates this matrix with another matrix (row counts must agree)
        /// </summary>
        /// <param name="right"></param>
        /// <returns></returns>
        IMatrix<T> ConcatRight(IMatrix<T> right);

        /// <summary>
        /// Applies an indexed mapping function to this matrix (matrix will be modified in place)
        /// </summary>
        /// <param name="mutator"></param>
        void MapIndexedInPlace(Func<uint, uint, T, T> mutator);

        /// <summary>
        /// Computes the singular value decomposition of this matrix
        /// https://en.wikipedia.org/wiki/Singular_value_decomposition
        /// </summary>
        /// <returns></returns>
        (IMatrix<T> U, IVector<T> S, IMatrix<T> VT) Svd();

        /// <summary>
        /// Creates a new matrix from the specified rows of this matrix
        /// </summary>
        /// <param name="rowIndices">Row indices</param>
        /// <returns></returns>
        IMatrix<T> GetNewMatrixFromRows(IEnumerable<uint> rowIndices);

        /// <summary>
        /// Creates a new matrix from the specified columns of this matrix
        /// </summary>
        /// <param name="columnIndices">Column indices</param>
        /// <returns></returns>
        IMatrix<T> GetNewMatrixFromColumns(IEnumerable<uint> columnIndices);

        /// <summary>
        /// Adds a tensor segment to each row of this matrix (matrix will be modified in place)
        /// </summary>
        /// <param name="segment"></param>
        void AddToEachRow(IReadOnlyNumericSegment<T> segment);

        /// <summary>
        /// Adds a tensor segment to each column of this matrix (matrix will be modified in place)
        /// </summary>
        /// <param name="segment"></param>
        void AddToEachColumn(IReadOnlyNumericSegment<T> segment);

        /// <summary>
        /// Multiplies each row of this matrix with a tensor segment (matrix will be modified in place)
        /// </summary>
        /// <param name="segment"></param>
        void MultiplyEachRowWith(IReadOnlyNumericSegment<T> segment);

        /// <summary>
        /// Multiplies each column of this matrix with a tensor segment (matrix will be modified in place)
        /// </summary>
        /// <param name="segment"></param>
        void MultiplyEachColumnWith(IReadOnlyNumericSegment<T> segment);

        /// <summary>
        /// Computes the per row software of this matrix
        /// </summary>
        /// <returns></returns>
        INumericSegment<T>[] SoftmaxPerRow();

        /// <summary>
        /// Computes the per row softmax derivative of this matrix
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        INumericSegment<T>[] SoftmaxDerivativePerRow(IReadOnlyNumericSegment<T>[] rows);

        /// <summary>
        /// Clones the matrix
        /// </summary>
        /// <returns></returns>
        new IMatrix<T> Clone();
    }

    /// <summary>
    /// 3D tensor - a block of matrices
    /// </summary>
    public interface ITensor3D<T> : ITensorType<T, IReadOnlyTensor3D<T>, ITensor3D<T>>, IReadOnlyTensor3D<T>
        where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        /// <summary>
        /// Returns a value from this 3D tensor
        /// </summary>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        new T this[int depth, int rowY, int columnX] { get; set; }

        /// <summary>
        /// Returns a value from this 3D tensor
        /// </summary>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        new T this[uint depth, uint rowY, uint columnX] { get; set; }

        /// <summary>
        /// Returns a value from this 3D tensor
        /// </summary>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        T this[long depth, long rowY, long columnX] { get; set; }

        /// <summary>
        /// Returns a value from this 3D tensor
        /// </summary>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        T this[ulong depth, ulong rowY, ulong columnX] { get; set; }

        /// <summary>
        /// Returns a matrix from the tensor
        /// </summary>
        /// <param name="index">Matrix index</param>
        /// <returns></returns>
        new IMatrix<T> GetMatrix(uint index);

        /// <summary>
        /// Creates a new 3D tensor with a "padding" of zeroes around the edge of each matrix
        /// </summary>
        /// <param name="padding">Size of padding</param>
        /// <returns></returns>
        ITensor3D<T> AddPadding(uint padding);

        /// <summary>
        /// Removes previously added "padding" from the edge of each matrix
        /// </summary>
        /// <param name="padding">Size of padding</param>
        /// <returns></returns>
        ITensor3D<T> RemovePadding(uint padding);

        /// <summary>
        /// Image to column (convolution operator)
        /// </summary>
        /// <param name="filterWidth">Width of each filter</param>
        /// <param name="filterHeight">Height of each filter</param>
        /// <param name="xStride">Horizontal stride</param>
        /// <param name="yStride">Vertical stride</param>
        /// <returns></returns>
        IMatrix<T> Im2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride);

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
        ITensor3D<T> ReverseIm2Col(IMatrix <T>filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride);

        /// <summary>
        /// Computes a max pooling operation
        /// </summary>
        /// <param name="filterWidth"></param>
        /// <param name="filterHeight"></param>
        /// <param name="xStride"></param>
        /// <param name="yStride"></param>
        /// <param name="saveIndices"></param>
        /// <returns></returns>
        (ITensor3D<T> Result, ITensor3D<T>? Indices) MaxPool(uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices);

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
        ITensor3D<T> ReverseMaxPool(ITensor3D<T> indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride);

        /// <summary>
        /// Adds all matrices into one new matrix
        /// </summary>
        /// <returns></returns>
        IMatrix<T> AddAllMatrices();

        /// <summary>
        /// Multiply each matrix individually by another matrix
        /// </summary>
        /// <param name="matrix">Other matrix</param>
        /// <returns></returns>
        ITensor3D<T> MultiplyEachMatrixBy(IMatrix<T> matrix);
        
        /// <summary>
        /// Transpose another matrix and multiply each matrix individually by the result
        /// </summary>
        /// <param name="matrix">Other matrix</param>
        /// <returns></returns>
        ITensor3D<T> TransposeAndMultiplyEachMatrixBy(IMatrix<T> matrix);
        
        /// <summary>
        /// Adds a vector to each row of each matrix (tensor will be modified in place)
        /// </summary>
        /// <param name="vector"></param>
        void AddToEachRow(IVector<T> vector);

        /// <summary>
        /// Adds a vector to each column of each matrix (tensor will be modified in place)
        /// </summary>
        /// <param name="vector"></param>
        void AddToEachColumn(IVector<T> vector);

        /// <summary>
        /// Multiplies this matrix with a 4D tensor
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        ITensor3D<T> Multiply(ITensor4D<T> other);

        /// <summary>
        /// Transposes the 4D matrix and multiplies this tensor with the result
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        ITensor3D<T> TransposeAndMultiply(ITensor4D<T> other);

        /// <summary>
        /// Transposes this tensor and multiply the result with another 4D tensor
        /// </summary>
        /// <param name="other">Other tensor</param>
        /// <returns></returns>
        ITensor3D<T> TransposeThisAndMultiply(ITensor4D<T> other);

        /// <summary>
        /// Clones the tensor
        /// </summary>
        /// <returns></returns>
        new ITensor3D<T> Clone();
    }

    /// <summary>
    /// 4D tensor - a block of 3D tensors
    /// </summary>
    public interface ITensor4D<T> : ITensorType<T, IReadOnlyTensor4D<T>, ITensor4D<T>>, IReadOnlyTensor4D<T>
        where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        /// <summary>
        /// Returns a value from this 4D tensor
        /// </summary>
        /// <param name="count">3D tensor index</param>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        new T this[int count, int depth, int rowY, int columnX] { get; set; }

        /// <summary>
        /// Returns a value from this 4D tensor
        /// </summary>
        /// <param name="count">3D tensor index</param>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        new T this[uint count, uint depth, uint rowY, uint columnX] { get; set; }

        /// <summary>
        /// Returns a value from this 4D tensor
        /// </summary>
        /// <param name="count">3D tensor index</param>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        T this[long count, long depth, long rowY, long columnX] { get; set; }

        /// <summary>
        /// Returns a value from this 4D tensor
        /// </summary>
        /// <param name="count">3D tensor index</param>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        T this[ulong count, ulong depth, ulong rowY, ulong columnX] { get; set; }

        /// <summary>
        /// Returns a 3D tensor
        /// </summary>
        /// <param name="index">3D tensor index</param>
        /// <returns></returns>
        new ITensor3D<T> GetTensor(uint index);

        /// <summary>
        /// Adds padding to each 3D tensor
        /// </summary>
        /// <param name="padding">Size of padding</param>
        /// <returns></returns>
        ITensor4D<T> AddPadding(uint padding);

        /// <summary>
        /// Removes padding from each 3D tensor
        /// </summary>
        /// <param name="padding">Size of padding</param>
        /// <returns></returns>
        ITensor4D<T> RemovePadding(uint padding);

        /// <summary>
        /// Max pooling operation
        /// </summary>
        /// <param name="filterWidth"></param>
        /// <param name="filterHeight"></param>
        /// <param name="xStride"></param>
        /// <param name="yStride"></param>
        /// <param name="saveIndices"></param>
        /// <returns></returns>
        (ITensor4D<T> Result, ITensor4D<T>? Indices) MaxPool(uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices);

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
        ITensor4D<T> ReverseMaxPool(ITensor4D<T> indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride);

        /// <summary>
        /// Image to column (convolution operator)
        /// </summary>
        /// <param name="filterWidth"></param>
        /// <param name="filterHeight"></param>
        /// <param name="xStride"></param>
        /// <param name="yStride"></param>
        /// <returns></returns>
        ITensor3D<T> Im2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride);

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
        ITensor4D<T> ReverseIm2Col(IMatrix<T> filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride);

        /// <summary>
        /// Computes the sum of all columns
        /// </summary>
        /// <returns></returns>
        IVector<T> ColumnSums();

        /// <summary>
        /// Clones the tensor
        /// </summary>
        /// <returns></returns>
        new ITensor4D<T> Clone();
    }

    /// <summary>
    /// Cost functions measure the difference between predicted vs expected outputs
    /// </summary>
    public interface ICostFunction<T> where T: unmanaged, INumber<T>
    {
        /// <summary>
        /// Calculates the cost between the predicated vs the expected values
        /// </summary>
        /// <param name="predicted"></param>
        /// <param name="expected"></param>
        /// <returns></returns>
        T Cost(IReadOnlyNumericSegment<T> predicted, IReadOnlyNumericSegment<T> expected);

        /// <summary>
        /// Calculates the gradient of the cost function between the predicted vs expected values
        /// </summary>
        /// <param name="predicted"></param>
        /// <param name="expected"></param>
        /// <returns></returns>
        IReadOnlyNumericSegment<T> Gradient(IReadOnlyNumericSegment<T> predicted, IReadOnlyNumericSegment<T> expected);
    }

}
