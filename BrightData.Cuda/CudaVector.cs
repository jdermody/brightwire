﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using BrightData.Helper;
using BrightData.LinearAlgebra;

namespace BrightData.Cuda
{
	/// <summary>
	/// GPU backed vector
	/// </summary>
    internal class CudaVector : IFloatVector, IHaveDeviceMemory
	{
		readonly CudaProvider _cuda;
		readonly IDeviceMemoryPtr _data;
		bool _disposed = false;
#if DEBUG
		static int _gid = 0;
		static int GetNextIndex() => Interlocked.Increment(ref _gid);
		readonly int _id = GetNextIndex();
		public static int _badAlloc = -1;
		public static int _badDispose = -1;

		public bool IsValid => !_disposed;
#else
		public bool IsValid => true;
#endif

		public CudaVector(CudaProvider cuda, IDeviceMemoryPtr data, bool isOwner)
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
		~CudaVector()
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

		public override string? ToString()
		{
			return AsIndexable().ToString();
		}

        public ILinearAlgebraProvider LinearAlgebraProvider => _cuda.DataContext.LinearAlgebraProvider;
		public uint Count => _data.Size;
		public IDeviceMemoryPtr Memory => _data;

		public Vector<float> Data
		{
			get
			{
				Debug.Assert(IsValid);
				var data = new float[Count];
				_data.CopyToHost(data);

				return _cuda.DataContext.CreateVector(data);
			}

			set
			{
				Debug.Assert(IsValid);
				var data = new float[Count];
				_data.CopyToHost(data);

                var data2 = value.Segment;
                for (uint i = 0, len = (uint)data.Length; i < len; i++)
                    data[i] = data2[i];

				_data.CopyToDevice(data);
			}
		}

		public IFloatVector Add(IFloatVector vector)
		{
			Debug.Assert(IsValid && vector.IsValid);
			var other = (CudaVector)vector;
			Debug.Assert(other.Count == Count);

			var ret = _cuda.Allocate(other._data.Size);
			ret.CopyToDevice(other._data);
			_cuda.Blas.Axpy(1.0f, _data.DeviceVariable, 1, ret.DeviceVariable, 1);
			return new CudaVector(_cuda, ret, true);
		}

		public void AddInPlace(IFloatVector vector, float coefficient1 = 1, float coefficient2 = 1)
		{
			Debug.Assert(IsValid && vector.IsValid);
			var other = (CudaVector)vector;
			Debug.Assert(other.Count == Count);

			_cuda.AddInPlace(_data, other._data, Count, coefficient1, coefficient2);
		}

		public IIndexableFloatVector AsIndexable()
		{
			Debug.Assert(IsValid);
			var data = new float[Count];
			_data.CopyToHost(data);
			return _cuda.NumericsProvider.CreateVector(Count, i => data[i]).AsIndexable();
		}

		public IFloatVector Clone()
		{
			Debug.Assert(IsValid);
			var data = _cuda.Allocate(Count);
			data.CopyToDevice(_data);
			return new CudaVector(_cuda, data, true);
		}

		public void CopyFrom(IFloatVector vector)
		{
			Debug.Assert(vector.IsValid);
			var other = (CudaVector)vector;
			Debug.Assert(other.Count == Count);

			_data.CopyToDevice(other._data);
		}

		public float DotProduct(IFloatVector vector)
		{
			Debug.Assert(IsValid && vector.IsValid);
			var other = (CudaVector)vector;
			Debug.Assert(other.Count == Count);

			return _cuda.Blas.Dot(_data.DeviceVariable, 1, other._data.DeviceVariable, 1);
		}

		public IFloatVector GetNewVectorFromIndexes(IEnumerable<uint> indices)
		{
			Debug.Assert(IsValid);
			var data = _cuda.VectorCopy(_data, indices.ToArray());
			return new CudaVector(_cuda, data, true);
		}

		public IFloatVector Abs()
		{
			Debug.Assert(IsValid);
			return new CudaVector(_cuda, _cuda.Abs(_data, Count), true);
		}

		public IFloatVector Log()
		{
			Debug.Assert(IsValid);
			return new CudaVector(_cuda, _cuda.Log(_data, Count), true);
		}

		public IFloatVector Sigmoid()
		{
			Debug.Assert(IsValid);
			return new CudaVector(_cuda, _cuda.Sigmoid(_data, Count), true);
		}

		public float L1Norm()
		{
			Debug.Assert(IsValid);
			var abs = (CudaVector)Abs();
			return _cuda.SumValues(abs._data, Count);
		}

		public float L2Norm()
		{
			Debug.Assert(IsValid);
			return _cuda.Blas.Norm2(_data.DeviceVariable, 1);
		}

		public uint MaximumAbsoluteIndex()
		{
			Debug.Assert(IsValid);
			return (uint)_cuda.Blas.Max(_data.DeviceVariable, 1) - 1;
		}

		public uint MinimumAbsoluteIndex()
		{
			Debug.Assert(IsValid);
			return (uint)_cuda.Blas.Min(_data.DeviceVariable, 1) - 1;
		}

		public void Multiply(float scalar)
		{
			Debug.Assert(IsValid);
			_cuda.Blas.Scale(scalar, _data.DeviceVariable, 1);
		}

		public IFloatVector PointwiseMultiply(IFloatVector vector)
		{
			Debug.Assert(IsValid && vector.IsValid);
			var other = (CudaVector)vector;
			Debug.Assert(other.Count == Count);

			return new CudaVector(_cuda, _cuda.PointwiseMultiply(_data, other._data, Count), true);
		}

		public IFloatVector Sqrt()
		{
			Debug.Assert(IsValid);
			var ret = _cuda.Sqrt(_data, Count, 1e-8f);
			return new CudaVector(_cuda, ret, true);
		}

		public IFloatVector Subtract(IFloatVector vector)
		{
			Debug.Assert(IsValid && vector.IsValid);
			var other = (CudaVector)vector;
			Debug.Assert(other.Count == Count);

			var ret = _cuda.Allocate(_data.Size);
			ret.CopyToDevice(_data);
			_cuda.Blas.Axpy(-1.0f, other.Memory.DeviceVariable, 1, ret.DeviceVariable, 1);
			return new CudaVector(_cuda, ret, true);
		}

		public void SubtractInPlace(IFloatVector vector, float coefficient1 = 1, float coefficient2 = 1)
		{
			Debug.Assert(IsValid && vector.IsValid);
			var other = (CudaVector)vector;
			Debug.Assert(other.Count == Count);

			_cuda.SubtractInPlace(_data, other._data, Count, coefficient1, coefficient2);
		}

		public IFloatMatrix ReshapeAsColumnMatrix()
		{
			Debug.Assert(IsValid);
			return new CudaMatrix(_cuda, Count, 1, _data, false);
		}

		public IFloatMatrix ReshapeAsRowMatrix()
		{
			Debug.Assert(IsValid);
			return new CudaMatrix(_cuda, 1, Count, _data, false);
		}

		public float EuclideanDistance(IFloatVector vector)
		{
			Debug.Assert(IsValid && vector.IsValid);
			var other = (CudaVector)vector;
			Debug.Assert(other.Count == Count);

			return _cuda.EuclideanDistance(_data, other._data, Count);
		}

		public float CosineDistance(IFloatVector vector)
		{
			Debug.Assert(IsValid && vector.IsValid);
			var other = (CudaVector)vector;
			Debug.Assert(other.Count == Count);

			return _cuda.CosineDistance(_data, other._data, Count);
        }

		public float ManhattanDistance(IFloatVector vector)
		{
			Debug.Assert(IsValid && vector.IsValid);
			var other = (CudaVector)vector;
			Debug.Assert(other.Count == Count);

			return _cuda.ManhattanDistance(_data, other._data, Count);
		}

		public float MeanSquaredDistance(IFloatVector vector)
		{
			Debug.Assert(IsValid && vector.IsValid);
			var norm = Subtract(vector).L2Norm();
			return norm * norm / Count;
		}

		public float SquaredEuclidean(IFloatVector vector)
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

		public void Normalize(NormalizationType type)
		{
			Debug.Assert(IsValid);
			if (type == NormalizationType.FeatureScale) {
				var (min, max) = GetMinMax();
				float range = max - min;
				if (range > 0)
					_cuda.Normalise(_data, Count, min, range);
			} else if (type == NormalizationType.Standard) {
				var mean = Average();
				var stdDev = StdDev(mean);
				if (FloatMath.IsNotZero(stdDev))
					_cuda.Normalise(_data, Count, mean, stdDev);
			} else if (type == NormalizationType.Euclidean || type == NormalizationType.Manhattan) {
				var p = type == NormalizationType.Manhattan
					? L1Norm()
					: L2Norm()
				;

				if (FloatMath.IsNotZero(p))
					Multiply(1f / p);
			}
		}

		public IFloatVector Softmax()
		{
			Debug.Assert(IsValid);
			var (_, max) = GetMinMax();

			var softmax = _cuda.SoftmaxVector(_data, Count, max);
			var softmaxSum = _cuda.SumValues(softmax, Count);
			if (FloatMath.IsNotZero(softmaxSum))
				_cuda.Blas.Scale(1f / softmaxSum, softmax.DeviceVariable, 1);
			return new CudaVector(_cuda, softmax, true);
		}

		Func<IFloatVector, float> GetDistanceFunc(DistanceMetric distance)
        {
            return distance switch {
                DistanceMetric.Cosine => CosineDistance,
                DistanceMetric.Euclidean => EuclideanDistance,
                DistanceMetric.Manhattan => ManhattanDistance,
                DistanceMetric.MeanSquared => MeanSquaredDistance,
                DistanceMetric.SquaredEuclidean => SquaredEuclidean,
                _ => throw new NotImplementedException()
            };
        }

		public IFloatVector FindDistances(IFloatVector[] data, DistanceMetric distance)
		{
			Debug.Assert(IsValid && data.All(v => v.IsValid));
			if (distance == DistanceMetric.Cosine) {
				var norm = DotProduct(this);
				var dataNorm = data.Select(d => d.DotProduct(d)).ToList();
				var ret = new float[data.Length];
				for (var i = 0; i < data.Length; i++)
					ret[i] = Convert.ToSingle(1d - DotProduct(data[i]) / Math.Sqrt(norm * dataNorm[i]));
				return _cuda.CreateVector((uint)data.Length, i => ret[i]);
			} else if (distance == DistanceMetric.Euclidean || distance == DistanceMetric.Manhattan) {
				var ret = _cuda.CalculateDistances(new[] { (IFloatVector)this }, data, distance);
				return ret.ReshapeAsVector();
			} else {
				var distanceFunc = GetDistanceFunc(distance);
				var ret = new float[data.Length];
				for (var i = 0; i < data.Length; i++)
					ret[i] = distanceFunc(data[i]);
				return _cuda.CreateVector((uint)data.Length, i => ret[i]);
			}
		}

		public float FindDistance(IFloatVector other, DistanceMetric distance)
		{
			Debug.Assert(IsValid && other.IsValid);
            using var ret = FindDistances(new[] { other }, distance);
            return ret.Data.Segment[0];
		}

		public IFloatVector CosineDistance(IFloatVector[] data, ref float[]? dataNorm)
		{
			Debug.Assert(IsValid && data.All(v => v.IsValid));
			var norm = DotProduct(this);
			dataNorm ??= data.Select(d => d.DotProduct(d)).ToArray();

			var ret = new float[data.Length];
			for (var i = 0; i < data.Length; i++)
				ret[i] = Convert.ToSingle(1d - DotProduct(data[i]) / Math.Sqrt(norm * dataNorm[i]));
			return _cuda.CreateVector((uint)data.Length, i => ret[i]);
		}

		public void Add(float scalar)
		{
			Debug.Assert(IsValid);
			_cuda.VectorAdd(_data, Count, scalar);
		}

		public IFloatVector[] Split(uint blockCount)
		{
			Debug.Assert(IsValid);
			var blockSize = Count / blockCount;
			return blockCount.AsRange().Select(i => {
				var ptr2 = _cuda.OffsetByBlock(_data, i, blockSize);
				var vector = new CudaVector(_cuda, ptr2, false);
				return (IFloatVector)vector;
			}).ToArray();
		}

		public IFloatMatrix SoftmaxDerivative()
		{
			Debug.Assert(IsValid);
			var ret = _cuda.VectorSoftmaxDerivative(_data, Count);
			return new CudaMatrix(_cuda, Count, Count, ret, true);
		}

		public void RotateInPlace(uint blockCount)
		{
			Debug.Assert(IsValid);
			_cuda.RotateInPlace(_data, Count, blockCount);
		}

		public IFloatVector Reverse()
		{
			Debug.Assert(IsValid);
			var ret = _cuda.Reverse(_data, Count);
			return new CudaVector(_cuda, ret, true);
		}

		public float GetAt(uint index)
		{
			Debug.Assert(IsValid);
			return _data.DeviceVariable[index];
		}

		public void SetAt(uint index, float value)
		{
			Debug.Assert(IsValid);
			_data.DeviceVariable[index] = value;
		}

		public bool IsEntirelyFinite()
		{
			return _cuda.IsFinite(_data, _data.Size);
		}

        public void RoundInPlace(float lower = 0f, float upper = 1f, float mid = 0.5f)
        {
            _cuda.RoundInPlace(_data, _data.Size, lower, upper, mid);
        }

        public IFloatMatrix ReshapeAsMatrix(uint rows, uint columns)
		{
			Debug.Assert(IsValid && rows * columns == _data.Size);
			return new CudaMatrix(_cuda, rows, columns, _data, false);
		}

		public I3DFloatTensor ReshapeAs3DTensor(uint rows, uint columns, uint depth)
		{
			Debug.Assert(IsValid && rows * columns * depth == _data.Size);
			return new Cuda3DTensor(_cuda, rows, columns, depth, _data, false);
		}

		public I4DFloatTensor ReshapeAs4DTensor(uint rows, uint columns, uint depth, uint count)
		{
			Debug.Assert(IsValid && rows * columns * depth * count == _data.Size);
			return new Cuda4DTensor(_cuda, rows, columns, depth, count, _data, false);
		}
	}
}
