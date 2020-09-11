using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BrightData.FloatTensors
{
    public class FloatVector : IIndexableFloatVector
    {
        public FloatVector(Vector<float> data) => Data = data;

        public static Vector<float> Create(IBrightDataContext context, float[] ret) => context.CreateVector(ret);
        public static Vector<float> ReadFrom(IBrightDataContext context, BinaryReader reader) => new Vector<float>(context, reader);

        public void Dispose()
        {
            Data.Dispose();
        }

        public bool IsValid { get; } = true;
        public IFloatMatrix ReshapeAsColumnMatrix() => new FloatMatrix(Data.Reshape(1, Data.Size));
        public IFloatMatrix ReshapeAsRowMatrix() => new FloatMatrix(Data.Reshape(Data.Size, 1));

        public uint Count => Data.Size;
        public Vector<float> Data { get; set; }
        public IFloatVector Add(IFloatVector vector)
        {
            throw new NotImplementedException();
        }

        public IFloatVector Subtract(IFloatVector vector)
        {
            throw new NotImplementedException();
        }

        public float L1Norm()
        {
            throw new NotImplementedException();
        }

        public float L2Norm()
        {
            throw new NotImplementedException();
        }

        public uint MaximumIndex()
        {
            throw new NotImplementedException();
        }

        public uint MinimumIndex()
        {
            throw new NotImplementedException();
        }

        public void Multiply(float scalar)
        {
            throw new NotImplementedException();
        }

        public void Add(float scalar)
        {
            throw new NotImplementedException();
        }

        public void AddInPlace(IFloatVector vector, float coefficient1 = 1, float coefficient2 = 1)
        {
            throw new NotImplementedException();
        }

        public void SubtractInPlace(IFloatVector vector, float coefficient1 = 1, float coefficient2 = 1)
        {
            throw new NotImplementedException();
        }

        public IIndexableFloatVector AsIndexable()
        {
            throw new NotImplementedException();
        }

        public IFloatVector PointwiseMultiply(IFloatVector vector)
        {
            throw new NotImplementedException();
        }

        public float DotProduct(IFloatVector vector)
        {
            throw new NotImplementedException();
        }

        public IFloatVector GetNewVectorFromIndexes(IReadOnlyList<uint> indices)
        {
            throw new NotImplementedException();
        }

        public IFloatVector Clone()
        {
            throw new NotImplementedException();
        }

        public IFloatVector Sqrt()
        {
            throw new NotImplementedException();
        }

        public IFloatVector Abs()
        {
            throw new NotImplementedException();
        }

        public void CopyFrom(IFloatVector vector)
        {
            throw new NotImplementedException();
        }

        public float EuclideanDistance(IFloatVector vector)
        {
            throw new NotImplementedException();
        }

        public float CosineDistance(IFloatVector vector)
        {
            throw new NotImplementedException();
        }

        public float ManhattanDistance(IFloatVector vector)
        {
            throw new NotImplementedException();
        }

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
            throw new NotImplementedException();
        }

        public float Average()
        {
            throw new NotImplementedException();
        }

        public float StdDev(float? mean)
        {
            throw new NotImplementedException();
        }

        public void Normalise(NormalizationType type)
        {
            throw new NotImplementedException();
        }

        public IFloatVector Softmax()
        {
            throw new NotImplementedException();
        }

        public IFloatMatrix SoftmaxDerivative()
        {
            throw new NotImplementedException();
        }

        public IFloatVector FindDistances(IReadOnlyList<IFloatVector> data, DistanceMetric distance)
        {
            throw new NotImplementedException();
        }

        public float FindDistance(IFloatVector other, DistanceMetric distance)
        {
            throw new NotImplementedException();
        }

        public IFloatVector CosineDistance(IReadOnlyList<IFloatVector> data, ref float[] dataNorm)
        {
            throw new NotImplementedException();
        }

        public IFloatVector Log()
        {
            throw new NotImplementedException();
        }

        public IFloatVector Sigmoid()
        {
            throw new NotImplementedException();
        }

        public IFloatMatrix ReshapeAsMatrix(uint rows, uint columns)
        {
            throw new NotImplementedException();
        }

        public I3DFloatTensor ReshapeAs3DTensor(uint rows, uint columns, uint depth)
        {
            throw new NotImplementedException();
        }

        public I4DFloatTensor ReshapeAs4DTensor(uint rows, uint columns, uint depth, uint count)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<IFloatVector> Split(uint blockCount)
        {
            throw new NotImplementedException();
        }

        public void RotateInPlace(uint blockCount = 1)
        {
            throw new NotImplementedException();
        }

        public IFloatVector Reverse()
        {
            throw new NotImplementedException();
        }

        public float GetAt(uint index)
        {
            throw new NotImplementedException();
        }

        public void SetAt(uint index, float value)
        {
            throw new NotImplementedException();
        }

        public bool IsEntirelyFinite()
        {
            throw new NotImplementedException();
        }

        public float this[uint index]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public IEnumerable<float> Values { get; }
        public float[] ToArray()
        {
            throw new NotImplementedException();
        }

        public float[] GetInternalArray()
        {
            throw new NotImplementedException();
        }

        public IIndexableFloatVector Append(IReadOnlyList<float> data)
        {
            throw new NotImplementedException();
        }
    }
}
