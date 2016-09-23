using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightWire.Models;
using BrightWire.Helper;
using System.Threading.Tasks;

namespace BrightWire.LinearAlgebra
{
    public class CpuVector : IIndexableVector
    {
        readonly Vector<float> _vector;

        public bool IsValid { get { return true; } }

        public CpuVector(DenseVector vector)
        {
            _vector = vector;
        }
        public CpuVector(Vector<float> vector)
        {
            _vector = vector;
        }

        protected virtual void Dispose(bool disposing)
        {
            // nop
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public float this[int index]
        {
            get
            {
                return _vector[index];
            }

            set
            {
                _vector[index] = value;
            }
        }

        public float[] ToArray()
        {
            return _vector.ToArray();
        }

        public int Count
        {
            get
            {
                return _vector.Count;
            }
        }

        public object WrappedObject
        {
            get
            {
                return _vector;
            }
        }

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
            return _vector.Map(v => Math.Abs(v)).MaximumIndex();
        }

        public int MinimumIndex()
        {
            return _vector.Map(v => Math.Abs(v)).MinimumIndex();
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

        public IMatrix ToColumnMatrix(int numCols = 1)
        {
            return new CpuMatrix(DenseMatrix.Create(_vector.Count, numCols, (x, y) => _vector[x]));
        }

        public IMatrix ToRowMatrix(int numRows = 1)
        {
            return new CpuMatrix(DenseMatrix.Create(numRows, _vector.Count, (x, y) => _vector[y]));
        }

        public override string ToString()
        {
            return _vector.ToVectorString();
        }

        public FloatArray Data
        {
            get
            {
                return new FloatArray {
                    Data = _vector.ToArray()
                };
            }

            set
            {
                if(value.Data != null) {
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

        public IEnumerable<float> Values
        {
            get
            {
                return _vector.AsEnumerable();
            }
        }

        public IVector GetNewVectorFromIndexes(int[] indexes)
        {
            return new CpuVector(DenseVector.Create(indexes.Length, i => this[indexes[i]]));
        }

        public IVector Abs()
        {
            return new CpuVector(DenseVector.Create(_vector.Count, i => Convert.ToSingle(Math.Abs(_vector[i]))));
        }

        public IVector Sqrt()
        {
            return new CpuVector(DenseVector.Create(_vector.Count, i => Convert.ToSingle(Math.Sqrt(_vector[i]))));
        }

        public void ReplaceIndexedValues(IVector vector, int[] indexes)
        {
            var other = (CpuVector)vector;
            for (var i = 0; i < indexes.Length; i++) {
                this[indexes[i]] = other[i];
            }
        }

        public IVector Clone()
        {
            return new CpuVector(DenseVector.OfVector(_vector));
        }

        public float EuclideanDistance(IVector vector)
        {
            var other = (CpuVector)vector;
            return Convert.ToSingle(Distance.Euclidean(_vector, other._vector));
        }

        public float CosineDistance(IVector vector)
        {
            var other = (CpuVector)vector;
            return Convert.ToSingle(Distance.Cosine(_vector.ToArray(), other._vector.ToArray()));
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

        public Tuple<float, float> GetMinMax()
        {
            float min = 0f, max = 0f;
            foreach (var val in _vector.Enumerate(Zeros.AllowSkip)) {
                if (val > max)
                    max = val;
                if (val < min)
                    min = val;
            }
            return Tuple.Create(min, max);
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

        public void Normalise(NormalisationType type)
        {
            if (type == NormalisationType.FeatureScale) {
                var minMax = GetMinMax();
                float range = minMax.Item2 - minMax.Item1;
                if (range > 0)
                    _vector.MapInplace(v => (v - minMax.Item1) / range);
            }
            else if(type == NormalisationType.Standard) {
                var mean = Average();
                var stdDev = StdDev(mean);
                if (stdDev != 0)
                    _vector.MapInplace(v => (v - mean) / stdDev);
            }
            else if (type == NormalisationType.Euclidean || type == NormalisationType.Manhattan) {
                var p = (type == NormalisationType.Manhattan) ? 1.0 : 2.0;
                var norm = _vector.Normalize(p);
                norm.CopyTo(_vector);
            }
        }

        public IIndexableVector Append(IReadOnlyList<float> data)
        {
            return new CpuVector(DenseVector.Create(Count + data.Count, i => i < Count ? _vector[i] : data[i-Count]));
        }

        public IVector Softmax()
        {
            var minMax = GetMinMax();
            var max = minMax.Item2;

            var softmax = _vector.Map(v => BoundMath.Exp(v - max));
            var sum = softmax.Sum();
            if(sum != 0)
                return new CpuVector(softmax.Divide(sum));
            return new CpuVector(softmax);
        }

        Func<IVector, float> _GetDistanceFunc(DistanceMetric distance)
        {
            switch(distance) {
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

        public IVector CosineDistance(IReadOnlyList<IVector> data, ref float[] dataNorm)
        {
            var norm = DotProduct(this);
            if (dataNorm == null)
                dataNorm = data.Select(d => d.DotProduct(d)).ToArray();

            var ret = new float[data.Count];
            for (var i = 0; i < data.Count; i++)
                ret[i] = Convert.ToSingle(1d - DotProduct(data[i]) / Math.Sqrt(norm * dataNorm[i]));
            return new CpuVector(DenseVector.Create(data.Count, i => ret[i]));
        }
    }
}
