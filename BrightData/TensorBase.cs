using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace BrightData
{
    public abstract class TensorBase<T, DT> : ITensor<T>, IHaveTensorSegment<T>, ICanWriteToBinaryWriter
        where DT: ITensor<T>, IHaveTensorSegment<T>
        where T: struct
    {
        protected readonly ITensorSegment<T> _data;
        readonly Lazy<INumericComputation<T>> _computation;
        bool _wasDisposed = false;

        TensorBase(IBrightDataContext context)
        {
            Context = context;
            _computation = new Lazy<INumericComputation<T>>(() => Context.GetComputation<T>());
            context.MemoryLayer.Add(this);
        }

        protected TensorBase(IBrightDataContext context, ITensorSegment<T> data, uint[] shape) : this(context)
        {
            Shape = shape;
            _data = data;
            //_data.AddRef();
        }

        public void Dispose()
        {
            if (!_wasDisposed) {
                _wasDisposed = true;
                _data.Release();
            }
        }

        protected TensorBase(IBrightDataContext context, BinaryReader reader) : this(context)
        {
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

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(Shape.Length);
            foreach (var item in Shape)
                writer.Write(item);
            writer.Flush();
            _data.WriteTo(writer.BaseStream);
        }

        protected abstract DT Create(ITensorSegment<T> segment);

        public uint[] Shape { get; }
        protected INumericComputation<T> Computation => _computation.Value;

        public ITensorSegment<T> GetData()
        {
            _data.AddRef();
            return _data;
        }
        public IBrightDataContext Context { get; }

        public T[] ToArray() => _data.ToArray();
        public void InitializeFrom(Stream stream) => _data.InitializeFrom(stream);

        public DT Add(DT tensor) => Create(Computation.Add(_data, tensor.Data));
        public void AddInPlace(DT tensor) => Computation.AddInPlace(_data, tensor.Data);
        public DT Subtract(DT tensor) => Create(Computation.Subtract(_data, tensor.Data));
        public void SubtractInPlace(DT tensor) => Computation.SubtractInPlace(_data, tensor.Data);
        public DT PointwiseMultiply(DT tensor) => Create(Computation.Multiply(_data, tensor.Data));
        public void PointwiseMultiplyInPlace(DT tensor) => Computation.SubtractInPlace(_data, tensor.Data);
        public DT PointwiseDivide(DT tensor) => Create(Computation.Divide(_data, tensor.Data));
        public void PointwiseDivideInPlace(DT tensor) => Computation.DivideInPlace(_data, tensor.Data);

        ITensorSegment<T> IHaveTensorSegment<T>.Data => _data;

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

        public Vector<T> Reshape()
        {
            return new Vector<T>(Context, GetData());
        }

        public Matrix<T> Reshape(uint? rows, uint? columns)
        {
            var shape = _ResolveShape(_data.Size, new[] { rows, columns });
            return new Matrix<T>(Context, GetData(), shape[0], shape[1]);
        }

        public Tensor3D<T> Reshape(uint? depth, uint? rows, uint? columns)
        {
            var shape = _ResolveShape(_data.Size, new[] { depth, rows, columns });
            return new Tensor3D<T>(Context, GetData(), shape[0], shape[1], shape[2]);
        }

        public Tensor4D<T> Reshape(uint? count, uint? depth, uint? rows, uint? columns)
        {
            var shape = _ResolveShape(_data.Size, new[] { count, depth, rows, columns });
            return new Tensor4D<T>(Context, GetData(), shape[0], shape[1], shape[2], shape[3]);
        }
    }
}
