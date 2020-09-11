using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BrightData
{
    public abstract class TensorBase<T, DT> : ITensor<T>
        where DT : ITensor<T>
        where T : struct
    {
        protected ITensorSegment<T> _data;
        Lazy<INumericComputation<T>> _computation;
        bool _wasDisposed = false;

        TensorBase(IBrightDataContext context)
        {
            Context = context;
            _computation = new Lazy<INumericComputation<T>>(() => Context.GetComputation<T>());
            context.MemoryLayer.Add(this);
        }

        protected TensorBase(ITensorSegment<T> data, uint[] shape) : this(data.Context)
        {
            Shape = shape;
            _data = data;
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

            var data = context.TensorPool.Get<T>(size);
            data.InitializeFrom(reader.BaseStream);
            _data = data.GetSegment();
        }

        public void Dispose()
        {
            if (!_wasDisposed) {
                lock (this) {
                    if (_wasDisposed) {
                        _wasDisposed = true;
                        _data.Release();
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
            _data.WriteTo(writer.BaseStream);
        }

        protected abstract DT Create(ITensorSegment<T> segment);

        public uint[] Shape { get; private set; }
        public INumericComputation<T> Computation => _computation.Value;

        public ITensorSegment<T> GetDataCopy()
        {
            _data.AddRef();
            return _data;
        }
        public IBrightDataContext Context { get; private set; }
        public uint Size => this.GetSize();
        public uint Rank => this.GetRank();

        public T[] ToArray() => _data.ToArray();
        public void InitializeFrom(Stream stream) => _data.InitializeFrom(stream);

        public DT Add(DT tensor) => Create(Computation.Add(_data, tensor.Data));
        public void AddInPlace(DT tensor) => Computation.AddInPlace(_data, tensor.Data);
        public void AddInPlace(DT tensor, T coefficient1, T coefficient2) => Computation.AddInPlace(_data, tensor.Data, coefficient1, coefficient2);
        public void AddInPlace(T scalar) => Computation.AddInPlace(_data, scalar);
        public DT Subtract(DT tensor) => Create(Computation.Subtract(_data, tensor.Data));
        public void SubtractInPlace(DT tensor) => Computation.SubtractInPlace(_data, tensor.Data);
        public void SubtractInPlace(DT tensor, T coefficient1, T coefficient2) => Computation.SubtractInPlace(_data, tensor.Data, coefficient1, coefficient2);
        public DT PointwiseMultiply(DT tensor) => Create(Computation.PointwiseMultiply(_data, tensor.Data));
        public void MultiplyInPlace(T scalar) => Computation.MultiplyInPlace(_data, scalar);
        public void PointwiseMultiplyInPlace(DT tensor) => Computation.SubtractInPlace(_data, tensor.Data);
        public DT PointwiseDivide(DT tensor) => Create(Computation.PointwiseDivide(_data, tensor.Data));
        public void PointwiseDivideInPlace(DT tensor) => Computation.PointwiseDivideInPlace(_data, tensor.Data);
        public DT Multiply(T scalar) => Create(Computation.Multiply(_data, scalar));

        public DT Log() => Create(Computation.Log(_data));
        public DT Abs() => Create(Computation.Abs(_data));
        public DT Sqrt() => Create(Computation.Sqrt(_data));
        public DT Squared() => Create(Computation.Squared(_data));
        public DT Pow(T power) => Create(Computation.Pow(_data, power));

        public T DotProduct(DT tensor) => Computation.DotProduct(_data, tensor.Data);
        public T Sum() => Computation.Sum(_data);
        public uint? Search(T value) => Computation.Search(_data, value);
        public void ConstrainInPlace(T? minValue, T? maxValue) => Computation.ConstrainInPlace(_data, minValue, maxValue);
        public T Average() => Computation.Average(_data);
        public T L1Norm() => Computation.L1Norm(_data);
        public T L2Norm() => Computation.L2Norm(_data);

        public T CosineDistance(DT tensor) => Computation.CosineDistance(_data, tensor.Data);
        public T EuclideanDistance(DT tensor) => Computation.EuclideanDistance(_data, tensor.Data);
        public T ManhattanDistance(DT tensor) => Computation.ManhattanDistance(_data, tensor.Data);

        public T Mean() => Computation.Average(_data);
        public T StdDev(T? mean) => Computation.StdDev(_data, mean);

        public DT Softmax() => Create(Computation.Softmax(_data));
        public Matrix<T> SoftmaxDerivative() => Computation.SoftmaxDerivative(_data);
        public DT Sigmoid() => Create(Computation.Sigmoid(_data));
        public DT SigmoidDerivative() => Create(Computation.SigmoidDerivative(_data));
        public DT Tanh() => Create(Computation.Tanh(_data));
        public DT TanhDerivative() => Create(Computation.TanhDerivative(_data));
        public DT Relu() => Create(Computation.Relu(_data));
        public DT ReluDerivative() => Create(Computation.ReluDerivative(_data));
        public DT LeakyRelu() => Create(Computation.LeakyRelu(_data));
        public DT LeakyReluDerivative() => Create(Computation.LeakyReluDerivative(_data));

        public (T Min, T Max, uint MinIndex, uint MaxIndex) GetMinAndMaxValues() => Computation.GetMinAndMaxValues(_data);
        public bool IsEntirelyFinite() => Computation.IsEntirelyFinite(_data);
        public DT Reverse() => Create(Computation.Reverse(_data));
        public List<ITensorSegment<T>> Split(int blockCount) => Computation.Split(_data, blockCount);
        public ITensorSegment<T> Data => _data;

        public DT Clone()
        {
            var (block, isNewCopy) = _data.GetBlock(Context.TensorPool);
            return Create((isNewCopy ? block : block.Clone()).GetSegment());
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
            var shape = _ResolveShape(_data.Size, new[] { rows, columns });
            return new Matrix<T>(GetDataCopy(), shape[0], shape[1]);
        }

        public Tensor3D<T> Reshape(uint? depth, uint? rows, uint? columns)
        {
            var shape = _ResolveShape(_data.Size, new[] { depth, rows, columns });
            return new Tensor3D<T>(GetDataCopy(), shape[0], shape[1], shape[2]);
        }

        public Tensor4D<T> Reshape(uint? count, uint? depth, uint? rows, uint? columns)
        {
            var shape = _ResolveShape(_data.Size, new[] { count, depth, rows, columns });
            return new Tensor4D<T>(GetDataCopy(), shape[0], shape[1], shape[2], shape[3]);
        }

        /// <summary>
        /// Finds the index with the highest value
        /// </summary>
        public uint MaximumIndex() => GetMinAndMaxValues().MaxIndex;
    }
}
