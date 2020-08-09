using BrightWire.Models;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BrightWire.LinearAlgebra.Helper;
using BrightData;

namespace BrightWire.LinearAlgebra
{
    /// <summary>
    /// Vector that uses the CPU based math.net numerics library
    /// </summary>
    class CpuVector : IIndexableVector
    {
        readonly MathNet.Numerics.LinearAlgebra.Vector<float> _vector;

        public bool IsValid => true;

	    public CpuVector(DenseVector vector)
        {
            _vector = vector;
        }
        public CpuVector(MathNet.Numerics.LinearAlgebra.Vector<float> vector)
        {
            _vector = vector;
        }

        public void Dispose()
        {
            // nop
        }

        public float this[int index]
        {
            get => _vector[index];
	        set => _vector[index] = value;
        }

	    public float[] ToArray() => _vector.ToArray();
	    public float[] GetInternalArray() => _vector.AsArray();
        public int Count => _vector.Count;

	    public IVector Add(IVector vector)
        {
            var other = (CpuVector)vector;
            return new CpuVector(_vector.Add(other._vector));
        }

        public void AddInPlace(IVector vector, float coefficient1 = 1.0f, float coefficient2 = 1.0f)
        {
            var other = (CpuVector)vector;
            _vector.MapIndexedInplace((i, v) => (v * coefficient1) + (other[i] * coefficient2));
        }

        public float L1Norm()
        {
            return Convert.ToSingle(_vector.L1Norm());
        }

        public float L2Norm()
        {
            return Convert.ToSingle(_vector.L2Norm());
        }

        public int MaximumIndex()
        {
            return _vector.Map(Math.Abs).MaximumIndex();
        }

        public int MinimumIndex()
        {
            return _vector.Map(Math.Abs).MinimumIndex();
        }

        public void Multiply(float scalar)
        {
            _vector.MapInplace(x => x * scalar);
        }

        public IVector Subtract(IVector vector)
        {
            var other = (CpuVector)vector;
            return new CpuVector(_vector.Subtract(other._vector));
        }

        public void SubtractInPlace(IVector vector, float coefficient1 = 1.0f, float coefficient2 = 1.0f)
        {
            var other = (CpuVector)vector;
            _vector.MapIndexedInplace((i, v) => (v * coefficient1) - (other[i] * coefficient2));
        }

        public IMatrix ReshapeAsColumnMatrix()
        {
            return new CpuMatrix(DenseMatrix.Build.Dense(_vector.Count, 1, GetInternalArray()));
        }

        public IMatrix ReshapeAsRowMatrix()
        {
            return new CpuMatrix(DenseMatrix.Build.Dense(1, _vector.Count, GetInternalArray()));
        }

        public override string ToString()
        {
            return _vector.ToVectorString();
        }

        public FloatVector Data
        {
            get => new FloatVector {
	            Data = _vector.ToArray()
            };

	        set
            {
                if (value.Data != null) {
                    var data = value.Data;
                    for (int i = 0, len = data.Length; i < len; i++)
                        _vector[i] = data[i];
                }
            }
        }

        public IIndexableVector AsIndexable()
        {
            return this;
        }

        public IVector PointwiseMultiply(IVector vector)
        {
            var other = (CpuVector)vector;
            return new CpuVector(_vector.PointwiseMultiply(other._vector));
        }

        public float DotProduct(IVector vector)
        {
            var other = (CpuVector)vector;
            return _vector.DotProduct(other._vector);
        }

        public IEnumerable<float> Values => _vector.AsEnumerable();

	    public IVector GetNewVectorFromIndexes(IReadOnlyList<int> indexes)
        {
            return new CpuVector(DenseVector.Create(indexes.Count, i => this[indexes[i]]));
        }

        public IVector Abs()
        {
            return new CpuVector(DenseVector.Create(_vector.Count, i => Convert.ToSingle(Math.Abs(_vector[i]))));
        }

        public IVector Sqrt()
        {
            return new CpuVector(DenseVector.Create(_vector.Count, i => Convert.ToSingle(Math.Sqrt(_vector[i] + 1e-8f))));
        }

        public IVector Clone()
        {
            return new CpuVector(DenseVector.OfVector(_vector));
        }

        public float EuclideanDistance(IVector vector)
        {
            var other = (CpuVector)vector;
	        Debug.Assert(other.Count == Count);

	        return Subtract(other).L2Norm();
        }

        public float CosineDistance(IVector vector)
        {
            var other = (CpuVector)vector;
            return Convert.ToSingle(Distance.Cosine(GetInternalArray(), other.GetInternalArray()));
        }

        public float ManhattanDistance(IVector vector)
        {
            var other = (CpuVector)vector;
            return Convert.ToSingle(Distance.Manhattan(_vector, other._vector));
        }

        public float MeanSquaredDistance(IVector vector)
        {
            var other = (CpuVector)vector;
            return Convert.ToSingle(Distance.MSE(_vector, other._vector));
        }

        public float SquaredEuclidean(IVector vector)
        {
            var other = (CpuVector)vector;
            return Convert.ToSingle(Distance.SSD(_vector, other._vector));
        }

        public void CopyFrom(IVector vector)
        {
            var other = (CpuVector)vector;
            other._vector.CopyTo(_vector);
        }

        public (float Min, float Max) GetMinMax()
        {
            float min = 0f, max = 0f;
            foreach (var val in _vector.Enumerate(Zeros.AllowSkip)) {
                if (val > max)
                    max = val;
                if (val < min)
                    min = val;
            }
            return (min, max);
        }

        public float Average()
        {
            return _vector.Average();
        }

        public float StdDev(float? mean)
        {
            var mean2 = mean ?? Average();
            return Convert.ToSingle(Math.Sqrt(_vector.Select(v => Math.Pow(v - mean2, 2)).Average()));
        }

        public void Normalise(NormalizationType type)
        {
            if (type == NormalizationType.FeatureScale) {
                var (min, max) = GetMinMax();
                var range = max - min;
                if (range > 0)
                    _vector.MapInplace(v => (v - min) / range);
            } else if (type == NormalizationType.Standard) {
                var mean = Average();
                var stdDev = StdDev(mean);
                if (BoundMath.IsNotZero(stdDev))
                    _vector.MapInplace(v => (v - mean) / stdDev);
            } else if (type == NormalizationType.Euclidean || type == NormalizationType.Manhattan) {
                var p = (type == NormalizationType.Manhattan) ? 1.0 : 2.0;
                var norm = _vector.Normalize(p);
                norm.CopyTo(_vector);
            }
        }

        public IIndexableVector Append(IReadOnlyList<float> data)
        {
            return new CpuVector(DenseVector.Create(Count + data.Count, i => i < Count ? _vector[i] : data[i - Count]));
        }

        public IVector Softmax()
        {
            var minMax = GetMinMax();
            var max = minMax.Max;

            var softmax = _vector.Map(v => Math.Exp(v - max));
            var sum = softmax.Sum();
            if (BoundMath.IsNotZero(Convert.ToSingle(sum)))
                return new CpuVector(softmax.Divide(sum).ToSingle());
            return new CpuVector(softmax.ToSingle());
        }

        public IMatrix SoftmaxDerivative()
        {
            return new CpuMatrix(DenseMatrix.Build.Dense(Count, Count, (x, y) => x == y 
                ? this[x] * (1 - this[x]) 
                : -this[x] * this[y])
            );
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
            var distanceFunc = _GetDistanceFunc(distance);
            var ret = new float[data.Count];
            Parallel.ForEach(data, (vec, ps, ind) => ret[ind] = distanceFunc(vec));
            return new CpuVector(DenseVector.Create(data.Count, i => ret[i]));
        }

        public float FindDistance(IVector other, DistanceMetric distance)
        {
            return _GetDistanceFunc(distance)(other);
        }

        public IVector CosineDistance(IReadOnlyList<IVector> data, ref float[] dataNorm)
        {
            var norm = DotProduct(this);
            if (dataNorm == null)
                dataNorm = data.Select(d => d.DotProduct(d)).ToArray();

            var ret = new float[data.Count];
            for (var i = 0; i < data.Count; i++)
                ret[i] = Convert.ToSingle(1d - DotProduct(data[i]) / (Math.Sqrt(norm) * Math.Sqrt(dataNorm[i])));
            return new CpuVector(DenseVector.Create(data.Count, i => ret[i]));
        }

        public IVector Log()
        {
            return new CpuVector(_vector.PointwiseLog());
        }

        public IVector Sigmoid()
        {
            return new CpuVector(_vector.Map(CpuMatrix._Sigmoid));
        }

        public void Add(float scalar)
        {
            _vector.MapInplace(v => v + scalar);
        }

        public IMatrix ReshapeAsMatrix(int rows, int columns)
        {
	        Debug.Assert(rows * columns == _vector.Count);
            return new CpuMatrix(DenseMatrix.Build.Dense(rows, columns, GetInternalArray()));
        }

        public I3DTensor ReshapeAs3DTensor(int rows, int columns, int depth)
        {
	        Debug.Assert(rows * columns * depth == _vector.Count);
            if (depth > 1) {
	            var slice = Split(depth);
	            var matrixList = slice.Select(part => part.ReshapeAsMatrix(rows, columns).AsIndexable()).ToList();
	            return new Cpu3DTensor(matrixList);
            } else {
                var matrix = ReshapeAsMatrix(rows, columns).AsIndexable();
                return new Cpu3DTensor(new[] { matrix });
            }
        }

	    public I4DTensor ReshapeAs4DTensor(int rows, int columns, int depth, int count)
	    {
		    Debug.Assert(rows * columns * depth * count == _vector.Count);
		    if (count > 1) {
			    var slice = Split(count);
			    var tensorList = slice.Select(part => part.ReshapeAs3DTensor(rows, columns, depth).AsIndexable()).ToList();
			    return new Cpu4DTensor(tensorList);
		    } else {
			    var tensor = ReshapeAs3DTensor(rows, columns, depth).AsIndexable();
			    return new Cpu4DTensor(new[] { tensor });
		    }
	    }

        public IReadOnlyList<IVector> Split(int blockCount)
        {
            int index = 0;
            float[] curr = null;
            var ret = new List<IVector>();
            var blockSize = Count / blockCount;

            for (int i = 0, len = Count; i < len; i++) {
                if (i % blockSize == 0) {
                    if (curr != null)
                        ret.Add(new CpuVector(curr));
                    curr = new float[blockSize];
                    index = 0;
                }
	            // ReSharper disable once PossibleNullReferenceException
                curr[index++] = _vector[i];
            }
            ret.Add(new CpuVector(curr));
            return ret;
        }

	    public void RotateInPlace(int blockCount)
	    {
		    var blockSize = Count / blockCount;

		    for (int i = 0, len = Count; i < len; i += 2) {
			    int blockIndex = i / blockSize;
			    int blockOffset = i % blockSize;

			    int index1 = blockIndex * blockSize + blockSize - blockOffset - 1;
			    int index2 = blockIndex * blockSize + blockOffset; 
			    var temp = this[index1];
			    this[index1] = this[index2];
			    this[index2] = temp;
		    }
	    }

        public IVector Reverse()
        {
            var len = Count - 1;
            return new CpuVector(DenseVector.Create(Count, i => this[len - i]));
        }

	    public float GetAt(int index)
	    {
		    return _vector[index];
	    }

	    public void SetAt(int index, float value)
	    {
		    _vector[index] = value;
	    }

	    public bool IsEntirelyFinite()
	    {
		    return !_vector.Any(v => float.IsNaN(v) || float.IsInfinity(v));
	    }
    }
}
