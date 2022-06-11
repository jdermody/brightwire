using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BrightData.Helper;

namespace BrightData.LinearAlgebra
{
    /// <summary>
    /// Base class for tensors
    /// </summary>
    /// <typeparam name="T">Data type within the tensor</typeparam>
    /// <typeparam name="DT">Underlying type (vector, matrix etc)</typeparam>
    public abstract class TensorBase<T, DT> : ShapedBase, ITensor<T>
        where DT : ITensor<T>
        where T : struct
    {
        /// <summary>
        /// Data segment
        /// </summary>
        protected ITensorSegment<T> _segment = null!;
        Lazy<INumericComputation<T>> _computation = null!;
        bool _wasDisposed = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="segment">Data segment</param>
        /// <param name="shape">Shape of tensor</param>
        protected TensorBase(ITensorSegment<T> segment, uint[] shape) : base(shape)
        {
            Context = segment.Context;
            _computation = new Lazy<INumericComputation<T>>(() => Context.GetComputation<T>());
            Context.MemoryLayer.Add(this);
            _segment = segment;
            _segment.AddRef();
        }

        /// <summary>
        /// Initialize from binary reader
        /// </summary>
        /// <param name="context">Bright data context</param>
        /// <param name="reader">Reader to initialize from</param>
        protected TensorBase(IBrightDataContext context, BinaryReader reader) : base(Array.Empty<uint>())
        {
            Initialize(context, reader);
        }

        /// <inheritdoc />
        public void Initialize(IBrightDataContext context, BinaryReader reader)
        {
            Context = context;
            _computation = new Lazy<INumericComputation<T>>(() => Context.GetComputation<T>());
            context.MemoryLayer.Add(this);

            var size = ReadFrom(reader);

            _segment = context.CreateSegment<T>(size);
            _segment.InitializeFrom(reader.BaseStream);
            _segment.AddRef();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_wasDisposed) {
                _wasDisposed = true;
                _segment.Release();
            }
        }

        /// <inheritdoc />
        public override void WriteTo(BinaryWriter writer)
        {
            base.WriteTo(writer);
            writer.Flush();
            _segment.WriteTo(writer.BaseStream);
        }

        /// <summary>
        /// Creates a derived type
        /// </summary>
        /// <param name="segment">Data segment</param>
        /// <returns></returns>
        protected abstract DT Create(ITensorSegment<T> segment);

        /// <inheritdoc />
        public INumericComputation<T> Computation => _computation.Value;

        /// <inheritdoc />
        public ITensorSegment<T> GetDataCopy()
        {
            _segment.AddRef();
            return _segment;
        }

        /// <inheritdoc />
        public IBrightDataContext Context { get; private set; } = null!;

        /// <inheritdoc />
        public bool IsValid => _segment.IsValid;

        /// <inheritdoc />
        public T[] ToArray() => _segment.Values.ToArray();

        /// <summary>
        /// Adds this to another tensor (new tensor returned - this tensor is not changed)
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        /// <returns>New tensor</returns>
        public DT Add(DT tensor) => Create(Computation.Add(_segment, tensor.Segment));

        /// <summary>
        /// Adds this to another tensor (new tensor returned - this tensor is not changed)
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        /// <param name="coefficient1">Value to multiply each element of this tensor</param>
        /// <param name="coefficient2">Value to multiply each element of the other tensor</param>
        /// <returns>New tensor</returns>
        public DT Add(DT tensor, T coefficient1, T coefficient2) => Create(Computation.Add(_segment, tensor.Segment, coefficient1, coefficient2));

        /// <summary>
        /// Adds a scalar to this tensor (new tensor returned - this tensor is not changed)
        /// </summary>
        /// <param name="scalar">Value to add to each element</param>
        /// <returns>New tensor</returns>
        public DT Add(T scalar) => Create(Computation.Add(_segment, scalar));

        /// <summary>
        /// Adds another tensor to this in place (this tensor is changed)
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        public void AddInPlace(DT tensor) => Computation.AddInPlace(_segment, tensor.Segment);

        /// <summary>
        /// Adds another tensor to this in place (this tensor is changed)
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        /// <param name="coefficient1">Value to multiply each element of this tensor</param>
        /// <param name="coefficient2">Value to multiply each element of the other tensor</param>
        public void AddInPlace(DT tensor, T coefficient1, T coefficient2) => Computation.AddInPlace(_segment, tensor.Segment, coefficient1, coefficient2);

        /// <summary>
        /// Adds a scalar to this in place (this tensor is changed)
        /// </summary>
        /// <param name="scalar">Value to add to each element</param>
        public void AddInPlace(T scalar) => Computation.AddInPlace(_segment, scalar);

        /// <summary>
        /// Subtracts another tensor from this (new tensor returned - this tensor is not changed)
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        /// <returns></returns>
        public DT Subtract(DT tensor) => Create(Computation.Subtract(_segment, tensor.Segment));

        /// <summary>
        /// Subtracts another tensor from this (new tensor returned - this tensor is not changed)
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        /// <param name="coefficient1">Value to multiply each element of this tensor</param>
        /// <param name="coefficient2">Value to multiply each element of the other tensor</param>
        /// <returns></returns>
        public DT Subtract(DT tensor, T coefficient1, T coefficient2) => Create(Computation.Subtract(_segment, tensor.Segment, coefficient1, coefficient2));

        /// <summary>
        /// Subtracts another tensor from this in place (this tensor is changed)
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        public void SubtractInPlace(DT tensor) => Computation.SubtractInPlace(_segment, tensor.Segment);

        /// <summary>
        /// Subtracts another tensor from this in place (this tensor is changed)
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        /// <param name="coefficient1">Value to multiply each element of this tensor</param>
        /// <param name="coefficient2">Value to multiply each element of the other tensor</param>
        public void SubtractInPlace(DT tensor, T coefficient1, T coefficient2) => Computation.SubtractInPlace(_segment, tensor.Segment, coefficient1, coefficient2);

        /// <summary>
        /// Multiplies each element with the corresponding element from the other tensor (new tensor returned - this tensor is not changed)
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        /// <returns></returns>
        public DT PointwiseMultiply(DT tensor) => Create(Computation.PointwiseMultiply(_segment, tensor.Segment));

        /// <summary>
        /// Multiplies each element with the corresponding element from the other tensor (this tensor is changed)
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        public void PointwiseMultiplyInPlace(DT tensor) => Computation.SubtractInPlace(_segment, tensor.Segment);

        /// <summary>
        /// Multiplies each element with a value  (new tensor returned - this tensor is not changed)
        /// </summary>
        /// <param name="scalar">Value to multiply</param>
        /// <returns></returns>
        public DT Multiply(T scalar) => Create(Computation.Multiply(_segment, scalar));

        /// <summary>
        /// Multiplies each element with a value (this tensor is changed)
        /// </summary>
        /// <param name="scalar">Value to multiply</param>
        public void MultiplyInPlace(T scalar) => Computation.MultiplyInPlace(_segment, scalar);

        /// <summary>
        /// Divides each element with the corresponding element from the other tensor (new tensor returned - this tensor is not changed)
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        /// <returns></returns>
        public DT PointwiseDivide(DT tensor) => Create(Computation.PointwiseDivide(_segment, tensor.Segment));

        /// <summary>
        /// Divides each element with the corresponding element from the other tensor (this tensor is changed)
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        public void PointwiseDivideInPlace(DT tensor) => Computation.PointwiseDivideInPlace(_segment, tensor.Segment);

        /// <summary>
        /// Creates a new tensor with each value set to the log of each element in this tensor
        /// </summary>
        /// <returns></returns>
        public DT Log() => Create(Computation.Log(_segment));

        /// <summary>
        /// Creates a new tensor with each value set to the absolute value of each element in this tensor
        /// </summary>
        /// <returns></returns>
        public DT Abs() => Create(Computation.Abs(_segment));

        /// <summary>
        /// Creates a new tensor with each value set to the square root of value of each element in this tensor
        /// </summary>
        /// <returns></returns>
        public DT Sqrt() => Create(Computation.Sqrt(_segment));

        /// <summary>
        /// Creates a new tensor with each value set to the squared value of each element in this tensor
        /// </summary>
        /// <returns></returns>
        public DT Squared() => Create(Computation.Squared(_segment));

        /// <summary>
        /// Creates a new tensor with each value set to pow(n) of element in this tensor
        /// </summary>
        /// <param name="power">Power</param>
        /// <returns></returns>
        public DT Pow(T power) => Create(Computation.Pow(_segment, power));

        /// <summary>
        /// Computes the dot product of this tensor and another tensor
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        /// <returns></returns>
        public T DotProduct(DT tensor) => Computation.DotProduct(_segment, tensor.Segment);

        /// <summary>
        /// Sums all elements of this tensor
        /// </summary>
        /// <returns></returns>
        public T Sum() => Computation.Sum(_segment);

        /// <summary>
        /// Finds the index of an element of this tensor
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public uint? Search(T value) => Computation.Search(_segment, value);

        /// <summary>
        /// Mutates this tensor so that any value outside the range will be modified to fit within
        /// </summary>
        /// <param name="minValue">Minimum allowed value</param>
        /// <param name="maxValue">Maximum allowed value</param>
        public void ConstrainInPlace(T? minValue, T? maxValue) => Computation.ConstrainInPlace(_segment, minValue, maxValue);

        /// <summary>
        /// Finds the average of all elements within this tensor
        /// </summary>
        /// <returns></returns>
        public T Average() => Computation.Average(_segment);

        /// <summary>
        /// Computes the L1 norm of this tensor
        /// </summary>
        /// <returns></returns>
        public T L1Norm() => Computation.L1Norm(_segment);

        /// <summary>
        /// Computes the L2 norm of this tensor
        /// </summary>
        /// <returns></returns>
        public T L2Norm() => Computation.L2Norm(_segment);

        /// <summary>
        /// Calculates the cosine distance between this and another tensor
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        /// <returns></returns>
        public T CosineDistance(DT tensor) => Computation.CosineDistance(_segment, tensor.Segment);

        /// <summary>
        /// Computes the euclidean distance between this and another tensor
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        /// <returns></returns>
        public T EuclideanDistance(DT tensor) => Computation.EuclideanDistance(_segment, tensor.Segment);

        /// <summary>
        /// Computes the manhattan distance between this and another tensor
        /// </summary>
        /// <param name="tensor">Other tensor</param>
        /// <returns></returns>
        public T ManhattanDistance(DT tensor) => Computation.ManhattanDistance(_segment, tensor.Segment);

        /// <summary>
        /// Computes the mean squared distance between this and another tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public T MeanSquaredDistance(DT tensor) => Computation.MeanSquaredDistance(_segment, tensor.Segment);

        /// <summary>
        /// Computes the squared euclidean distance between this and another tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public T SquaredEuclideanDistance(DT tensor) => Computation.SquaredEuclideanDistance(_segment, tensor.Segment);

        /// <summary>
        /// Computes the average of the elements in this tensor
        /// </summary>
        /// <returns></returns>
        public T Mean() => Computation.Average(_segment);

        /// <summary>
        /// Computes the standard deviation of the elements in this tensor
        /// </summary>
        /// <param name="mean"></param>
        /// <returns></returns>
        public T StdDev(T? mean) => Computation.StdDev(_segment, mean);

        /// <summary>
        /// Computes the softmax
        /// </summary>
        /// <returns></returns>
        public DT Softmax() => Create(Computation.Softmax(_segment));

        /// <summary>
        /// Computes the softmax derivative
        /// </summary>
        /// <returns></returns>
        public Matrix<T> SoftmaxDerivative() => Computation.SoftmaxDerivative(_segment);

        /// <summary>
        /// Computes the sigmoid function of each element
        /// </summary>
        /// <returns></returns>
        public DT Sigmoid() => Create(Computation.Sigmoid(_segment));

        /// <summary>
        /// Computes the sigmoid derivative of each element
        /// </summary>
        /// <returns></returns>
        public DT SigmoidDerivative() => Create(Computation.SigmoidDerivative(_segment));

        /// <summary>
        /// Computes tanh of each element in this tensor
        /// </summary>
        /// <returns></returns>
        public DT Tanh() => Create(Computation.Tanh(_segment));

        /// <summary>
        /// Computes tanh derivative of each element in this tensor
        /// </summary>
        /// <returns></returns>
        public DT TanhDerivative() => Create(Computation.TanhDerivative(_segment));

        /// <summary>
        /// Computes the relu of each element in this tensor
        /// </summary>
        /// <returns></returns>
        public DT Relu() => Create(Computation.Relu(_segment));

        /// <summary>
        /// Computes the relu derivative of each element in this tensor
        /// </summary>
        /// <returns></returns>
        public DT ReluDerivative() => Create(Computation.ReluDerivative(_segment));

        /// <summary>
        /// Computes the leaky relu of each element in this tensor
        /// </summary>
        /// <returns></returns>
        public DT LeakyRelu() => Create(Computation.LeakyRelu(_segment));

        /// <summary>
        /// Computes the leaky relu derivative of each element in this tensor
        /// </summary>
        /// <returns></returns>
        public DT LeakyReluDerivative() => Create(Computation.LeakyReluDerivative(_segment));

        /// <summary>
        /// Finds the min and max values and indices from this tensnor
        /// </summary>
        /// <returns></returns>
        public (T Min, T Max, uint MinIndex, uint MaxIndex) GetMinAndMaxValues() => Computation.GetMinAndMaxValues(_segment);

        /// <summary>
        /// Finds the min and max absolute values and indices from this tensnor
        /// </summary>
        /// <returns></returns>
        public (T Min, T Max, uint MinIndex, uint MaxIndex) GetMinAndMaxAbsoluteValues()
        {
            using var temp = Abs();
            return Computation.GetMinAndMaxValues(temp.Segment);
        }

        /// <summary>
        /// Checks if each element is entirely finite (not infinity, NaN etc)
        /// </summary>
        /// <returns>True if all elements are finite</returns>
        public bool IsEntirelyFinite() => Computation.IsEntirelyFinite(_segment);

        /// <summary>
        /// Reverse the order of the elements in this tensor
        /// </summary>
        /// <returns></returns>
        public DT Reverse() => Create(Computation.Reverse(_segment));

        /// <summary>
        /// Splits this tensor into segments
        /// </summary>
        /// <param name="blockCount">Number of blocks to split into</param>
        /// <returns></returns>
        public List<ITensorSegment<T>> Split(uint blockCount) => Computation.Split(_segment, blockCount);

        /// <inheritdoc />
        public ITensorSegment<T> Segment => _segment;

        /// <summary>
        /// Clones this tensor
        /// </summary>
        /// <returns></returns>
        public DT Clone() => Create(Context.CreateSegment(_segment.Clone()));

        /// <summary>
        /// Reshapes to a vector
        /// </summary>
        /// <returns></returns>
        public Vector<T> Reshape() => new(GetDataCopy());

        /// <summary>
        /// Reshapes to a matrix
        /// </summary>
        /// <param name="rows">Number of rows</param>
        /// <param name="columns">Number of columns</param>
        /// <returns></returns>
        public Matrix<T> Reshape(uint? rows, uint? columns)
        {
            var shape = ResolveShape(_segment.Size, new[] { rows, columns });
            return new Matrix<T>(GetDataCopy(), shape[0], shape[1]);
        }

        /// <summary>
        /// Reshapes to a 3D tensor
        /// </summary>
        /// <param name="depth">Number of matrices</param>
        /// <param name="rows">Number of rows</param>
        /// <param name="columns">Number of columns</param>
        /// <returns></returns>
        public Tensor3D<T> Reshape(uint? depth, uint? rows, uint? columns)
        {
            var shape = ResolveShape(_segment.Size, new[] { depth, rows, columns });
            return new Tensor3D<T>(GetDataCopy(), shape[0], shape[1], shape[2]);
        }

        /// <summary>
        /// Reshapes to a 4D tensor
        /// </summary>
        /// <param name="count">Number of 3D tensors</param>
        /// <param name="depth">Number of matrices</param>
        /// <param name="rows">Number of rows</param>
        /// <param name="columns">Number of columns</param>
        /// <returns></returns>
        public Tensor4D<T> Reshape(uint? count, uint? depth, uint? rows, uint? columns)
        {
            var shape = ResolveShape(_segment.Size, new[] { count, depth, rows, columns });
            return new Tensor4D<T>(GetDataCopy(), shape[0], shape[1], shape[2], shape[3]);
        }

        /// <summary>
        /// Returns the index with the highest value
        /// </summary>
        public uint MaximumIndex() => GetMinAndMaxValues().MaxIndex;

        /// <summary>
        /// Returns the index with the lowest value
        /// </summary>
        /// <returns></returns>
        public uint MinimumIndex() => GetMinAndMaxValues().MinIndex;

        /// <summary>
        /// Returns the index with the highest absolute value
        /// </summary>
        public uint MaximumAbsoluteIndex() => GetMinAndMaxAbsoluteValues().MaxIndex;

        /// <summary>
        /// Returns the index with the lowest absolute value
        /// </summary>
        /// <returns></returns>
        public uint MinimumAbsoluteIndex() => GetMinAndMaxAbsoluteValues().MinIndex;

        /// <summary>
        /// Rounds each value to either upper (if >= mid) or lower
        /// </summary>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        /// <param name="mid"></param>
        public void RoundInPlace(T? lower, T? upper, T? mid) => Computation.RoundInPlace(_segment, lower ?? Computation.One, upper ?? Computation.Zero, mid);

        /// <summary>
        /// Parallel application of mapping function to each element of the tensor
        /// </summary>
        /// <param name="mapper">Mapping function to apply</param>
        /// <returns></returns>
        protected ITensorSegment<T> MapParallel(Func<uint, T, T> mapper)
        {
            var ret = Context.CreateSegment<T>(Size);
            // ReSharper disable once AccessToDisposedClosure
            Parallel.For(0, (int) Size, i => ret[i] = mapper((uint) i, _segment[i]));
            return ret;
        }

        /// <summary>
        /// Sets each element to zero
        /// </summary>
        public void Clear() => Segment.Initialize(Computation.Zero);
    }
}
