using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BrightData.Helper;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

namespace BrightData.Numerics
{
    public class NumericsVector : IComputableVector
    {
        readonly MathNet.Numerics.LinearAlgebra.Vector<float> _vector;

        public bool IsValid => true;

        public NumericsVector(DenseVector vector)
        {
            _vector = vector;
        }
        public NumericsVector(MathNet.Numerics.LinearAlgebra.Vector<float> vector)
        {
            _vector = vector;
        }

        public void Dispose()
        {
            // nop
        }

        public int Size => _vector.Count;

        public float this[int index]
        {
            get => _vector[index];
            set => _vector[index] = value;
        }

        public float[] ToArray() => _vector.ToArray();
        public float[] GetInternalArray() => _vector.AsArray();
        public int Count => _vector.Count;

        public IComputableVector Add(IComputableVector vector)
        {
            var other = (NumericsVector)vector;
            return new NumericsVector(_vector.Add(other._vector));
        }

        public void AddInPlace(IComputableVector vector, float coefficient1 = 1.0f, float coefficient2 = 1.0f)
        {
            var other = (NumericsVector)vector;
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

        public void MultiplyInPlace(float scalar)
        {
            _vector.MapInplace(x => x * scalar);
        }

        public IComputableVector Subtract(IComputableVector vector)
        {
            var other = (NumericsVector)vector;
            return new NumericsVector(_vector.Subtract(other._vector));
        }

        public void SubtractInPlace(IComputableVector vector, float coefficient1 = 1.0f, float coefficient2 = 1.0f)
        {
            var other = (NumericsVector)vector;
            _vector.MapIndexedInplace((i, v) => (v * coefficient1) - (other[i] * coefficient2));
        }

        public IComputableMatrix ReshapeAsSingleColumnMatrix()
        {
            return new NumericsMatrix(DenseMatrix.Build.Dense(_vector.Count, 1, GetInternalArray()));
        }

        public IComputableMatrix ReshapeAsSingleRowMatrix()
        {
            return new NumericsMatrix(DenseMatrix.Build.Dense(1, _vector.Count, GetInternalArray()));
        }

        public override string ToString()
        {
            return _vector.ToVectorString();
        }

        //public Vector<float> Data
        //{
        //    get => new FloatVector {
        //        Data = _vector.ToArray()
        //    };

        //    set {
        //        if (value.Data != null) {
        //            var data = value.Data;
        //            for (int i = 0, len = data.Length; i < len; i++)
        //                _vector[i] = data[i];
        //        }
        //    }
        //}

        public Vector<float> ToVector(IBrightDataContext context)
        {
            return context.CreateVector((uint)_vector.Count, i => _vector[(int)i]);
        }

        public IComputableVector PointwiseMultiply(IComputableVector vector)
        {
            var other = (NumericsVector)vector;
            return new NumericsVector(_vector.PointwiseMultiply(other._vector));
        }

        public float DotProduct(IComputableVector vector)
        {
            var other = (NumericsVector)vector;
            return _vector.DotProduct(other._vector);
        }

        public IEnumerable<float> Values => _vector.AsEnumerable();

        public IComputableVector GetNewVectorFromIndexes(IReadOnlyList<int> indexes)
        {
            return new NumericsVector(DenseVector.Create(indexes.Count, i => this[indexes[i]]));
        }

        public IComputableVector Abs()
        {
            return new NumericsVector(DenseVector.Create(_vector.Count, i => Convert.ToSingle(Math.Abs(_vector[i]))));
        }

        public IComputableVector Sqrt()
        {
            return new NumericsVector(DenseVector.Create(_vector.Count, i => Convert.ToSingle(Math.Sqrt(_vector[i] + 1e-8f))));
        }

        public IComputableVector Clone()
        {
            return new NumericsVector(DenseVector.OfVector(_vector));
        }

        public float EuclideanDistance(IComputableVector vector)
        {
            var other = (NumericsVector)vector;
            Debug.Assert(other.Count == Count);

            return Subtract(other).L2Norm();
        }

        public float CosineDistance(IComputableVector vector)
        {
            var other = (NumericsVector)vector;
            return Convert.ToSingle(MathNet.Numerics.Distance.Cosine(GetInternalArray(), other.GetInternalArray()));
        }

        public float ManhattanDistance(IComputableVector vector)
        {
            var other = (NumericsVector)vector;
            return Convert.ToSingle(MathNet.Numerics.Distance.Manhattan(_vector, other._vector));
        }

        public float MeanSquaredDistance(IComputableVector vector)
        {
            var other = (NumericsVector)vector;
            return Convert.ToSingle(MathNet.Numerics.Distance.MSE(_vector, other._vector));
        }

        public float SquaredEuclidean(IComputableVector vector)
        {
            var other = (NumericsVector)vector;
            return Convert.ToSingle(MathNet.Numerics.Distance.SSD(_vector, other._vector));
        }

        public void CopyFrom(IComputableVector vector)
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

        //public void Normalise(NormalisationType type)
        //{
        //    if (type == NormalisationType.FeatureScale) {
        //        var (min, max) = GetMinMax();
        //        var range = max - min;
        //        if (range > 0)
        //            _vector.MapInplace(v => (v - min) / range);
        //    } else if (type == NormalisationType.Standard) {
        //        var mean = Average();
        //        var stdDev = StdDev(mean);
        //        if (BoundMath.IsNotZero(stdDev))
        //            _vector.MapInplace(v => (v - mean) / stdDev);
        //    } else if (type == NormalisationType.Euclidean || type == NormalisationType.Manhattan) {
        //        var p = (type == NormalisationType.Manhattan) ? 1.0 : 2.0;
        //        var norm = _vector.Normalize(p);
        //        norm.CopyTo(_vector);
        //    }
        //}

        //public IIndexableVector Append(IReadOnlyList<float> data)
        //{
        //    return new NumericsVector(DenseVector.Create(Count + data.Count, i => i < Count ? _vector[i] : data[i - Count]));
        //}

        public IComputableVector Softmax()
        {
            var minMax = GetMinMax();
            var max = minMax.Max;

            var softmax = _vector.Map(v => Math.Exp(v - max));
            var sum = softmax.Sum();
            if (FloatMath.IsNotZero(Convert.ToSingle(sum)))
                return new NumericsVector(softmax.Divide(sum).ToSingle());
            return new NumericsVector(softmax.ToSingle());
        }

        public IComputableMatrix SoftmaxDerivative()
        {
            return new NumericsMatrix(DenseMatrix.Build.Dense(Count, Count, (x, y) => x == y
                ? this[x] * (1 - this[x])
                : -this[x] * this[y])
            );
        }

        //Func<IComputableVector, float> _GetDistanceFunc(DistanceMetric distance)
        //{
        //    switch (distance) {
        //        case DistanceMetric.Cosine:
        //            return CosineDistance;
        //        case DistanceMetric.Euclidean:
        //            return EuclideanDistance;
        //        case DistanceMetric.Manhattan:
        //            return ManhattanDistance;
        //        case DistanceMetric.MeanSquared:
        //            return MeanSquaredDistance;
        //        case DistanceMetric.SquaredEuclidean:
        //            return SquaredEuclidean;
        //    }
        //    throw new NotImplementedException();
        //}

        //public IComputableVector FindDistances(IReadOnlyList<IComputableVector> data, DistanceMetric distance)
        //{
        //    var distanceFunc = _GetDistanceFunc(distance);
        //    var ret = new float[data.Count];
        //    Parallel.ForEach(data, (vec, ps, ind) => ret[ind] = distanceFunc(vec));
        //    return new NumericsVector(DenseVector.Create(data.Count, i => ret[i]));
        //}

        //public float FindDistance(IComputableVector other, DistanceMetric distance)
        //{
        //    return _GetDistanceFunc(distance)(other);
        //}

        public IComputableVector CosineDistance(IReadOnlyList<IComputableVector> data, ref float[] dataNorm)
        {
            var norm = DotProduct(this);
            if (dataNorm == null)
                dataNorm = data.Select(d => d.DotProduct(d)).ToArray();

            var ret = new float[data.Count];
            for (var i = 0; i < data.Count; i++)
                ret[i] = Convert.ToSingle(1d - DotProduct(data[i]) / (Math.Sqrt(norm) * Math.Sqrt(dataNorm[i])));
            return new NumericsVector(DenseVector.Create(data.Count, i => ret[i]));
        }

        public IComputableVector Log()
        {
            return new NumericsVector(_vector.PointwiseLog());
        }

        public IComputableVector Sigmoid()
        {
            return new NumericsVector(_vector.Map(NumericsMatrix._Sigmoid));
        }

        public void AddInPlace(float scalar)
        {
            _vector.MapInplace(v => v + scalar);
        }

        public IComputableMatrix ReshapeAsMatrix(int rows, int columns)
        {
            Debug.Assert(rows * columns == _vector.Count);
            return new NumericsMatrix(DenseMatrix.Build.Dense(rows, columns, GetInternalArray()));
        }

        public IComputable3DTensor ReshapeAs3DTensor(int rows, int columns, int depth)
        {
            Debug.Assert(rows * columns * depth == _vector.Count);
            if (depth > 1) {
                var slice = Split(depth);
                var matrixList = slice.Select(part => part.ReshapeAsMatrix(rows, columns)).ToList();
                return new Numerics3DTensor(matrixList);
            } else {
                var matrix = ReshapeAsMatrix(rows, columns);
                return new Numerics3DTensor(new[] { matrix });
            }
        }

        public IComputable4DTensor ReshapeAs4DTensor(int rows, int columns, int depth, int count)
        {
            Debug.Assert(rows * columns * depth * count == _vector.Count);
            if (count > 1) {
                var slice = Split(count);
                var tensorList = slice.Select(part => part.ReshapeAs3DTensor(rows, columns, depth)).ToList();
                return new Numerics4DTensor(tensorList);
            } else {
                var tensor = ReshapeAs3DTensor(rows, columns, depth);
                return new Numerics4DTensor(new[] { tensor });
            }
        }

        public IReadOnlyList<IComputableVector> Split(int blockCount)
        {
            int index = 0;
            float[] curr = null;
            var ret = new List<IComputableVector>();
            var blockSize = Count / blockCount;

            for (int i = 0, len = Count; i < len; i++) {
                if (i % blockSize == 0) {
                    if (curr != null)
                        ret.Add(new NumericsVector(curr));
                    curr = new float[blockSize];
                    index = 0;
                }
                // ReSharper disable once PossibleNullReferenceException
                curr[index++] = _vector[i];
            }
            ret.Add(new NumericsVector(curr));
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

        public IComputableVector Reverse()
        {
            var len = Count - 1;
            return new NumericsVector(DenseVector.Create(Count, i => this[len - i]));
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
