using ManagedCuda;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BrightWire.Models;
using ManagedCuda.BasicTypes;
using BrightWire.Cuda.Helper;
using System.Threading;

namespace BrightWire.LinearAlgebra
{
    /// <summary>
    /// GPU backed vector
    /// </summary>
    internal class GpuVector : IVector
    {
        readonly CudaProvider _cuda;
        readonly IDeviceMemoryPtr _data;
        bool _disposed = false;
#if DEBUG
        static int _gid = 0;
        static int _GetNextIndex() => Interlocked.Increment(ref _gid);
        readonly int _id = _GetNextIndex();
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
            _data = cuda.Allocate(size);
            _data.CopyToDevice(data);
            cuda.Register(this);
#if DEBUG
            if (_id == _badAlloc)
                Debugger.Break();
#endif
        }

        public GpuVector(CudaProvider cuda, IIndexableVector vector) : this(cuda, vector.Count, i => vector[i])
        {
        }

        internal GpuVector(CudaProvider cuda, IDeviceMemoryPtr data)
        {
            _cuda = cuda;
            _data = data;
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
                _data.Free();
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
#if DEBUG
            GC.SuppressFinalize(this);
#endif
        }

        public override string ToString()
        {
            return AsIndexable().ToString();
        }

        public int Count
        {
            get
            {
                return _data.Size;
            }
        }

        public FloatVector Data
        {
            get
            {
                Debug.Assert(IsValid);
                var data = new float[Count];
                _data.CopyToHost(data);

                return new FloatVector {
                    Data = data
                };
            }

            set
            {
                Debug.Assert(IsValid);
                var data = new float[Count];
                _data.CopyToHost(data);

                if (value.Data != null) {
                    var data2 = value.Data;
                    for (int i = 0, len = data.Length; i < len; i++)
                        data[i] = data2[i];
                }

                _data.CopyToDevice(data);
            }
        }

        internal CudaDeviceVariable<float> CudaDeviceVariable { get { return _data.DeviceVariable; } }
        internal IDeviceMemoryPtr Memory => _data;

        public IVector Add(IVector vector)
        {
            Debug.Assert(IsValid && vector.IsValid);
            var other = (GpuVector)vector;
            Debug.Assert(other.Count == Count);

            var ret = _cuda.Allocate(other._data.Size);
            ret.CopyToDevice(other._data);
            _cuda.Blas.Axpy(1.0f, _data.DeviceVariable, 1, ret.DeviceVariable, 1);
            return new GpuVector(_cuda, ret);
        }

        public void AddInPlace(IVector vector, float coefficient1 = 1, float coefficient2 = 1)
        {
            Debug.Assert(IsValid && vector.IsValid);
            var other = (GpuVector)vector;
            Debug.Assert(other.Count == Count);

            _cuda.AddInPlace(_data, other._data, Count, coefficient1, coefficient2);
        }

        public IIndexableVector AsIndexable()
        {
            Debug.Assert(IsValid);
            var data = new float[Count];
            _data.CopyToHost(data);
            return _cuda.NumericsProvider.CreateVector(Count, i => data[i]).AsIndexable();
        }

        public IVector Clone()
        {
            Debug.Assert(IsValid);
            var data = _cuda.Allocate(Count);
            data.CopyToDevice(_data);
            return new GpuVector(_cuda, data);
        }

        public void CopyFrom(IVector vector)
        {
            Debug.Assert(vector.IsValid);
            var other = (GpuVector)vector;
            Debug.Assert(other.Count == Count);

            _data.CopyToDevice(other._data);
        }

        public float DotProduct(IVector vector)
        {
            Debug.Assert(IsValid && vector.IsValid);
            var other = (GpuVector)vector;
            Debug.Assert(other.Count == Count);

            return _cuda.Blas.Dot(_data.DeviceVariable, 1, other._data.DeviceVariable, 1);
        }

        public IVector GetNewVectorFromIndexes(IReadOnlyList<int> indices)
        {
            Debug.Assert(IsValid);
            var data = _cuda.VectorCopy(_data, Count, indices.ToArray());
            return new GpuVector(_cuda, data);
        }

        public IVector Abs()
        {
            Debug.Assert(IsValid);
            return new GpuVector(_cuda, _cuda.Abs(_data, Count));
        }

        public IVector Log()
        {
            Debug.Assert(IsValid);
            return new GpuVector(_cuda, _cuda.Log(_data, Count));
        }

        public IVector Sigmoid()
        {
            Debug.Assert(IsValid);
            return new GpuVector(_cuda, _cuda.Sigmoid(_data, Count));
        }

        public float L1Norm()
        {
            Debug.Assert(IsValid);
            var abs = (GpuVector)Abs();
            return _cuda.SumValues(abs._data, Count);
        }

        public float L2Norm()
        {
            Debug.Assert(IsValid);
            return _cuda.Blas.Norm2(_data.DeviceVariable, 1);
        }

        public int MaximumIndex()
        {
            Debug.Assert(IsValid);
            return _cuda.Blas.Max(_data.DeviceVariable, 1) - 1;
        }

        public int MinimumIndex()
        {
            Debug.Assert(IsValid);
            return _cuda.Blas.Min(_data.DeviceVariable, 1) - 1;
        }

        public void Multiply(float scalar)
        {
            Debug.Assert(IsValid);
            _cuda.Blas.Scale(scalar, _data.DeviceVariable, 1);
        }

        public IVector PointwiseMultiply(IVector vector)
        {
            Debug.Assert(IsValid && vector.IsValid);
            var other = (GpuVector)vector;
            Debug.Assert(other.Count == Count);

            return new GpuVector(_cuda, _cuda.PointwiseMultiply(_data, other._data, Count));
        }

        public IVector Sqrt()
        {
            Debug.Assert(IsValid);
            var ret = _cuda.Sqrt(_data, Count, 0);
            return new GpuVector(_cuda, ret);
        }

        public IVector Subtract(IVector vector)
        {
            Debug.Assert(IsValid && vector.IsValid);
            var other = (GpuVector)vector;
            Debug.Assert(other.Count == Count);

            var ret = _cuda.Allocate(_data.Size);
            ret.CopyToDevice(_data);
            _cuda.Blas.Axpy(-1.0f, other.CudaDeviceVariable, 1, ret.DeviceVariable, 1);
            return new GpuVector(_cuda, ret);
        }

        public void SubtractInPlace(IVector vector, float coefficient1 = 1, float coefficient2 = 1)
        {
            Debug.Assert(IsValid && vector.IsValid);
            var other = (GpuVector)vector;
            Debug.Assert(other.Count == Count);

            _cuda.SubtractInPlace(_data, other._data, Count, coefficient1, coefficient2);
        }

        public IMatrix ToColumnMatrix()
        {
            Debug.Assert(IsValid);
            //var ret = _cuda.Allocate(_data.Size);
            //ret.CopyToDevice(_data);
            //return new GpuMatrix(_cuda, Count, 1, ret);
            return new GpuMatrix(_cuda, Count, 1, _data);
        }

        public IMatrix ToRowMatrix()
        {
            Debug.Assert(IsValid);
            //var ret = _cuda.Allocate(_data.Size);
            //ret.CopyToDevice(_data);
            //return new GpuMatrix(_cuda, 1, Count, ret);
            return new GpuMatrix(_cuda, 1, Count, _data);
        }

        public float EuclideanDistance(IVector vector)
        {
            Debug.Assert(IsValid && vector.IsValid);
            var other = (GpuVector)vector;
            Debug.Assert(other.Count == Count);

            return _cuda.EuclideanDistance(_data, other._data, Count);
        }

        public float CosineDistance(IVector vector)
        {
            Debug.Assert(IsValid && vector.IsValid);
            var other = (GpuVector)vector;
            Debug.Assert(other.Count == Count);

            var ab = DotProduct(other);
            var a2 = DotProduct(this);
            var b2 = other.DotProduct(other);
            return (float)(1d - (ab / (Math.Sqrt(a2 * b2))));
        }

        public float ManhattanDistance(IVector vector)
        {
            Debug.Assert(IsValid && vector.IsValid);
            var other = (GpuVector)vector;
            Debug.Assert(other.Count == Count);

            return _cuda.ManhattanDistance(_data, other._data, Count);
        }

        public float MeanSquaredDistance(IVector vector)
        {
            Debug.Assert(IsValid && vector.IsValid);
            var norm = Subtract(vector).L2Norm();
            return norm * norm / Count;
        }

        public float SquaredEuclidean(IVector vector)
        {
            Debug.Assert(IsValid && vector.IsValid);
            var norm = Subtract(vector).L2Norm();
            return norm * norm;
        }

        public (float Min, float Max) GetMinMax()
        {
            Debug.Assert(IsValid);
            return _cuda.FindMinAndMax(_data, Count);
        }

        public float Average()
        {
            Debug.Assert(IsValid);
            return _cuda.SumValues(_data, Count) / Count;
        }

        public float StdDev(float? mean)
        {
            Debug.Assert(IsValid);
            return _cuda.FindStdDev(_data, Count, mean ?? Average());
        }

        public void Normalise(NormalisationType type)
        {
            Debug.Assert(IsValid);
            if (type == NormalisationType.FeatureScale) {
                var minMax = GetMinMax();
                float range = minMax.Max - minMax.Min;
                if (range > 0)
                    _cuda.Normalise(_data, Count, minMax.Min, range);
            }
            else if (type == NormalisationType.Standard) {
                var mean = Average();
                var stdDev = StdDev(mean);
                if (stdDev != 0f)
                    _cuda.Normalise(_data, Count, mean, stdDev);
            }
            else if (type == NormalisationType.Euclidean || type == NormalisationType.Manhattan) {
                float p;
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
            Debug.Assert(IsValid);
            var minMax = GetMinMax();

            var softmax = _cuda.SoftmaxVector(_data, Count, minMax.Max);
            var softmaxSum = _cuda.SumValues(softmax, Count);
            if(softmaxSum != 0f)
                _cuda.Blas.Scale(1f / softmaxSum, softmax.DeviceVariable, 1);
            return new GpuVector(_cuda, softmax);
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
            Debug.Assert(IsValid && data.All(v => v.IsValid));
            if (distance == DistanceMetric.Cosine) {
                var norm = DotProduct(this);
                var dataNorm = data.Select(d => d.DotProduct(d)).ToList();
                var ret = new float[data.Count];
                for (var i = 0; i < data.Count; i++)
                    ret[i] = Convert.ToSingle(1d - DotProduct(data[i]) / Math.Sqrt(norm * dataNorm[i]));
                return new GpuVector(_cuda, data.Count, i => ret[i]);
            }
            else if (distance == DistanceMetric.Euclidean) {
                var ptrArray = data.Cast<GpuVector>().Select(d => d._data.DevicePointer).ToArray();
                var ret = _cuda.MultiEuclideanDistance(_data, ptrArray, Count);
                using (var matrix = new GpuMatrix(_cuda, Count, data.Count, ret)) {
                    using(var temp = matrix.ColumnSums())
                        return temp.Sqrt();
                }
            }else if(distance == DistanceMetric.Manhattan) {
                var ptrArray = data.Cast<GpuVector>().Select(d => d._data.DevicePointer).ToArray();
                var ret = _cuda.MultiManhattanDistance(_data, ptrArray, Count);
                using (var matrix = new GpuMatrix(_cuda, Count, data.Count, ret)) {
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
            Debug.Assert(IsValid && other.IsValid);
            using (var ret = FindDistances(new[] { other }, distance))
                return ret.Data.Data[0];
        }

        public IVector CosineDistance(IReadOnlyList<IVector> data, ref float[] dataNorm)
        {
            Debug.Assert(IsValid && data.All(v => v.IsValid));
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
            _cuda.VectorAdd(_data, Count, scalar);
        }

        public IMatrix ConvertInPlaceToMatrix(int rows, int columns)
        {
            Debug.Assert(IsValid);
            return new GpuMatrix(_cuda, rows, columns, _data);
        }

        public IReadOnlyList<IVector> Split(int blockCount)
        {
            Debug.Assert(IsValid);
            var blockSize = Count / blockCount;
            var ptr = _data.DevicePointer.Pointer;
            var size = blockSize * sizeof(float);
            return Enumerable.Range(0, blockCount).Select(i => {
                var offset = (size * i);
                var ptr2 = new PtrToMemory(_cuda.Context, new CUdeviceptr(ptr + offset), size);
                return new GpuVector(_cuda, ptr2);
            }).ToList();
        }

        public IMatrix SoftmaxDerivative()
        {
            Debug.Assert(IsValid);
            var ret = _cuda.VectorSoftmaxDerivative(_data, Count);
            return new GpuMatrix(_cuda, Count, Count, ret);
        }

        public IVector Rotate(int blockCount)
        {
            Debug.Assert(IsValid);
            var ret = _cuda.Rotate(_data, Count, blockCount);
            return new GpuVector(_cuda, ret);
        }

        public IVector Reverse()
        {
            Debug.Assert(IsValid);
            var ret = _cuda.Reverse(_data, Count);
            return new GpuVector(_cuda, ret);
        }

        public I3DTensor ConvertTo3DTensor(int rows, int columns, int depth)
        {
            var matrixList = Split(depth).Select(v => v.ConvertInPlaceToMatrix(rows, columns)).Cast<GpuMatrix>().ToList();
            return new Gpu3DTensor(_cuda, rows, columns, depth, matrixList);
        }
    }
}
