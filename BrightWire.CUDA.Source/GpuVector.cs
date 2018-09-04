using ManagedCuda;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using BrightWire.Models;
using ManagedCuda.BasicTypes;
using BrightWire.Cuda.Helper;
using BrightWire.LinearAlgebra.Helper;

namespace BrightWire.LinearAlgebra
{
	/// <summary>
	/// GPU backed vector
	/// </summary>
	class GpuVector : IVector, IHaveDeviceMemory
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

		public bool IsValid => !_disposed;
#else
        public bool IsValid => true;
#endif

		public GpuVector(CudaProvider cuda, IDeviceMemoryPtr data, bool isOwner)
		{
			_cuda = cuda;
			_data = data;
			cuda.Register(this);
#if DEBUG
			if (_id == _badAlloc)
				Debugger.Break();
#endif
		}

//		public GpuVector(CudaProvider cuda, IIndexableVector vector) : this(cuda, vector.Count, i => vector[i])
//		{
//		}

//		internal GpuVector(CudaProvider cuda, IDeviceMemoryPtr data)
//		{
//			_cuda = cuda;
//			_data = data;
//			cuda.Register(this);

//#if DEBUG
//			if (_id == _badAlloc)
//				Debugger.Break();
//#endif
//		}

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

		public int Count => _data.Size;
		public int BlockSize => CudaProvider.FLOAT_SIZE;

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

		public CudaDeviceVariable<float> CudaDeviceVariable => _data.DeviceVariable;
		public IDeviceMemoryPtr Memory => _data;

		public IVector Add(IVector vector)
		{
			Debug.Assert(IsValid && vector.IsValid);
			var other = (GpuVector)vector;
			Debug.Assert(other.Count == Count);

			var ret = _cuda.Allocate(other._data.Size);
			ret.CopyToDevice(other._data);
			_cuda.Blas.Axpy(1.0f, _data.DeviceVariable, 1, ret.DeviceVariable, 1);
			return new GpuVector(_cuda, ret, true);
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
			return new GpuVector(_cuda, data, true);
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
			return new GpuVector(_cuda, data, true);
		}

		public IVector Abs()
		{
			Debug.Assert(IsValid);
			return new GpuVector(_cuda, _cuda.Abs(_data, Count), true);
		}

		public IVector Log()
		{
			Debug.Assert(IsValid);
			return new GpuVector(_cuda, _cuda.Log(_data, Count), true);
		}

		public IVector Sigmoid()
		{
			Debug.Assert(IsValid);
			return new GpuVector(_cuda, _cuda.Sigmoid(_data, Count), true);
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

			return new GpuVector(_cuda, _cuda.PointwiseMultiply(_data, other._data, Count), true);
		}

		public IVector Sqrt()
		{
			Debug.Assert(IsValid);
			var ret = _cuda.Sqrt(_data, Count, 0);
			return new GpuVector(_cuda, ret, true);
		}

		public IVector Subtract(IVector vector)
		{
			Debug.Assert(IsValid && vector.IsValid);
			var other = (GpuVector)vector;
			Debug.Assert(other.Count == Count);

			var ret = _cuda.Allocate(_data.Size);
			ret.CopyToDevice(_data);
			_cuda.Blas.Axpy(-1.0f, other.CudaDeviceVariable, 1, ret.DeviceVariable, 1);
			return new GpuVector(_cuda, ret, true);
		}

		public void SubtractInPlace(IVector vector, float coefficient1 = 1, float coefficient2 = 1)
		{
			Debug.Assert(IsValid && vector.IsValid);
			var other = (GpuVector)vector;
			Debug.Assert(other.Count == Count);

			_cuda.SubtractInPlace(_data, other._data, Count, coefficient1, coefficient2);
		}

		public IMatrix AsColumnMatrix()
		{
			Debug.Assert(IsValid);
			return new GpuMatrix(_cuda, Count, 1, _data, false);
		}

		public IMatrix AsRowMatrix()
		{
			Debug.Assert(IsValid);
			return new GpuMatrix(_cuda, 1, Count, _data, false);
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
			} else if (type == NormalisationType.Standard) {
				var mean = Average();
				var stdDev = StdDev(mean);
				if (BoundMath.IsNotZero(stdDev))
					_cuda.Normalise(_data, Count, mean, stdDev);
			} else if (type == NormalisationType.Euclidean || type == NormalisationType.Manhattan) {
				float p;
				if (type == NormalisationType.Manhattan)
					p = L1Norm();
				else
					p = L2Norm();

				if (BoundMath.IsNotZero(p))
					Multiply(1f / p);
			}
		}

		public IVector Softmax()
		{
			Debug.Assert(IsValid);
			var minMax = GetMinMax();

			var softmax = _cuda.SoftmaxVector(_data, Count, minMax.Max);
			var softmaxSum = _cuda.SumValues(softmax, Count);
			if (BoundMath.IsNotZero(softmaxSum))
				_cuda.Blas.Scale(1f / softmaxSum, softmax.DeviceVariable, 1);
			return new GpuVector(_cuda, softmax, true);
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
				return _cuda.CreateVector(data.Count, i => ret[i]);
			} else if (distance == DistanceMetric.Euclidean) {
				var ptrArray = data.Cast<GpuVector>().Select(d => d._data.DevicePointer).ToArray();
				var ret = _cuda.MultiEuclideanDistance(_data, ptrArray, Count);
				using (var matrix = new GpuMatrix(_cuda, Count, data.Count, ret, true)) {
					using (var temp = matrix.ColumnSums())
						return temp.Sqrt();
				}
			} else if (distance == DistanceMetric.Manhattan) {
				var ptrArray = data.Cast<GpuVector>().Select(d => d._data.DevicePointer).ToArray();
				var ret = _cuda.MultiManhattanDistance(_data, ptrArray, Count);
				using (var matrix = new GpuMatrix(_cuda, Count, data.Count, ret, true)) {
					return matrix.ColumnSums();
				}
			} else {
				var distanceFunc = _GetDistanceFunc(distance);
				var ret = new float[data.Count];
				for (var i = 0; i < data.Count; i++)
					ret[i] = distanceFunc(data[i]);
				return _cuda.CreateVector(data.Count, i => ret[i]);
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
			return _cuda.CreateVector(data.Count, i => ret[i]);
		}

		public void Add(float scalar)
		{
			Debug.Assert(IsValid);
			_cuda.VectorAdd(_data, Count, scalar);
		}

		public IMatrix AsMatrix(int rows, int columns)
		{
			Debug.Assert(IsValid);
			return new GpuMatrix(_cuda, rows, columns, _data, false);
		}

		public IReadOnlyList<IVector> Split(int blockCount)
		{
			Debug.Assert(IsValid);
			var blockSize = Count / blockCount;
			return Enumerable.Range(0, blockCount).Select(i => {
				var ptr2 = _cuda.OffsetByBlock(_data, i, blockSize);
				var vector = new GpuVector(_cuda, ptr2, false);
				return vector;
			}).ToList();
		}

		public IMatrix SoftmaxDerivative()
		{
			Debug.Assert(IsValid);
			var ret = _cuda.VectorSoftmaxDerivative(_data, Count);
			return new GpuMatrix(_cuda, Count, Count, ret, true);
		}

		public void RotateInPlace(int blockCount)
		{
			Debug.Assert(IsValid);
			_cuda.RotateInPlace(_data, Count, blockCount);
		}

		public IVector Reverse()
		{
			Debug.Assert(IsValid);
			var ret = _cuda.Reverse(_data, Count);
			return new GpuVector(_cuda, ret, true);
		}

		public I3DTensor As3DTensor(int rows, int columns, int depth)
		{
			return new Gpu3DTensor(_cuda, rows, columns, depth, _data, false);
		}

		public I4DTensor As4DTensor(int rows, int columns, int depth, int count)
		{
			return new Gpu4DTensor(_cuda, rows, columns, depth, count, _data, false);
		}

		public float GetAt(int index)
		{
			Debug.Assert(IsValid);
			return _data.DeviceVariable[index];
		}
	}
}
