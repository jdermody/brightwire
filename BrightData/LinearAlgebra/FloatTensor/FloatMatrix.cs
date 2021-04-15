using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace BrightData.LinearAlgebra.FloatTensor
{
    internal class FloatMatrix : IIndexableFloatMatrix
    {
        public FloatMatrix(Matrix<float> data) => Data = data;
        public void Dispose() => Data.Dispose();

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
            var other = vector.Data;
            Data.MapIndexedInPlace((j, k, v) => v + other[k]);
        }

        public void AddToEachColumn(IFloatVector vector)
        {
            var other = vector.Data;
            Data.MapIndexedInPlace((j, k, v) => v + other[j]);
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

        public IFloatMatrix Sqrt() => new FloatMatrix(Data.Sqrt());
        public IFloatMatrix Pow(float power) => new FloatMatrix(Data.Pow(power));

        public IFloatMatrix PointwiseDivide(IFloatMatrix matrix) => new FloatMatrix(Data.PointwiseDivide(matrix.Data));

        public void L1Regularisation(float coefficient) => Data.MapInPlace(v => v - (v > 0 ? 1 : v < 0 ? -1 : 0) * coefficient);

        public IFloatVector ColumnL2Norm()
        {
            var norms = Data.Columns.Select(c => c.L2Norm()).ToArray();
            return new FloatVector(Data.Context.CreateVector(norms));
        }

        public IFloatVector RowL2Norm()
        {
            var norms = Data.Rows.Select(c => c.L2Norm()).ToArray();
            return new FloatVector(Data.Context.CreateVector(norms));
        }

        public void PointwiseDivideRows(IFloatVector vector)
        {
            var v2 = vector.Data;
            Data.MapIndexedInPlace((x, y, v) => v / v2[x]);
        }

        public void PointwiseDivideColumns(IFloatVector vector)
        {
            var v2 = vector.Data;
            Data.MapIndexedInPlace((x, y, v) => v / v2[y]);
        }

        public void Constrain(float min, float max) => Data.ConstrainInPlace(min, max);

        public IFloatVector GetRowSegment(uint index, uint columnIndex, uint length)
        {
            var buffer = new float[length];
            for (uint i = 0; i < length; i++)
                buffer[i] = Data[index, columnIndex + i];
            return new FloatVector(Data.Context.CreateVector(buffer));
        }

        public IFloatVector GetColumnSegment(uint columnIndex, uint rowIndex, uint length)
        {
            var buffer = new float[length];
            for (uint i = 0; i < length; i++)
                buffer[i] = Data[rowIndex + i, columnIndex];
            return new FloatVector(Data.Context.CreateVector(buffer));
        }

        public IFloatMatrix ConcatColumns(IFloatMatrix bottom)
        {
            Debug.Assert(ColumnCount == bottom.ColumnCount);
            var ret = Data.Context.CreateMatrix(RowCount + bottom.RowCount, ColumnCount, (x, y) => {
                var m = x >= RowCount ? bottom.Data : Data;
                return m[x >= RowCount ? x - RowCount : x, y];
            });
            return new FloatMatrix(ret);
        }

        public IFloatMatrix ConcatRows(IFloatMatrix right)
        {
            Debug.Assert(RowCount == right.RowCount);
            var ret = Data.Context.CreateMatrix(RowCount, ColumnCount + right.ColumnCount, (x, y) => {
                var m = y >= ColumnCount ? right.Data : Data;
                return m[x, y >= ColumnCount ? y - ColumnCount : y];
            });
            return new FloatMatrix(ret);
        }

        public (IFloatMatrix Left, IFloatMatrix Right) SplitAtColumn(uint columnIndex)
        {
            var context = Data.Context;
            var ret1 = context.CreateMatrix(RowCount, columnIndex, (x, y) => this[x, y]);
            var ret2 = context.CreateMatrix(RowCount, ColumnCount - columnIndex, (x, y) => this[x, columnIndex + y]);
            return (new FloatMatrix(ret1), new FloatMatrix(ret2));
        }

        public (IFloatMatrix Top, IFloatMatrix Bottom) SplitAtRow(uint rowIndex)
        {
            var context = Data.Context;
            var ret1 = context.CreateMatrix(rowIndex, ColumnCount, (x, y) => this[x, y]);
            var ret2 = context.CreateMatrix(RowCount - rowIndex, ColumnCount, (x, y) => this[rowIndex + x, y]);
            return (new FloatMatrix(ret1), new FloatMatrix(ret2));
        }

        public (IFloatMatrix U, IFloatVector S, IFloatMatrix VT) Svd()
        {
            throw new NotImplementedException();
        }

        public IFloatVector ReshapeAsVector() => new FloatVector(new Vector<float>(Data.Segment));

        public I3DFloatTensor ReshapeAs3DTensor(uint rows, uint columns)
        {
            Debug.Assert(rows * columns == RowCount);
            var matrixList = new Matrix<float>[ColumnCount];
            for (uint i = 0, len = ColumnCount; i < len; i++)
                matrixList[i] = Data.Column(i).Reshape(rows, columns);
            return new Float3DTensor(Data.Context.CreateTensor3D(matrixList));
        }

        public I4DFloatTensor ReshapeAs4DTensor(uint rows, uint columns, uint depth)
        {
            var context = Data.Context;
            var list = new Tensor3D<float>[ColumnCount];
            for (uint i = 0; i < ColumnCount; i++)
                list[i] = context.CreateTensor3D(Column(i).Split(depth).Select(v => v.ReshapeAsMatrix(rows, columns).Data).ToArray());
            return new Float4DTensor(list);
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
        public IIndexableFloatMatrix Map(Func<float, float> mutator) => new FloatMatrix(Data.Map(mutator));

        public IIndexableFloatMatrix MapIndexed(Func<uint, uint, float, float> mutator) => new FloatMatrix(Data.MapIndexed(mutator));

        public string AsXml
        {
            get
            {
                var ret = new StringBuilder();
                var settings = new XmlWriterSettings {
                    OmitXmlDeclaration = true
                };
                using var writer = XmlWriter.Create(new StringWriter(ret), settings);
                writer.WriteStartElement("matrix");
                for (var i = 0; i < RowCount; i++) {
                    writer.WriteStartElement("row");

                    var row = new StringBuilder();
                    for (var j = 0; j < ColumnCount; j++) {
                        if (j > 0)
                            row.Append("|");
                        row.Append(Data[i, j]);
                    }
                    writer.WriteValue(row.ToString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                return ret.ToString();
            }
        }
        public override string ToString() => $"Simple: {Data}";

        public ILinearAlgebraProvider LinearAlgebraProvider => Data.Context.LinearAlgebraProvider;
    }
}
