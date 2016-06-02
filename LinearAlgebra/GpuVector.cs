using ManagedCuda;
using MathNet.Numerics.LinearAlgebra.Single;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Icbld.BrightWire.Models;

namespace Icbld.BrightWire.LinearAlgebra
{
    internal class GpuVector : IVector
    {
        readonly CudaProvider _cuda;
        readonly CudaDeviceVariable<float> _data;
        readonly int _size;
        bool _disposed = false;
#if DEBUG
        static int _gid = 0;
        int _id = _gid++;
        public static int _badAlloc = -1;
        public static int _badDispose = -1;

        public bool IsValid { get { return !_disposed; } }
#else
        public bool IsValid { get { return true; } }
#endif

        public GpuVector(CudaProvider cuda, int size, Func<int, float> init)
        {
            _cuda = cuda;
            var data = new float[size];
            for (var i = 0; i < size; i++)
                data[i] = init(i);
            _data = new CudaDeviceVariable<float>(cuda.Context.AllocateMemory(size * sizeof(float)));
            _data.CopyToDevice(data);
            _size = size;
#if DEBUG
            if (_id == _badAlloc)
                Debugger.Break();
#endif
        }

        public GpuVector(CudaProvider cuda, IIndexableVector vector) : this(cuda, vector.Count, i => vector[i])
        {
        }

        internal GpuVector(CudaProvider cuda, int size, CudaDeviceVariable<float> data)
        {
            _cuda = cuda;
            _data = data;
            _size = size;
#if DEBUG
            if (_id == _badAlloc)
                Debugger.Break();
#endif
        }

#if DEBUG
        ~GpuVector()
        {
            if (!_disposed)
                Debug.WriteLine("\tVector {0} was not disposed!!", _id);
        }
#endif

        protected virtual void Dispose(bool disposing)
        {
#if DEBUG
            if (_id == _badDispose)
                Debugger.Break();
#endif
            if (disposing && !_disposed) {
                _data.Dispose();
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public override string ToString()
        {
            return AsIndexable().ToString();
        }

        public int Count
        {
            get
            {
                return _size;
            }
        }

        public object WrappedObject
        {
            get
            {
                return _data;
            }
        }

        public FloatArray Data
        {
            get
            {
                var data = new float[_size];
                _data.CopyToHost(data);

                return new FloatArray {
                    Data = data
                };
            }

            set
            {
                var data = new float[_size];
                _data.CopyToHost(data);

                if (value.Data != null) {
                    var data2 = value.Data;
                    for (int i = 0, len = data.Length; i < len; i++)
                        data[i] = data2[i];
                }

                _data.CopyToDevice(data);
            }
        }

        public IVector Add(IVector vector)
        {
            Debug.Assert(IsValid && vector.IsValid);
            var other = (GpuVector)vector;
            var ret = new CudaDeviceVariable<float>(other._data.Size);
            ret.CopyToDevice(other._data);
            _cuda.Blas.Axpy(1.0f, _data, 1, ret, 1);
            return new GpuVector(_cuda, _size, ret);
        }

        public void AddInPlace(IVector vector, float coefficient1 = 1, float coefficient2 = 1)
        {
            Debug.Assert(IsValid && vector.IsValid);
            var other = (GpuVector)vector;
            _cuda.AddInPlace(_data, other._data, _size, coefficient1, coefficient2);
        }

        public IIndexableVector AsIndexable()
        {
            var data = new float[_size];
            _data.CopyToHost(data);
            return new CpuVector(DenseVector.Create(_size, (i) => data[i]));
        }

        public IVector Clone()
        {
            Debug.Assert(IsValid);
            var data = new CudaDeviceVariable<float>(_size);
            data.CopyToDevice(_data);
            return new GpuVector(_cuda, _size, data);
        }

        public void CopyFrom(IVector vector)
        {
            Debug.Assert(vector.IsValid);
            var other = (GpuVector)vector;
            _data.CopyToDevice(other._data);
        }

        public float DotProduct(IVector vector)
        {
            Debug.Assert(IsValid && vector.IsValid);
            var other = (GpuVector)vector;
            return _cuda.Blas.Dot(_data, 1, other._data, 1);
        }

        public IVector GetNewVectorFromIndexes(int[] indices)
        {
            // defer to CPU version
            return AsIndexable().GetNewVectorFromIndexes(indices);
        }

        public float L2Norm()
        {
            Debug.Assert(IsValid);
            return _cuda.Blas.Norm2(_data, 1);
        }

        public int MaximumIndex()
        {
            Debug.Assert(IsValid);
            return _cuda.Blas.Max(_data, 1) - 1;
        }

        public int MinimumIndex()
        {
            Debug.Assert(IsValid);
            return _cuda.Blas.Min(_data, 1) - 1;
        }

        public void Multiply(float scalar)
        {
            Debug.Assert(IsValid);
            _cuda.Blas.Scale(scalar, _data, 1);
        }

        public IVector PointwiseMultiply(IVector vector)
        {
            Debug.Assert(IsValid && vector.IsValid);
            var other = (GpuVector)vector;
            return new GpuVector(_cuda, _size, _cuda.PointwiseMultiply(_data, other._data, _size));
        }

        public IVector Sqrt()
        {
            Debug.Assert(IsValid);
            var ret = _cuda.Sqrt(_data, _size, 0);
            return new GpuVector(_cuda, _size, ret);
        }

        public IVector Subtract(IVector vector)
        {
            Debug.Assert(IsValid && vector.IsValid);
            var other = (GpuVector)vector;
            var ret = new CudaDeviceVariable<float>(_data.Size);
            ret.CopyToDevice(_data);
            _cuda.Blas.Axpy(-1.0f, other._data, 1, ret, 1);
            return new GpuVector(_cuda, _size, ret);
        }

        public void SubtractInPlace(IVector vector, float coefficient1 = 1, float coefficient2 = 1)
        {
            Debug.Assert(IsValid && vector.IsValid);
            var other = (GpuVector)vector;
            _cuda.SubtractInPlace(_data, other._data, _size, coefficient1, coefficient2);
        }

        public IMatrix ToColumnMatrix(int numCols = 1)
        {
            Debug.Assert(IsValid);
            var ret = new CudaDeviceVariable<float>(_data.Size);
            ret.CopyToDevice(_data);
            return new GpuMatrix(_cuda, _size, 1, ret);
        }

        public IMatrix ToRowMatrix(int numRows = 1)
        {
            Debug.Assert(IsValid);
            var ret = new CudaDeviceVariable<float>(_data.Size);
            ret.CopyToDevice(_data);
            return new GpuMatrix(_cuda, 1, _size, ret);
        }

        public float EuclideanDistance(IVector vector)
        {
            // defer to CPU version
            return AsIndexable().EuclideanDistance(vector.AsIndexable());
        }

        public float CosineDistance(IVector vector)
        {
            // defer to CPU version
            return AsIndexable().CosineDistance(vector.AsIndexable());
        }

        public float ManhattanDistance(IVector vector)
        {
            // defer to CPU version
            return AsIndexable().ManhattanDistance(vector.AsIndexable());
        }

        public float MeanSquaredDistance(IVector vector)
        {
            // defer to CPU version
            return AsIndexable().MeanSquaredDistance(vector.AsIndexable());
        }

        public float SquaredEuclidean(IVector vector)
        {
            // defer to CPU version
            return AsIndexable().SquaredEuclidean(vector.AsIndexable());
        }
    }
}
