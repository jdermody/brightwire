using System;
using System.Linq;

namespace BrightData.FloatTensor
{
    public class Float3DTensor : IIndexable3DFloatTensor
    {
        public Float3DTensor(Tensor3D<float> data) => Data = data;
        public void Dispose() => Data.Dispose();

        public static Tensor3D<float> Create(IBrightDataContext context, Matrix<float>[] matrices) => context.CreateTensor3D(matrices);

        public uint RowCount => Data.RowCount;
        public uint ColumnCount => Data.ColumnCount;
        public uint Depth => Data.Depth;
        public Tensor3D<float> Data { get; set; }
        public IFloatMatrix GetMatrixAt(uint depth) => new FloatMatrix(Data.Matrix(depth));
        public IIndexable3DFloatTensor AsIndexable() => this;

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

        public IFloatVector ReshapeAsVector() => new FloatVector(new Vector<float>(Data.Segment));

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

        public void AddInPlace(I3DFloatTensor tensor) => Data.AddInPlace(tensor.Data);

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
            get => Data[depth, row, column];
            set => Data[depth, row, column] = value;
        }

        public IIndexableFloatMatrix[] Matrix => Data.Matrices.Select(m => (IIndexableFloatMatrix)new FloatMatrix(m)).ToArray();
        public string AsXml => throw new NotImplementedException();
        public float[] GetInternalArray()
        {
            throw new NotImplementedException();
        }
        public override string ToString() => $"Simple: {Data}";

        public ILinearAlgebraProvider LinearAlgebraProvider => Data.Context.LinearAlgebraProvider;
    }
}
