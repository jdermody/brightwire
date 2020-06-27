using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BrightData.Helper;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

namespace BrightData.Numerics
{
    public class NumericsVector : ITensor<float>, ITensorSegment<float>
    {
        readonly MathNet.Numerics.LinearAlgebra.Vector<float> _vector;
        private uint _size;

        uint IReferenceCountedMemory.Size => _size;

        public int AddRef() => 1; // nop
        public int Release() => 1; // nop

        public long AllocationIndex => 1; // nop
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

        public int Size => _vector.Count;

        public float this[int index]
        {
            get => _vector[index];
            set => _vector[index] = value;
        }

        public IBrightDataContext Context { get; }
        public (ITensorBlock<float> Block, bool IsNewCopy) GetBlock(ITensorPool pool)
        {
            throw new NotImplementedException();
        }

        public float this[uint index]
        {
            get => _vector[(int)index];
            set => _vector[(int)index] = value;
        }

        public float this[long index]
        {
            get => _vector[(int)index];
            set => _vector[(int)index] = value;
        }

        public float[] ToArray() => _vector.ToArray();
        public float[] GetInternalArray() => _vector.AsArray();
        public int Count => _vector.Count;

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

        //public Vector<float> ToVector(IBrightDataContext context)
        //{
        //    return context.CreateVector((uint)_vector.Count, i => _vector[(int)i]);
        //}

        //public IComputableVector PointwiseMultiply(IComputableVector vector)
        //{
        //    var other = (NumericsVector)vector;
        //    return new NumericsVector(_vector.PointwiseMultiply(other._vector));
        //}

        //public float DotProduct(IComputableVector vector)
        //{
        //    var other = (NumericsVector)vector;
        //    return _vector.DotProduct(other._vector);
        //}

        public IEnumerable<float> Values => _vector.AsEnumerable();
        public void InitializeFrom(Stream stream)
        {
            throw new NotImplementedException();
        }

        public void Initialize(Func<uint, float> initializer)
        {
            _vector.MapIndexedInplace((i, val) => initializer((uint) i));
        }

        public void Initialize(float initializer)
        {
            _vector.MapInplace(v => initializer);
        }

        public void Initialize(float[] initialData)
        {
            _vector.MapIndexedInplace((i, v) => initialData[i]);
        }

        public void WriteTo(Stream writerBaseStream)
        {
            throw new NotImplementedException();
        }

        //public IComputableVector GetNewVectorFromIndexes(IReadOnlyList<int> indexes)
        //{
        //    return new NumericsVector(DenseVector.Create(indexes.Count, i => this[indexes[i]]));
        //}


        

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

        //public IComputableVector CosineDistance(IReadOnlyList<IComputableVector> data, ref float[] dataNorm)
        //{
        //    var norm = DotProduct(this);
        //    if (dataNorm == null)
        //        dataNorm = data.Select(d => d.DotProduct(d)).ToArray();

        //    var ret = new float[data.Count];
        //    for (var i = 0; i < data.Count; i++)
        //        ret[i] = Convert.ToSingle(1d - DotProduct(data[i]) / (Math.Sqrt(norm) * Math.Sqrt(dataNorm[i])));
        //    return new NumericsVector(DenseVector.Create(data.Count, i => ret[i]));
        //}

        public float GetAt(int index)
        {
            return _vector[index];
        }

        public void SetAt(int index, float value)
        {
            _vector[index] = value;
        }

        public uint[] Shape { get; }
        public ITensorSegment<float> GetDataCopy()
        {
            throw new NotImplementedException();
        }

        public ITensorSegment<float> Data { get; }
        public INumericComputation<float> Computation { get; }
    }
}
