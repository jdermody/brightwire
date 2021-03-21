using System;
using System.Collections.Generic;
using System.Linq;
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
            throw new NotImplementedException();
        }

        public float L2Norm()
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public float SquaredEuclidean(IFloatVector vector)
        {
            throw new NotImplementedException();
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
            Data.Segment.InitializeFrom(i => Convert.ToSingle(normalizer.Normalize(this[i])));
        }

        public IFloatVector Softmax() => new FloatVector(Data.Softmax());
        public IFloatMatrix SoftmaxDerivative() => new FloatMatrix(Data.SoftmaxDerivative());

        public IFloatVector FindDistances(IFloatVector[] data, DistanceMetric distance)
        {
            throw new NotImplementedException();
        }

        public float FindDistance(IFloatVector other, DistanceMetric distance)
        {
            throw new NotImplementedException();
        }

        public IFloatVector CosineDistance(IFloatVector[] data, ref float[]? dataNorm)
        {
            throw new NotImplementedException();
        }

        public IFloatVector Log() => new FloatVector(Data.Log());
        public IFloatVector Sigmoid() => new FloatVector(Data.Sigmoid());
        public IFloatMatrix ReshapeAsMatrix(uint rows, uint columns) => new FloatMatrix(Data.Reshape(rows, columns));
        public I3DFloatTensor ReshapeAs3DTensor(uint rows, uint columns, uint depth) => new Float3DTensor(Data.Reshape(depth, rows, columns));
        public I4DFloatTensor ReshapeAs4DTensor(uint rows, uint columns, uint depth, uint count)
        {
            throw new NotImplementedException();
        }

        public IFloatVector[] Split(uint blockCount) => Data
            .Split(blockCount)
            .Select(v => new FloatVector(new Vector<float>(v)))
            .Cast<IFloatVector>()
            .ToArray();

        public void RotateInPlace(uint blockCount = 1)
        {
            throw new NotImplementedException();
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

        public float[] GetInternalArray()
        {
            throw new NotImplementedException();
        }

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
