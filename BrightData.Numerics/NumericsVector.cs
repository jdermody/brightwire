using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BrightData.FloatTensors;
using BrightData.Helper;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

namespace BrightData.Numerics
{
    /// <summary>
    /// Vector that uses the CPU based math.net numerics library
    /// </summary>
    class NumericsVector : IIndexableFloatVector
    {
        readonly MathNet.Numerics.LinearAlgebra.Vector<float> _vector;

        public bool IsValid => true;

	    public NumericsVector(IBrightDataContext context, DenseVector vector)
        {
            Context = context;
            _vector = vector;
        }
        public NumericsVector(IBrightDataContext context, MathNet.Numerics.LinearAlgebra.Vector<float> vector)
        {
            Context = context;
            _vector = vector;
        }

        public void Dispose()
        {
            // nop
        }

        public float this[uint index]
        {
            get => _vector[(int)index];
	        set => _vector[(int)index] = value;
        }

        public IBrightDataContext Context { get; }
        public float[] ToArray() => _vector.ToArray();
	    public float[] GetInternalArray() => _vector.AsArray();
        public uint Count => (uint)_vector.Count;

	    public IFloatVector Add(IFloatVector vector)
        {
            var other = (NumericsVector)vector;
            return new NumericsVector(Context, _vector.Add(other._vector));
        }

        public void AddInPlace(IFloatVector vector, float coefficient1 = 1.0f, float coefficient2 = 1.0f)
        {
            var other = (NumericsVector)vector;
            _vector.MapIndexedInplace((i, v) => (v * coefficient1) + (other[(uint)i] * coefficient2));
        }

        public float L1Norm()
        {
            return Convert.ToSingle(_vector.L1Norm());
        }

        public float L2Norm()
        {
            return Convert.ToSingle(_vector.L2Norm());
        }

        public uint MaximumIndex()
        {
            return (uint)_vector.Map(Math.Abs).MaximumIndex();
        }

        public uint MinimumIndex()
        {
            return (uint)_vector.Map(Math.Abs).MinimumIndex();
        }

        public void Multiply(float scalar)
        {
            _vector.MapInplace(x => x * scalar);
        }

        public IFloatVector Subtract(IFloatVector vector)
        {
            var other = (NumericsVector)vector;
            return new NumericsVector(Context, _vector.Subtract(other._vector));
        }

        public void SubtractInPlace(IFloatVector vector, float coefficient1 = 1.0f, float coefficient2 = 1.0f)
        {
            var other = (NumericsVector)vector;
            _vector.MapIndexedInplace((i, v) => (v * coefficient1) - (other[(uint)i] * coefficient2));
        }

        public IFloatMatrix ReshapeAsColumnMatrix()
        {
            return new NumericsMatrix(Context, DenseMatrix.Build.Dense(_vector.Count, 1, GetInternalArray()));
        }

        public IFloatMatrix ReshapeAsRowMatrix()
        {
            return new NumericsMatrix(Context, DenseMatrix.Build.Dense(1, _vector.Count, GetInternalArray()));
        }

        public override string ToString()
        {
            return _vector.ToVectorString();
        }

        public BrightData.Vector<float> Data
        {
            get => FloatVector.Create(Context, _vector.ToArray());

	        set
            {
                if (value.Segment != null) {
                    var data = value.Segment;
                    for (uint i = 0, len = data.Size; i < len; i++)
                        _vector[(int)i] = data[i];
                }
            }
        }

        public IIndexableFloatVector AsIndexable()
        {
            return this;
        }

        public IFloatVector PointwiseMultiply(IFloatVector vector)
        {
            var other = (NumericsVector)vector;
            return new NumericsVector(Context, _vector.PointwiseMultiply(other._vector));
        }

        public float DotProduct(IFloatVector vector)
        {
            var other = (NumericsVector)vector;
            return _vector.DotProduct(other._vector);
        }

        public IEnumerable<float> Values => _vector.AsEnumerable();

	    public IFloatVector GetNewVectorFromIndexes(IEnumerable<uint> indexes)
        {
            var data = indexes.Select(i => this[i]).ToArray();
            return new NumericsVector(Context, DenseVector.Create(data.Length, i => data[i]));
        }

        public IFloatVector Abs()
        {
            return new NumericsVector(Context, DenseVector.Create(_vector.Count, i => Convert.ToSingle(Math.Abs(_vector[i]))));
        }

        public IFloatVector Sqrt()
        {
            return new NumericsVector(Context, DenseVector.Create(_vector.Count, i => Convert.ToSingle(Math.Sqrt(_vector[i] + 1e-8f))));
        }

        public IFloatVector Clone()
        {
            return new NumericsVector(Context, DenseVector.OfVector(_vector));
        }

        public float EuclideanDistance(IFloatVector vector)
        {
            var other = (NumericsVector)vector;
	        Debug.Assert(other.Count == Count);

	        return Subtract(other).L2Norm();
        }

        public float CosineDistance(IFloatVector vector)
        {
            var other = (NumericsVector)vector;
            return Convert.ToSingle(MathNet.Numerics.Distance.Cosine(GetInternalArray(), other.GetInternalArray()));
        }

        public float ManhattanDistance(IFloatVector vector)
        {
            var other = (NumericsVector)vector;
            return Convert.ToSingle(MathNet.Numerics.Distance.Manhattan(_vector, other._vector));
        }

        public float MeanSquaredDistance(IFloatVector vector)
        {
            var other = (NumericsVector)vector;
            return Convert.ToSingle(MathNet.Numerics.Distance.MSE(_vector, other._vector));
        }

        public float SquaredEuclidean(IFloatVector vector)
        {
            var other = (NumericsVector)vector;
            return Convert.ToSingle(MathNet.Numerics.Distance.SSD(_vector, other._vector));
        }

        public void CopyFrom(IFloatVector vector)
        {
            var other = (NumericsVector)vector;
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
                if (FloatMath.IsNotZero(stdDev))
                    _vector.MapInplace(v => (v - mean) / stdDev);
            } else if (type == NormalizationType.Euclidean || type == NormalizationType.Manhattan) {
                var p = (type == NormalizationType.Manhattan) ? 1.0 : 2.0;
                var norm = _vector.Normalize(p);
                norm.CopyTo(_vector);
            }
        }

        public IIndexableFloatVector Append(params float[] data)
        {
            var count = (int)Count;
            return new NumericsVector(Context, DenseVector.Create(count + data.Length, i => i < Count ? _vector[i] : data[i - count]));
        }

        public IFloatVector Softmax()
        {
            var minMax = GetMinMax();
            var max = minMax.Max;

            var softmax = _vector.Map(v => Math.Exp(v - max));
            var sum = softmax.Sum();
            if (FloatMath.IsNotZero(Convert.ToSingle(sum)))
                return new NumericsVector(Context, softmax.Divide(sum).ToSingle());
            return new NumericsVector(Context, softmax.ToSingle());
        }

        public IFloatMatrix SoftmaxDerivative()
        {
            var count = (int)Count;
            return new NumericsMatrix(Context, DenseMatrix.Build.Dense(count, count, (x, y) => x == y 
                ? this[(uint)x] * (1 - this[(uint)x]) 
                : -this[(uint)x] * this[(uint)y])
            );
        }

        Func<IFloatVector, float> _GetDistanceFunc(DistanceMetric distance)
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

        public IFloatVector FindDistances(IFloatVector[] data, DistanceMetric distance)
        {
            var distanceFunc = _GetDistanceFunc(distance);
            var ret = new float[data.Length];
            Parallel.ForEach(data, (vec, ps, ind) => ret[ind] = distanceFunc(vec));
            return new NumericsVector(Context, DenseVector.Create(data.Length, i => ret[i]));
        }

        public float FindDistance(IFloatVector other, DistanceMetric distance)
        {
            return _GetDistanceFunc(distance)(other);
        }

        public IFloatVector CosineDistance(IFloatVector[] data, ref float[] dataNorm)
        {
            var norm = DotProduct(this);
            dataNorm ??= data.Select(d => d.DotProduct(d)).ToArray();

            var ret = new float[data.Length];
            for (var i = 0; i < data.Length; i++)
                ret[i] = Convert.ToSingle(1d - DotProduct(data[i]) / (Math.Sqrt(norm) * Math.Sqrt(dataNorm[i])));
            return new NumericsVector(Context, DenseVector.Create(data.Length, i => ret[i]));
        }

        public IFloatVector Log()
        {
            return new NumericsVector(Context, _vector.PointwiseLog());
        }

        public IFloatVector Sigmoid()
        {
            return new NumericsVector(Context, _vector.Map(NumericsMatrix._Sigmoid));
        }

        public void Add(float scalar)
        {
            _vector.MapInplace(v => v + scalar);
        }

        public IFloatMatrix ReshapeAsMatrix(uint rows, uint columns)
        {
	        Debug.Assert(rows * columns == _vector.Count);
            return new NumericsMatrix(Context, DenseMatrix.Build.Dense((int)rows, (int)columns, GetInternalArray()));
        }

        public I3DFloatTensor ReshapeAs3DTensor(uint rows, uint columns, uint depth)
        {
	        Debug.Assert(rows * columns * depth == _vector.Count);
            if (depth > 1) {
	            var slice = Split(depth);
	            var matrixList = slice.Select(part => part.ReshapeAsMatrix(rows, columns).AsIndexable()).ToArray();
	            return new Numerics3DTensor(Context, matrixList);
            } else {
                var matrix = ReshapeAsMatrix(rows, columns).AsIndexable();
                return new Numerics3DTensor(Context, new[] { matrix });
            }
        }

	    public I4DFloatTensor ReshapeAs4DTensor(uint rows, uint columns, uint depth, uint count)
	    {
		    Debug.Assert(rows * columns * depth * count == _vector.Count);
		    if (count > 1) {
			    var slice = Split(count);
			    var tensorList = slice.Select(part => part.ReshapeAs3DTensor(rows, columns, depth).AsIndexable()).ToArray();
			    return new Numerics4DTensor(Context, tensorList);
		    } else {
			    var tensor = ReshapeAs3DTensor(rows, columns, depth).AsIndexable();
			    return new Numerics4DTensor(Context, new[] { tensor });
		    }
	    }

        public IFloatVector[] Split(uint blockCount)
        {
            int index = 0, blockIndex = 0;
            float[] curr = null;
            var ret = new IFloatVector[blockCount];
            var blockSize = Count / blockCount;

            for (uint i = 0, len = Count; i < len; i++) {
                if (i % blockSize == 0) {
                    if (curr != null)
                        ret[blockIndex++] = new NumericsVector(Context, curr);
                    curr = new float[blockSize];
                    index = 0;
                }
	            // ReSharper disable once PossibleNullReferenceException
                curr[index++] = _vector[(int)i];
            }
            ret[blockIndex] = new NumericsVector(Context, curr);
            return ret;
        }

	    public void RotateInPlace(uint blockCount)
	    {
		    var blockSize = Count / blockCount;

		    for (uint i = 0, len = Count; i < len; i += 2) {
			    var blockIndex = i / blockSize;
			    var blockOffset = i % blockSize;

			    var index1 = blockIndex * blockSize + blockSize - blockOffset - 1;
			    var index2 = blockIndex * blockSize + blockOffset; 
			    var temp = this[index1];
			    this[index1] = this[index2];
			    this[index2] = temp;
		    }
	    }

        public IFloatVector Reverse()
        {
            var len = Count - 1;
            return new NumericsVector(Context, DenseVector.Create((int)Count, i => this[len - (uint)i]));
        }

	    public float GetAt(uint index)
	    {
		    return _vector[(int)index];
	    }

	    public void SetAt(uint index, float value)
	    {
		    _vector[(int)index] = value;
	    }

	    public bool IsEntirelyFinite()
	    {
		    return !_vector.Any(v => float.IsNaN(v) || float.IsInfinity(v));
	    }

        public ILinearAlgebraProvider LinearAlgebraProvider => Context.LinearAlgebraProvider;
    }
}
