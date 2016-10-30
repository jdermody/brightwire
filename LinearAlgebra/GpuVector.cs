using ManagedCuda;
using MathNet.Numerics.LinearAlgebra.Single;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightWire.Models;
using BrightWire.Models.Simple;

namespace BrightWire.LinearAlgebra
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
            cuda.Register(this);
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
            cuda.Register(this);
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

        public CudaDeviceVariable<float> CudaDeviceVariable { get { return _data; } }

        public IVector Add(IVector vector)
        {
            Debug.Assert(IsValid && vector.IsValid);
            var other = (GpuVector)vector;
            Debug.Assert(other._size == _size);

            var ret = new CudaDeviceVariable<float>(other._data.Size);
            ret.CopyToDevice(other._data);
            _cuda.Blas.Axpy(1.0f, _data, 1, ret, 1);
            return new GpuVector(_cuda, _size, ret);
        }

        public void AddInPlace(IVector vector, float coefficient1 = 1, float coefficient2 = 1)
        {
            Debug.Assert(IsValid && vector.IsValid);
            var other = (GpuVector)vector;
            Debug.Assert(other._size == _size);

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
            Debug.Assert(other._size == _size);

            _data.CopyToDevice(other._data);
        }

        public float DotProduct(IVector vector)
        {
            Debug.Assert(IsValid && vector.IsValid);
            var other = (GpuVector)vector;
            Debug.Assert(other._size == _size);

            return _cuda.Blas.Dot(_data, 1, other._data, 1);
        }

        public IVector GetNewVectorFromIndexes(IReadOnlyList<int> indices)
        {
            // defer to CPU version
            return AsIndexable().GetNewVectorFromIndexes(indices);
        }

        public IVector Abs()
        {
            return new GpuVector(_cuda, _size, _cuda.Abs(_data, _size));
        }

        public IVector Log()
        {
            return new GpuVector(_cuda, _size, _cuda.Log(_data, _size));
        }

        public IVector Sigmoid()
        {
            return new GpuVector(_cuda, _size, _cuda.Sigmoid(_data, _size));
        }

        public float L1Norm()
        {
            Debug.Assert(IsValid);
            var abs = Abs() as GpuVector;
            return _cuda.SumValues(abs._data, _size);
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
            Debug.Assert(other._size == _size);

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
            Debug.Assert(other._size == _size);

            var ret = new CudaDeviceVariable<float>(_data.Size);
            ret.CopyToDevice(_data);
            _cuda.Blas.Axpy(-1.0f, other._data, 1, ret, 1);
            return new GpuVector(_cuda, _size, ret);
        }

        public void SubtractInPlace(IVector vector, float coefficient1 = 1, float coefficient2 = 1)
        {
            Debug.Assert(IsValid && vector.IsValid);
            var other = (GpuVector)vector;
            Debug.Assert(other._size == _size);

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
            Debug.Assert(IsValid && vector.IsValid);
            var other = (GpuVector)vector;
            Debug.Assert(other._size == _size);

            return _cuda.EuclideanDistance(_data, other._data, _size);
        }

        public float CosineDistance(IVector vector)
        {
            Debug.Assert(IsValid && vector.IsValid);
            var other = (GpuVector)vector;
            Debug.Assert(other._size == _size);

            var ab = DotProduct(other);
            var a2 = DotProduct(this);
            var b2 = other.DotProduct(other);
            return (float)(1d - (ab / (Math.Sqrt(a2 * b2))));
        }

        public float ManhattanDistance(IVector vector)
        {
            Debug.Assert(IsValid && vector.IsValid);
            var other = (GpuVector)vector;
            Debug.Assert(other._size == _size);

            return _cuda.ManhattanDistance(_data, other._data, _size);
        }

        public float MeanSquaredDistance(IVector vector)
        {
            var norm = Subtract(vector).L2Norm();
            return norm * norm / _size;
        }

        public float SquaredEuclidean(IVector vector)
        {
            var norm = Subtract(vector).L2Norm();
            return norm * norm;
        }

        public MinMax GetMinMax()
        {
            return _cuda.FindMinAndMax(_data, _size);
        }

        public float Average()
        {
            return _cuda.SumValues(_data, _size) / _size;
        }

        public float StdDev(float? mean)
        {
            return _cuda.FindStdDev(_data, _size, mean ?? Average());
        }

        public void Normalise(NormalisationType type)
        {
            if (type == NormalisationType.FeatureScale) {
                var minMax = GetMinMax();
                float range = minMax.Max - minMax.Min;
                if (range > 0)
                    _cuda.Normalise(_data, _size, minMax.Min, range);
            }
            else if (type == NormalisationType.Standard) {
                var mean = Average();
                var stdDev = StdDev(mean);
                if (stdDev != 0)
                    _cuda.Normalise(_data, _size, mean, stdDev);
            }
            else if (type == NormalisationType.Euclidean || type == NormalisationType.Manhattan) {
                float p = 0f;
                if (type == NormalisationType.Manhattan)
                    p = L1Norm();
                else
                    p = L2Norm();

                if(p != 0f)
                    Multiply(1f / p);
            }
        }

        public IVector Softmax()
        {
            var minMax = GetMinMax();

            var softmax = _cuda.SoftmaxVector(_data, _size, minMax.Max);
            var softmaxSum = _cuda.SumValues(softmax, _size);
            if(softmaxSum != 0)
                _cuda.Blas.Scale(1f / softmaxSum, softmax, 1);
            return new GpuVector(_cuda, _size, softmax);
        }

        Func<IVector, float> _GetDistanceFunc(DistanceMetric distance)
        {
            switch (distance) {
                case DistanceMetric.Cosine:
                    return CosineDistance;
                case DistanceMetric.Euclidean:
                    return EuclideanDistance;
                case DistanceMetric.Manhattan:
                    return ManhattanDistance;
                case DistanceMetric.MeanSquared:
                    return MeanSquaredDistance;
                case DistanceMetric.SquaredEuclidean:
                    return SquaredEuclidean;
            }
            throw new NotImplementedException();
        }

        public IVector FindDistances(IReadOnlyList<IVector> data, DistanceMetric distance)
        {
            if(distance == DistanceMetric.Cosine) {
                var norm = DotProduct(this);
                var dataNorm = data.Select(d => d.DotProduct(d)).ToList();
                var ret = new float[data.Count];
                for (var i = 0; i < data.Count; i++)
                    ret[i] = Convert.ToSingle(1d - DotProduct(data[i]) / Math.Sqrt(norm * dataNorm[i]));
                return new GpuVector(_cuda, data.Count, i => ret[i]);
            }
            else if (distance == DistanceMetric.Euclidean) {
                var ptrArray = data.Cast<GpuVector>().Select(d => d._data.DevicePointer).ToArray();
                var ret = _cuda.MultiEuclideanDistance(_data, ptrArray, _size);
                using (var matrix = new GpuMatrix(_cuda, _size, data.Count, ret)) {
                    using(var temp = matrix.ColumnSums())
                        return temp.Sqrt();
                }
            }else if(distance == DistanceMetric.Manhattan) {
                var ptrArray = data.Cast<GpuVector>().Select(d => d._data.DevicePointer).ToArray();
                var ret = _cuda.MultiManhattanDistance(_data, ptrArray, _size);
                using (var matrix = new GpuMatrix(_cuda, _size, data.Count, ret)) {
                    return matrix.ColumnSums();
                }
            }
            else {
                var distanceFunc = _GetDistanceFunc(distance);
                var ret = new float[data.Count];
                for(var i = 0; i < data.Count; i++)
                    ret[i] = distanceFunc(data[i]);
                return new GpuVector(_cuda, data.Count, i => ret[i]);
            }
        }

        public float FindDistance(IVector other, DistanceMetric distance)
        {
            using (var ret = FindDistances(new[] { other }, distance))
                return ret.Data.Data[0];
        }

        public IVector CosineDistance(IReadOnlyList<IVector> data, ref float[] dataNorm)
        {
            var norm = DotProduct(this);
            if (dataNorm == null)
                dataNorm = data.Select(d => d.DotProduct(d)).ToArray();

            var ret = new float[data.Count];
            for (var i = 0; i < data.Count; i++)
                ret[i] = Convert.ToSingle(1d - DotProduct(data[i]) / Math.Sqrt(norm * dataNorm[i]));
            return new GpuVector(_cuda, data.Count, i => ret[i]);
        }

        public void Add(float scalar)
        {
            Debug.Assert(IsValid);
            _cuda.VectorAdd(_data, _size, scalar);
        }
    }
}
