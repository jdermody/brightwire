using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BrightData.FloatTensors
{
    public class FloatMatrix : IIndexableFloatMatrix
    {
        public FloatMatrix(Matrix<float> data) => Data = data;
        public void Dispose() => Data.Dispose();

        public static Matrix<float> Create(IBrightDataContext context, Vector<float>[] rows) => context.CreateMatrixFromRows(rows);
        public static Matrix<float> ReadFrom(IBrightDataContext context, BinaryReader reader) => new Matrix<float>(context, reader);

        public bool IsValid { get; } = true;
        public IFloatMatrix Multiply(IFloatMatrix matrix) => new FloatMatrix(Data.Multiply(matrix.Data));
        public uint ColumnCount => Data.ColumnCount;
        public uint RowCount => Data.RowCount;
        public IFloatVector Column(uint index) => new FloatVector(Data.Column(index));
        public IFloatVector Diagonal() => new FloatVector(Data.GetDiagonal());
        public IFloatVector Row(uint index) => new FloatVector(Data.Row(index));
        public IFloatMatrix Add(IFloatMatrix matrix) => new FloatMatrix(Data.Add(matrix.Data));
        public IFloatMatrix Subtract(IFloatMatrix matrix) => new FloatMatrix(Data.Subtract(matrix.Data));
        public IFloatMatrix PointwiseMultiply(IFloatMatrix matrix) => new FloatMatrix(Data.PointwiseMultiply(matrix.Data));
        public IFloatMatrix TransposeAndMultiply(IFloatMatrix matrix) => new FloatMatrix(Data.Multiply(matrix.Data.Transpose()));
        public IFloatMatrix TransposeThisAndMultiply(IFloatMatrix matrix) => new FloatMatrix(Data.Transpose().Multiply(matrix.Data));
        public IFloatVector RowSums() => new FloatVector(Data.RowSums());
        public IFloatVector ColumnSums() => new FloatVector(Data.ColumnSums());
        public IFloatMatrix Transpose() => new FloatMatrix(Data.Transpose());
        public void Multiply(float scalar) => Data.Multiply(scalar);
        public IFloatMatrix Multiply(IFloatVector vector) => new FloatMatrix(Data.Multiply(vector.Data));
        public void AddInPlace(IFloatMatrix matrix, float coefficient1 = 1, float coefficient2 = 1) => Data.AddInPlace(matrix.Data, coefficient1, coefficient2);
        public void SubtractInPlace(IFloatMatrix matrix, float coefficient1 = 1, float coefficient2 = 1) => Data.SubtractInPlace(matrix.Data, coefficient1, coefficient2);
        public IFloatMatrix SigmoidActivation() => new FloatMatrix(Data.Sigmoid());
        public IFloatMatrix SigmoidDerivative() => new FloatMatrix(Data.SigmoidDerivative());
        public IFloatMatrix TanhActivation() => new FloatMatrix(Data.Tanh());
        public IFloatMatrix TanhDerivative() => new FloatMatrix(Data.TanhDerivative());
        public IFloatMatrix SoftmaxActivation() => new FloatMatrix(Data.Softmax());

        public void AddToEachRow(IFloatVector vector)
        {
            throw new NotImplementedException();
        }

        public void AddToEachColumn(IFloatVector vector)
        {
            throw new NotImplementedException();
        }

        public Matrix<float> Data { get; set; }
        public IIndexableFloatMatrix AsIndexable() => this;

        public IFloatMatrix GetNewMatrixFromRows(IEnumerable<uint> rowIndexes)
        {
            var ret = Data.Context.CreateMatrixFromRows(rowIndexes.Select(i => Data.Row(i)).ToArray());
            return new FloatMatrix(ret);
        }

        public IFloatMatrix GetNewMatrixFromColumns(IEnumerable<uint> columnIndexes)
        {
            var ret = Data.Context.CreateMatrixFromColumns(columnIndexes.Select(i => Data.Column(i)).ToArray());
            return new FloatMatrix(ret);
        }

        public void ClearRows(IEnumerable<uint> indexes)
        {
            foreach(var i in indexes)
                Row(i).Data.Segment.Initialize(0f);
        }

        public void ClearColumns(IEnumerable<uint> indexes)
        {
            foreach (var i in indexes)
                Column(i).Data.Segment.Initialize(0f);
        }

        public IFloatMatrix ReluActivation() => new FloatMatrix(Data.Relu());
        public IFloatMatrix ReluDerivative() => new FloatMatrix(Data.ReluDerivative());
        public IFloatMatrix LeakyReluActivation() => new FloatMatrix(Data.LeakyRelu());
        public IFloatMatrix LeakyReluDerivative() => new FloatMatrix(Data.LeakyReluDerivative());
        public IFloatMatrix Clone() => new FloatMatrix(Data.Clone());
        public void Clear() => Data.Segment.Initialize(0f);

        // TODO: remove value adjustment
        public IFloatMatrix Sqrt(float valueAdjustment = 1E-08f) => new FloatMatrix(Data.Sqrt());

        public IFloatMatrix Pow(float power) => new FloatMatrix(Data.Pow(power));

        public IFloatMatrix PointwiseDivide(IFloatMatrix matrix) => new FloatMatrix(Data.PointwiseDivide(matrix.Data));

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

        public void Constrain(float min, float max) => Data.ConstrainInPlace(min, max);

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

        public IFloatVector ReshapeAsVector() => new FloatVector(new Vector<float>(Data.Segment));

        public I3DFloatTensor ReshapeAs3DTensor(uint rows, uint columns)
        {
            throw new NotImplementedException();
        }

        public I4DFloatTensor ReshapeAs4DTensor(uint rows, uint columns, uint depth)
        {
            throw new NotImplementedException();
        }

        public float GetAt(uint row, uint column) => this[row, column];
        public void SetAt(uint row, uint column, float value) => this[row, column] = value;

        public IFloatVector[] ColumnVectors() => Columns.Cast<IFloatVector>().ToArray();
        public IFloatVector[] RowVectors() => Rows.Cast<IFloatVector>().ToArray();

        public float this[uint row, uint column]
        {
            get => Data[row, column];
            set => Data[row, column] = value;
        }

        public IEnumerable<IIndexableFloatVector> Rows => Data.Rows.Select(r => new FloatVector(r));
        public IEnumerable<IIndexableFloatVector> Columns => Data.Columns.Select(r => new FloatVector(r));
        public IEnumerable<float> Values => Data.Segment.Values;
        public IIndexableFloatMatrix Map(Func<float, float> mutator)
        {
            throw new NotImplementedException();
        }

        public IIndexableFloatMatrix MapIndexed(Func<uint, uint, float, float> mutator)
        {
            throw new NotImplementedException();
        }

        public string AsXml => throw new NotImplementedException();
        public float[] GetInternalArray()
        {
            throw new NotImplementedException();
        }
        public override string ToString() => $"Simple: {Data}";

        public ILinearAlgebraProvider LinearAlgebraProvider => Data.Context.LinearAlgebraProvider;
    }
}
