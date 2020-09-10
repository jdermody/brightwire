using BrightData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BrightWire.Models
{
    public class FloatVector : IIndexableFloatVector
    {
        public static Vector<float> Create(float[] ret)
        {
            throw new NotImplementedException();
        }

        public static Vector<float> Create(ITensorSegment<float> vector)
        {
            throw new NotImplementedException();
        }

        public static Vector<float> ReadFrom(BinaryReader reader)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool IsValid { get; }
        public IFloatMatrix ReshapeAsColumnMatrix()
        {
            throw new NotImplementedException();
        }

        public IFloatMatrix ReshapeAsRowMatrix()
        {
            throw new NotImplementedException();
        }

        public uint Count { get; }
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

    public class FloatMatrix : IIndexableFloatMatrix
    {
        public static Matrix<float> Create(Vector<float>[] rows)
        {
            throw new NotImplementedException();
        }

        public static Matrix<float> ReadFrom(BinaryReader reader)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool IsValid { get; }
        public IFloatMatrix Multiply(IFloatMatrix matrix)
        {
            throw new NotImplementedException();
        }

        public uint ColumnCount { get; }
        public uint RowCount { get; }
        public IFloatVector Column(uint index)
        {
            throw new NotImplementedException();
        }

        public IFloatVector Diagonal()
        {
            throw new NotImplementedException();
        }

        public IFloatVector Row(uint index)
        {
            throw new NotImplementedException();
        }

        public IFloatMatrix Add(IFloatMatrix matrix)
        {
            throw new NotImplementedException();
        }

        public IFloatMatrix Subtract(IFloatMatrix matrix)
        {
            throw new NotImplementedException();
        }

        public IFloatMatrix PointwiseMultiply(IFloatMatrix matrix)
        {
            throw new NotImplementedException();
        }

        public IFloatMatrix TransposeAndMultiply(IFloatMatrix matrix)
        {
            throw new NotImplementedException();
        }

        public IFloatMatrix TransposeThisAndMultiply(IFloatMatrix matrix)
        {
            throw new NotImplementedException();
        }

        public IFloatVector RowSums()
        {
            throw new NotImplementedException();
        }

        public IFloatVector ColumnSums()
        {
            throw new NotImplementedException();
        }

        public IFloatMatrix Transpose()
        {
            throw new NotImplementedException();
        }

        public void Multiply(float scalar)
        {
            throw new NotImplementedException();
        }

        public IFloatMatrix Multiply(IFloatVector vector)
        {
            throw new NotImplementedException();
        }

        public void AddInPlace(IFloatMatrix matrix, float coefficient1 = 1, float coefficient2 = 1)
        {
            throw new NotImplementedException();
        }

        public void SubtractInPlace(IFloatMatrix matrix, float coefficient1 = 1, float coefficient2 = 1)
        {
            throw new NotImplementedException();
        }

        public IFloatMatrix SigmoidActivation()
        {
            throw new NotImplementedException();
        }

        public IFloatMatrix SigmoidDerivative()
        {
            throw new NotImplementedException();
        }

        public IFloatMatrix TanhActivation()
        {
            throw new NotImplementedException();
        }

        public IFloatMatrix TanhDerivative()
        {
            throw new NotImplementedException();
        }

        public IFloatMatrix SoftmaxActivation()
        {
            throw new NotImplementedException();
        }

        public void AddToEachRow(IFloatVector vector)
        {
            throw new NotImplementedException();
        }

        public void AddToEachColumn(IFloatVector vector)
        {
            throw new NotImplementedException();
        }

        public Matrix<float> Data { get; set; }
        public IIndexableFloatMatrix AsIndexable()
        {
            throw new NotImplementedException();
        }

        public IFloatMatrix GetNewMatrixFromRows(IReadOnlyList<uint> rowIndexes)
        {
            throw new NotImplementedException();
        }

        public IFloatMatrix GetNewMatrixFromColumns(IReadOnlyList<uint> columnIndexes)
        {
            throw new NotImplementedException();
        }

        public void ClearRows(IReadOnlyList<uint> indexes)
        {
            throw new NotImplementedException();
        }

        public void ClearColumns(IReadOnlyList<uint> indexes)
        {
            throw new NotImplementedException();
        }

        public IFloatMatrix ReluActivation()
        {
            throw new NotImplementedException();
        }

        public IFloatMatrix ReluDerivative()
        {
            throw new NotImplementedException();
        }

        public IFloatMatrix LeakyReluActivation()
        {
            throw new NotImplementedException();
        }

        public IFloatMatrix LeakyReluDerivative()
        {
            throw new NotImplementedException();
        }

        public IFloatMatrix Clone()
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public IFloatMatrix Sqrt(float valueAdjustment = 1E-08f)
        {
            throw new NotImplementedException();
        }

        public IFloatMatrix Pow(float power)
        {
            throw new NotImplementedException();
        }

        public IFloatMatrix PointwiseDivide(IFloatMatrix matrix)
        {
            throw new NotImplementedException();
        }

        public void L1Regularisation(float coefficient)
        {
            throw new NotImplementedException();
        }

        public IFloatVector ColumnL2Norm()
        {
            throw new NotImplementedException();
        }

        public IFloatVector RowL2Norm()
        {
            throw new NotImplementedException();
        }

        public void PointwiseDivideRows(IFloatVector vector)
        {
            throw new NotImplementedException();
        }

        public void PointwiseDivideColumns(IFloatVector vector)
        {
            throw new NotImplementedException();
        }

        public void Constrain(float min, float max)
        {
            throw new NotImplementedException();
        }

        public IFloatVector GetRowSegment(uint rowIndex, uint columnIndex, uint length)
        {
            throw new NotImplementedException();
        }

        public IFloatVector GetColumnSegment(uint columnIndex, uint rowIndex, uint length)
        {
            throw new NotImplementedException();
        }

        public IFloatMatrix ConcatColumns(IFloatMatrix bottom)
        {
            throw new NotImplementedException();
        }

        public IFloatMatrix ConcatRows(IFloatMatrix right)
        {
            throw new NotImplementedException();
        }

        public (IFloatMatrix Left, IFloatMatrix Right) SplitAtColumn(uint columnIndex)
        {
            throw new NotImplementedException();
        }

        public (IFloatMatrix Top, IFloatMatrix Bottom) SplitAtRow(uint rowIndex)
        {
            throw new NotImplementedException();
        }

        public (IFloatMatrix U, IFloatVector S, IFloatMatrix VT) Svd()
        {
            throw new NotImplementedException();
        }

        public IFloatVector ReshapeAsVector()
        {
            throw new NotImplementedException();
        }

        public I3DFloatTensor ReshapeAs3DTensor(uint rows, uint columns)
        {
            throw new NotImplementedException();
        }

        public I4DFloatTensor ReshapeAs4DTensor(uint rows, uint columns, uint depth)
        {
            throw new NotImplementedException();
        }

        public float GetAt(uint row, uint column)
        {
            throw new NotImplementedException();
        }

        public void SetAt(uint row, uint column, float value)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<IFloatVector> ColumnVectors()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<IFloatVector> RowVectors()
        {
            throw new NotImplementedException();
        }

        public float this[uint row, uint column]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public IEnumerable<IIndexableFloatVector> Rows { get; }
        public IEnumerable<IIndexableFloatVector> Columns { get; }
        public IEnumerable<float> Values { get; }
        public IIndexableFloatMatrix Map(Func<float, float> mutator)
        {
            throw new NotImplementedException();
        }

        public IIndexableFloatMatrix MapIndexed(Func<uint, uint, float, float> mutator)
        {
            throw new NotImplementedException();
        }

        public string AsXml { get; }
        public float[] GetInternalArray()
        {
            throw new NotImplementedException();
        }
    }

    public class Float3DTensor : IIndexable3DFloatTensor
    {
        public static Tensor3D<float> Create(Matrix<float>[] matrices)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public uint RowCount { get; }
        public uint ColumnCount { get; }
        public uint Depth { get; }
        public Tensor3D<float> Data { get; set; }
        public IFloatMatrix GetMatrixAt(uint depth)
        {
            throw new NotImplementedException();
        }

        public IIndexable3DFloatTensor AsIndexable()
        {
            throw new NotImplementedException();
        }

        public I3DFloatTensor AddPadding(uint padding)
        {
            throw new NotImplementedException();
        }

        public I3DFloatTensor RemovePadding(uint padding)
        {
            throw new NotImplementedException();
        }

        public IFloatMatrix Im2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            throw new NotImplementedException();
        }

        public IFloatVector ReshapeAsVector()
        {
            throw new NotImplementedException();
        }

        public IFloatMatrix ReshapeAsMatrix()
        {
            throw new NotImplementedException();
        }

        public I4DFloatTensor ReshapeAs4DTensor(uint rows, uint columns)
        {
            throw new NotImplementedException();
        }

        public (I3DFloatTensor Result, I3DFloatTensor Indices) MaxPool(uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices)
        {
            throw new NotImplementedException();
        }

        public I3DFloatTensor ReverseMaxPool(I3DFloatTensor indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            throw new NotImplementedException();
        }

        public I3DFloatTensor ReverseIm2Col(IFloatMatrix filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            throw new NotImplementedException();
        }

        public IFloatMatrix CombineDepthSlices()
        {
            throw new NotImplementedException();
        }

        public void AddInPlace(I3DFloatTensor tensor)
        {
            throw new NotImplementedException();
        }

        public I3DFloatTensor Multiply(IFloatMatrix matrix)
        {
            throw new NotImplementedException();
        }

        public void AddToEachRow(IFloatVector vector)
        {
            throw new NotImplementedException();
        }

        public I3DFloatTensor TransposeThisAndMultiply(I4DFloatTensor tensor)
        {
            throw new NotImplementedException();
        }

        public float this[uint row, uint column, uint depth]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public IReadOnlyList<IIndexableFloatMatrix> Matrix { get; }
        public string AsXml { get; }
        public float[] GetInternalArray()
        {
            throw new NotImplementedException();
        }
    }
}
