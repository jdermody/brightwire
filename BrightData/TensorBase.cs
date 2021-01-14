using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightData.Helper;

namespace BrightData
{
    public abstract class TensorBase<T, DT> : ShapedBase, ITensor<T>
        where DT : ITensor<T>
        where T : struct
    {
        protected ITensorSegment<T> _segment;
        Lazy<INumericComputation<T>> _computation;
        bool _wasDisposed = false;

        TensorBase(IBrightDataContext context, uint[] shape) : base(shape)
        {
            Context = context;
            _computation = new Lazy<INumericComputation<T>>(() => Context.GetComputation<T>());
            context.MemoryLayer.Add(this);
        }

        protected TensorBase(ITensorSegment<T> segment, uint[] shape) : this(segment.Context, shape)
        {
            _segment = segment;
            _segment.AddRef();
        }

        protected TensorBase(IBrightDataContext context, BinaryReader reader) : base(null)
        {
            Initialize(context, reader);
        }

        public void Initialize(IBrightDataContext context, BinaryReader reader)
        {
            Context = context;
            _computation = new Lazy<INumericComputation<T>>(() => Context.GetComputation<T>());
            context.MemoryLayer.Add(this);

            var size = ReadFrom(reader);

            _segment = context.CreateSegment<T>(size);
            _segment.InitializeFrom(reader.BaseStream);
        }

        public void Dispose()
        {
            if (!_wasDisposed) {
                _wasDisposed = true;
                _segment.Release();
            }
        }

        public void WriteTo(BinaryWriter writer)
        {
            base.WriteTo(writer);
            writer.Flush();
            _segment.WriteTo(writer.BaseStream);
        }

        protected abstract DT Create(ITensorSegment<T> segment);

        
        public INumericComputation<T> Computation => _computation.Value;

        public ITensorSegment<T> GetDataCopy()
        {
            _segment.AddRef();
            return _segment;
        }
        public IBrightDataContext Context { get; private set; }

        public T[] ToArray() => _segment.ToArray();
        public void InitializeFrom(Stream stream) => _segment.InitializeFrom(stream);

        public DT Add(DT tensor) => Create(Computation.Add(_segment, tensor.Segment));
        public void AddInPlace(DT tensor) => Computation.AddInPlace(_segment, tensor.Segment);
        public void AddInPlace(DT tensor, T coefficient1, T coefficient2) => Computation.AddInPlace(_segment, tensor.Segment, coefficient1, coefficient2);
        public void AddInPlace(T scalar) => Computation.AddInPlace(_segment, scalar);
        public DT Subtract(DT tensor) => Create(Computation.Subtract(_segment, tensor.Segment));
        public void SubtractInPlace(DT tensor) => Computation.SubtractInPlace(_segment, tensor.Segment);
        public void SubtractInPlace(DT tensor, T coefficient1, T coefficient2) => Computation.SubtractInPlace(_segment, tensor.Segment, coefficient1, coefficient2);
        public DT PointwiseMultiply(DT tensor) => Create(Computation.PointwiseMultiply(_segment, tensor.Segment));
        public void MultiplyInPlace(T scalar) => Computation.MultiplyInPlace(_segment, scalar);
        public void PointwiseMultiplyInPlace(DT tensor) => Computation.SubtractInPlace(_segment, tensor.Segment);
        public DT PointwiseDivide(DT tensor) => Create(Computation.PointwiseDivide(_segment, tensor.Segment));
        public void PointwiseDivideInPlace(DT tensor) => Computation.PointwiseDivideInPlace(_segment, tensor.Segment);
        public DT Multiply(T scalar) => Create(Computation.Multiply(_segment, scalar));

        public DT Log() => Create(Computation.Log(_segment));
        public DT Abs() => Create(Computation.Abs(_segment));
        public DT Sqrt() => Create(Computation.Sqrt(_segment));
        public DT Squared() => Create(Computation.Squared(_segment));
        public DT Pow(T power) => Create(Computation.Pow(_segment, power));

        public T DotProduct(DT tensor) => Computation.DotProduct(_segment, tensor.Segment);
        public T Sum() => Computation.Sum(_segment);
        public uint? Search(T value) => Computation.Search(_segment, value);
        public void ConstrainInPlace(T? minValue, T? maxValue) => Computation.ConstrainInPlace(_segment, minValue, maxValue);
        public T Average() => Computation.Average(_segment);
        public T L1Norm() => Computation.L1Norm(_segment);
        public T L2Norm() => Computation.L2Norm(_segment);

        public T CosineDistance(DT tensor) => Computation.CosineDistance(_segment, tensor.Segment);
        public T EuclideanDistance(DT tensor) => Computation.EuclideanDistance(_segment, tensor.Segment);
        public T ManhattanDistance(DT tensor) => Computation.ManhattanDistance(_segment, tensor.Segment);

        public T Mean() => Computation.Average(_segment);
        public T StdDev(T? mean) => Computation.StdDev(_segment, mean);

        public DT Softmax() => Create(Computation.Softmax(_segment));
        public Matrix<T> SoftmaxDerivative() => Computation.SoftmaxDerivative(_segment);
        public DT Sigmoid() => Create(Computation.Sigmoid(_segment));
        public DT SigmoidDerivative() => Create(Computation.SigmoidDerivative(_segment));
        public DT Tanh() => Create(Computation.Tanh(_segment));
        public DT TanhDerivative() => Create(Computation.TanhDerivative(_segment));
        public DT Relu() => Create(Computation.Relu(_segment));
        public DT ReluDerivative() => Create(Computation.ReluDerivative(_segment));
        public DT LeakyRelu() => Create(Computation.LeakyRelu(_segment));
        public DT LeakyReluDerivative() => Create(Computation.LeakyReluDerivative(_segment));

        public (T Min, T Max, uint MinIndex, uint MaxIndex) GetMinAndMaxValues() => Computation.GetMinAndMaxValues(_segment);
        public bool IsEntirelyFinite() => Computation.IsEntirelyFinite(_segment);
        public DT Reverse() => Create(Computation.Reverse(_segment));
        public List<ITensorSegment<T>> Split(uint blockCount) => Computation.Split(_segment, blockCount);
        public ITensorSegment<T> Segment => _segment;

        public DT Clone() => Create(_segment.IsContiguous
            ? _segment
            : Context.CreateSegment(_segment.ToArray())
        );

        public Vector<T> Reshape() => new Vector<T>(GetDataCopy());

        public Matrix<T> Reshape(uint? rows, uint? columns)
        {
            var shape = _ResolveShape(_segment.Size, new[] { rows, columns });
            return new Matrix<T>(GetDataCopy(), shape[0], shape[1]);
        }

        public Tensor3D<T> Reshape(uint? depth, uint? rows, uint? columns)
        {
            var shape = _ResolveShape(_segment.Size, new[] { depth, rows, columns });
            return new Tensor3D<T>(GetDataCopy(), shape[0], shape[1], shape[2]);
        }

        public Tensor4D<T> Reshape(uint? count, uint? depth, uint? rows, uint? columns)
        {
            var shape = _ResolveShape(_segment.Size, new[] { count, depth, rows, columns });
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
    }
}
