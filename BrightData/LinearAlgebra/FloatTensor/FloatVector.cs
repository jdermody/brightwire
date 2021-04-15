using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BrightData.Analysis;
using BrightData.Transformation;

namespace BrightData.LinearAlgebra.FloatTensor
{
    internal class FloatVector : IIndexableFloatVector
    {
        public FloatVector(Vector<float> data) => Data = data;

        public void Dispose()
        {
            Data.Dispose();
        }

        public bool IsValid { get; } = true;
        public IFloatMatrix ReshapeAsColumnMatrix() => new FloatMatrix(Data.Reshape(1, Data.Size));
        public IFloatMatrix ReshapeAsRowMatrix() => new FloatMatrix(Data.Reshape(Data.Size, 1));

        public uint Count => Data.Size;
        public Vector<float> Data { get; set; }
        public IFloatVector Add(IFloatVector vector) => new FloatVector(Data.Add(vector.Data));
        public IFloatVector Subtract(IFloatVector vector) => new FloatVector(Data.Subtract(vector.Data));
        public float L1Norm()
        {
            using var abs = Data.Abs();
            return abs.Sum();
        }

        public float L2Norm()
        {
            using var squared = Data.PointwiseMultiply(Data);
            return MathF.Sqrt(squared.Sum());
        }

        public uint MaximumIndex() => Data.MaximumIndex();
        public uint MinimumIndex() => Data.MinimumIndex();
        public void Multiply(float scalar) => Data.Multiply(scalar);
        public void Add(float scalar) => Data.AddInPlace(scalar);
        public void AddInPlace(IFloatVector vector, float coefficient1 = 1, float coefficient2 = 1) => Data.AddInPlace(vector.Data, coefficient1, coefficient2);
        public void SubtractInPlace(IFloatVector vector, float coefficient1 = 1, float coefficient2 = 1) => Data.SubtractInPlace(vector.Data, coefficient1, coefficient2);
        public IIndexableFloatVector AsIndexable() => this;
        public IFloatVector PointwiseMultiply(IFloatVector vector) => new FloatVector(Data.PointwiseMultiply(vector.Data));
        public float DotProduct(IFloatVector vector) => Data.DotProduct(vector.Data);
        public IFloatVector GetNewVectorFromIndexes(IEnumerable<uint> indices) => new FloatVector(Data.Context.CreateVector(indices.Select(i => Data[i]).ToArray()));
        public IFloatVector Clone() => new FloatVector(Data.Clone());
        public IFloatVector Sqrt() => new FloatVector(Data.Sqrt());
        public IFloatVector Abs() => new FloatVector(Data.Abs());
        public void CopyFrom(IFloatVector vector) => Data.CopyFrom(vector.AsIndexable().ToArray());
        public float EuclideanDistance(IFloatVector vector) => Data.EuclideanDistance(vector.Data);
        public float CosineDistance(IFloatVector vector) => Data.CosineDistance(vector.Data);
        public float ManhattanDistance(IFloatVector vector) => Data.ManhattanDistance(vector.Data);
        public float MeanSquaredDistance(IFloatVector vector)
        {
            using var diff = Data.Subtract(vector.Data);
            var num = diff.L2Norm();
            return num * num / Count;
        }

        public float SquaredEuclidean(IFloatVector vector)
        {
            using var diff = Data.Subtract(vector.Data);
            var num = diff.L2Norm();
            return num * num;
        }

        public (float Min, float Max) GetMinMax()
        {
            var (min, max, _, _) = Data.GetMinAndMaxValues();
            return (min, max);
        }

        public float Average() => Data.Average();
        public float StdDev(float? mean) => Data.StdDev(mean);

        public void Normalize(NormalizationType type)
        {
            // analyse the data
            var analyser = new NumericAnalyser();
            foreach (var value in Data.Values)
                analyser.Add(value);

            // write the results
            var metaData = new MetaData();
            analyser.WriteTo(metaData);
            
            // normalize
            var normalizer = new NormalizeTransformation(type, metaData);
            Data.Segment.Initialize(i => Convert.ToSingle(normalizer.Normalize(this[i])));
        }

        public IFloatVector Softmax() => new FloatVector(Data.Softmax());
        public IFloatMatrix SoftmaxDerivative() => new FloatMatrix(Data.SoftmaxDerivative());

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
            var distanceFunc = GetDistanceFunc(distance);
            var ret = new float[data.Length];
            Parallel.ForEach(data, (vec, ps, ind) => ret[ind] = distanceFunc(vec));
            return new FloatVector(Data.Context.CreateVector(ret));
        }

        public float FindDistance(IFloatVector other, DistanceMetric distance) => GetDistanceFunc(distance)(other);

        IFloatVector IFloatVector.CosineDistance(IFloatVector[] data, ref float[]? dataNorm)
        {
            var norm = Data.DotProduct(Data);
            dataNorm ??= data.Select(d => d.Data.DotProduct(d.Data)).ToArray();

            var ret = new float[data.Length];
            for (var i = 0; i < data.Length; i++)
                ret[i] = 1f - DotProduct(data[i]) / (MathF.Sqrt(norm) * MathF.Sqrt(dataNorm[i]));
            return new FloatVector(Data.Context.CreateVector(ret));
        }

        public IFloatVector Log() => new FloatVector(Data.Log());
        public IFloatVector Sigmoid() => new FloatVector(Data.Sigmoid());
        public IFloatMatrix ReshapeAsMatrix(uint rows, uint columns) => new FloatMatrix(Data.Reshape(rows, columns));

        public I3DFloatTensor ReshapeAs3DTensor(uint rows, uint columns, uint depth)
        {
            Debug.Assert(rows * columns * depth == Count);
            if (depth > 1) {
                var slice = Split(depth);
                var matrixList = slice.Select(part => part.ReshapeAsMatrix(rows, columns).Data).ToArray();
                return new Float3DTensor(Data.Context.CreateTensor3D(matrixList));
            }
            var matrix = ReshapeAsMatrix(rows, columns).Data;
            return new Float3DTensor(Data.Context.CreateTensor3D(new[] { matrix }));
        }

        public I4DFloatTensor ReshapeAs4DTensor(uint rows, uint columns, uint depth, uint count)
        {
            Debug.Assert(rows * columns * depth * count == Count);
            if (count > 1) {
                var slice = Split(count);
                var tensorList = slice.Select(part => part.ReshapeAs3DTensor(rows, columns, depth).Data).ToArray();
                return new Float4DTensor(tensorList);
            }
            var tensor = ReshapeAs3DTensor(rows, columns, depth).Data;
            return new Float4DTensor(new[] { tensor });
        }

        public IFloatVector[] Split(uint blockCount) => Data
            .Split(blockCount)
            .Select(v => new FloatVector(new Vector<float>(v)))
            .Cast<IFloatVector>()
            .ToArray();

        public void RotateInPlace(uint blockCount = 1)
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

        public IFloatVector Reverse() => new FloatVector(Data.Reverse());
        public float GetAt(uint index) => Data[index];
        public void SetAt(uint index, float value) => Data[index] = value;
        public bool IsEntirelyFinite() => Data.IsEntirelyFinite();
        public void RoundInPlace(float lower = 0f, float upper = 1f, float mid = 0.5f) => Data.RoundInPlace(lower, upper, mid);

        public float this[uint index]
        {
            get => Data[index];
            set => Data[index] = value;
        }

        public IEnumerable<float> Values => Data.Values;
        public float[] ToArray() => Data.ToArray();

        public IIndexableFloatVector Append(float[] data)
        {
            var segment = Data.Context.CreateSegment<float>(Count + (uint) data.Length);
            Data.Segment.CopyTo(segment);
            uint index = 0;
            foreach (var item in data)
                segment[Count + index++] = item;
            return new FloatVector(new Vector<float>(segment));
        }

        public override string ToString() => $"Simple: {Data}";

        public ILinearAlgebraProvider LinearAlgebraProvider => Data.Context.LinearAlgebraProvider;
    }
}
