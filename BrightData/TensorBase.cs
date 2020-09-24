using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BrightData
{
    public abstract class TensorBase<T, DT> : ITensor<T>
        where DT : ITensor<T>
        where T : struct
    {
        protected ITensorSegment<T> _segment;
        Lazy<INumericComputation<T>> _computation;
        bool _wasDisposed = false;

        TensorBase(IBrightDataContext context)
        {
            Context = context;
            _computation = new Lazy<INumericComputation<T>>(() => Context.GetComputation<T>());
            context.MemoryLayer.Add(this);
        }

        protected TensorBase(ITensorSegment<T> segment, uint[] shape) : this(segment.Context)
        {
            Shape = shape;
            _segment = segment;
        }

        public void Initialize(IBrightDataContext context, BinaryReader reader)
        {
            Context = context;
            _computation = new Lazy<INumericComputation<T>>(() => Context.GetComputation<T>());
            context.MemoryLayer.Add(this);

            var len = reader.ReadInt32();
            Shape = new uint[len];
            uint size = 0;
            for (var i = 0; i < len; i++) {
                var item = reader.ReadUInt32();
                size = i == 0 ? item : size * item;
                Shape[i] = item;
            }

            _segment = context.CreateSegment<T>(size);
            _segment.InitializeFrom(reader.BaseStream);
        }

        public void Dispose()
        {
            if (!_wasDisposed) {
                lock (this) {
                    if (_wasDisposed) {
                        _wasDisposed = true;
                        _segment.Release();
                    }
                }
            }
        }

        protected TensorBase(IBrightDataContext context, BinaryReader reader)
        {
            Initialize(context, reader);
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(Shape.Length);
            foreach (var item in Shape)
                writer.Write(item);
            writer.Flush();
            _segment.WriteTo(writer.BaseStream);
        }

        protected abstract DT Create(ITensorSegment<T> segment);

        public uint[] Shape { get; private set; }
        public INumericComputation<T> Computation => _computation.Value;

        public ITensorSegment<T> GetDataCopy()
        {
            _segment.AddRef();
            return _segment;
        }
        public IBrightDataContext Context { get; private set; }
        public uint Size => this.GetSize();
        public uint Rank => this.GetRank();

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

        public DT Clone()
        {
            var (block, isNewCopy) = _segment.GetBlock(Context.TensorPool);
            return Create(Context.CreateSegment(isNewCopy ? block : (T[])block.Clone()));
        }

        static uint[] _ResolveShape(uint total, uint?[] shape)
        {
            uint nonNullTotal = 0;
            bool hasFoundNull = false;
            foreach (var item in shape) {
                if (item.HasValue)
                    nonNullTotal += item.Value;
                else if (!hasFoundNull)
                    hasFoundNull = true;
                else
                    throw new ArgumentException("Only one parameter can be null");
            }

            if (hasFoundNull && nonNullTotal == 0)
                throw new ArgumentException("Cannot resolve null parameter");

            return shape.Select(v => v ?? total / nonNullTotal).ToArray();
        }

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
        /// Finds the index with the highest value
        /// </summary>
        public uint MaximumIndex() => GetMinAndMaxValues().MaxIndex;

        public uint MinimumIndex() => GetMinAndMaxValues().MinIndex;
    }
}
