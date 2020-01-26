using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using BrightData.Helper;
using MathNet.Numerics.LinearAlgebra.Single;

namespace BrightData.Numerics
{
    class NumericsMatrix : IComputableMatrix
    {
        readonly MathNet.Numerics.LinearAlgebra.Matrix<float> _matrix;

        public bool IsValid => true;

        public NumericsMatrix(DenseMatrix matrix)
        {
            _matrix = matrix;
        }

        public NumericsMatrix(MathNet.Numerics.LinearAlgebra.Matrix<float> matrix)
        {
            _matrix = matrix;
        }

        public void Dispose()
        {

        }

        public float this[int row, int column]
        {
            get => _matrix[row, column];
            set => _matrix[row, column] = value;
        }

        public int ColumnCount => _matrix.ColumnCount;
        public int RowCount => _matrix.RowCount;
        public IEnumerable<float> Values => _matrix.Enumerate();
        public float[] GetInternalArray() => _matrix.AsColumnMajorArray();

        public IComputableVector Column(int index)
        {
            return new NumericsVector(_matrix.Column(index));
        }

        public IComputableMatrix Map(Func<float, float> mutator)
        {
            return new NumericsMatrix(_matrix.Map(mutator));
        }

        public IComputableMatrix MapIndexed(Func<int, int, float, float> mutator)
        {
            return new NumericsMatrix(_matrix.MapIndexed(mutator));
        }

        public IComputableMatrix Multiply(IComputableMatrix matrix)
        {
            var other = (NumericsMatrix)matrix;
            return new NumericsMatrix(_matrix.Multiply(other._matrix));
        }

        public IComputableMatrix PointwiseMultiply(IComputableMatrix matrix)
        {
            var other = (NumericsMatrix)matrix;
            Debug.Assert(RowCount == matrix.RowCount && ColumnCount == matrix.ColumnCount);
            return new NumericsMatrix(_matrix.PointwiseMultiply(other._matrix));
        }

        public IComputableVector RowSums()
        {
            var ret = _matrix.RowSums();
            return new NumericsVector(ret);
        }

        public IComputableVector ColumnSums()
        {
            var ret = _matrix.ColumnSums();
            return new NumericsVector(ret);
        }

        public IComputableMatrix Add(IComputableMatrix matrix)
        {
            var other = (NumericsMatrix)matrix;
            return new NumericsMatrix(_matrix.Add(other._matrix));
        }

        public IComputableMatrix Subtract(IComputableMatrix matrix)
        {
            var other = (NumericsMatrix)matrix;
            return new NumericsMatrix(_matrix.Subtract(other._matrix));
        }

        public IComputableMatrix TransposeAndMultiply(IComputableMatrix matrix)
        {
            var other = (NumericsMatrix)matrix;
            return new NumericsMatrix(_matrix.TransposeAndMultiply(other._matrix));
        }

        public IComputableMatrix TransposeThisAndMultiply(IComputableMatrix matrix)
        {
            var other = (NumericsMatrix)matrix;
            return new NumericsMatrix(_matrix.TransposeThisAndMultiply(other._matrix));
        }

        public IComputableMatrix Transpose()
        {
            return new NumericsMatrix(_matrix.Transpose());
        }

        public void MultiplyInPlace(float scalar)
        {
            _matrix.MapInplace(v => v * scalar);
        }

        public override string ToString()
        {
            return _matrix.ToMatrixString();
        }

        public void AddInPlace(IComputableMatrix matrix, float coefficient1 = 1.0f, float coefficient2 = 1.0f)
        {
            Debug.Assert(RowCount == matrix.RowCount && ColumnCount == matrix.ColumnCount);
            var other = (NumericsMatrix)matrix;
            _matrix.MapIndexedInplace((i, j, v) => (v * coefficient1) + (other[i, j] * coefficient2));
        }

        public void SubtractInPlace(IComputableMatrix matrix, float coefficient1 = 1.0f, float coefficient2 = 1.0f)
        {
            Debug.Assert(RowCount == matrix.RowCount && ColumnCount == matrix.ColumnCount);
            var other = (NumericsMatrix)matrix;
            _matrix.MapIndexedInplace((i, j, v) => (v * coefficient1) - (other[i, j] * coefficient2));
        }

        internal static float _Sigmoid(float val)
        {
            return FloatMath.Constrain(1.0f / (1.0f + FloatMath.Exp(-1.0f * val)));
        }

        internal static float _SigmoidDerivative(float val)
        {
            var score = _Sigmoid(val);
            return FloatMath.Constrain(score * (1.0f - score));
        }

        internal static float _Tanh(float val)
        {
            return Convert.ToSingle(Math.Tanh(val));
        }

        internal static float _TanhDerivative(float val)
        {
            return 1.0f - Convert.ToSingle(Math.Pow(_Tanh(val), 2));
        }

        internal static float _Relu(float val)
        {
            return (val <= 0) ? 0 : FloatMath.Constrain(val);
        }

        internal static float _ReluDerivative(float val)
        {
            return (val <= 0) ? 0f : 1;
        }

        internal static float _LeakyRelu(float val)
        {
            return (val <= 0) ? 0.01f * val : FloatMath.Constrain(val);
        }

        internal static float _LeakyReluDerivative(float val)
        {
            return (val <= 0) ? 0.01f : 1;
        }

        public IComputableMatrix ReluActivation()
        {
            return new NumericsMatrix(_matrix.Map(_Relu));
        }

        public IComputableMatrix ReluDerivative()
        {
            return new NumericsMatrix(_matrix.Map(_ReluDerivative));
        }

        public IComputableMatrix LeakyReluActivation()
        {
            return new NumericsMatrix(_matrix.Map(_LeakyRelu));
        }

        public IComputableMatrix LeakyReluDerivative()
        {
            return new NumericsMatrix(_matrix.Map(_LeakyReluDerivative));
        }

        public IComputableMatrix SigmoidActivation()
        {
            return new NumericsMatrix(_matrix.Map(_Sigmoid));
        }

        public IComputableMatrix SigmoidDerivative()
        {
            return new NumericsMatrix(_matrix.Map(_SigmoidDerivative));
        }

        public IComputableMatrix TanhActivation()
        {
            return new NumericsMatrix(_matrix.Map(_Tanh));
        }

        public IComputableMatrix TanhDerivative()
        {
            return new NumericsMatrix(_matrix.Map(_TanhDerivative));
        }

        public IComputableMatrix SoftmaxActivation()
        {
            var activation = Rows.Select(r => r.Softmax()).ToList();
            return new NumericsMatrix(DenseMatrix.Create(RowCount, ColumnCount, (x, y) => activation[x][y]));
        }

        public void AddToEachRow(IComputableVector vector)
        {
            var other = (NumericsVector)vector;
            _matrix.MapIndexedInplace((j, k, v) => v + other[k]);
        }

        public void AddToEachColumn(IComputableVector vector)
        {
            var other = (NumericsVector)vector;
            _matrix.MapIndexedInplace((j, k, v) => v + other[j]);
        }

        //public Matrix<float> Data
        //{
        //    get {
        //        var ret = FloatMatrix.Create(new FloatVector[_matrix.RowCount]);
        //        for (var i = 0; i < _matrix.RowCount; i++) {
        //            var row = new float[_matrix.ColumnCount];
        //            for (var j = 0; j < _matrix.ColumnCount; j++) {
        //                row[j] = _matrix[i, j];
        //            }
        //            ret.Row[i] = new FloatVector {
        //                Data = row
        //            };
        //        }
        //        return ret;
        //    }

        //    set {
        //        var arrayList = value.Row;
        //        var rowCount = arrayList.Length;
        //        for (var i = 0; i < rowCount; i++) {
        //            var row = arrayList[i];
        //            if (row.Data != null) {
        //                var data = row.Data;
        //                var columnCount = data.Length;
        //                for (var j = 0; j < columnCount; j++) {
        //                    if (i < _matrix.RowCount && j < _matrix.ColumnCount)
        //                        _matrix[i, j] = data[j];
        //                }
        //            }
        //        }
        //    }
        //}

        public Matrix<float> ToMatrix(IBrightDataContext context)
        {
            return context.CreateMatrix((uint)RowCount, (uint)ColumnCount, (i, j) => _matrix[(int)i, (int)j]);
        }

        public IComputableVector Row(int index)
        {
            return new NumericsVector(_matrix.Row(index));
        }

        public IEnumerable<IComputableVector> Rows
        {
            get {
                return _matrix.EnumerateRows().Select(v => new NumericsVector(v));
            }
        }

        public IEnumerable<IComputableVector> Columns
        {
            get {
                return _matrix.EnumerateColumns().Select(v => new NumericsVector(v));
            }
        }

        public IComputableMatrix GetNewMatrixFromRows(IReadOnlyList<int> rowIndexes)
        {
            return new NumericsMatrix(DenseMatrix.Create(rowIndexes.Count, ColumnCount, (x, y) => _matrix[rowIndexes[x], y]));
        }

        public IComputableMatrix GetNewMatrixFromColumns(IReadOnlyList<int> columnIndexes)
        {
            return new NumericsMatrix(DenseMatrix.Create(RowCount, columnIndexes.Count, (x, y) => _matrix[x, columnIndexes[y]]));
        }

        public void ClearRows(IReadOnlyList<int> indexes)
        {
            _matrix.ClearRows(indexes.ToArray());
        }

        public void ClearColumns(IReadOnlyList<int> indexes)
        {
            _matrix.ClearColumns(indexes.ToArray());
        }

        public IComputableMatrix Clone()
        {
            return new NumericsMatrix(DenseMatrix.OfMatrix(_matrix));
        }

        public void Clear()
        {
            _matrix.Clear();
        }

        public IComputableMatrix ConcatColumns(IComputableMatrix bottom)
        {
            var t = this;
            var b = (NumericsMatrix)bottom;
            Debug.Assert(ColumnCount == bottom.ColumnCount);

            var ret = DenseMatrix.Create(t.RowCount + b.RowCount, t.ColumnCount, (x, y) => {
                var m = x >= t.RowCount ? b._matrix : t._matrix;
                return m[x >= t.RowCount ? x - t.RowCount : x, y];
            });
            return new NumericsMatrix(ret);
        }

        public IComputableMatrix ConcatRows(IComputableMatrix right)
        {
            var t = this;
            var b = (NumericsMatrix)right;
            Debug.Assert(RowCount == right.RowCount);

            var ret = DenseMatrix.Create(t.RowCount, t.ColumnCount + b.ColumnCount, (x, y) => {
                var m = y >= t.ColumnCount ? b._matrix : t._matrix;
                return m[x, y >= t.ColumnCount ? y - t.ColumnCount : y];
            });
            return new NumericsMatrix(ret);
        }

        public (IComputableMatrix Left, IComputableMatrix Right) SplitAtColumn(int columnIndex)
        {
            var ret1 = DenseMatrix.Create(RowCount, columnIndex, (x, y) => this[x, y]);
            var ret2 = DenseMatrix.Create(RowCount, ColumnCount - columnIndex, (x, y) => this[x, columnIndex + y]);
            return (new NumericsMatrix(ret1), new NumericsMatrix(ret2));
        }

        public (IComputableMatrix Top, IComputableMatrix Bottom) SplitAtRow(int rowIndex)
        {
            var ret1 = DenseMatrix.Create(rowIndex, ColumnCount, (x, y) => this[x, y]);
            var ret2 = DenseMatrix.Create(RowCount - rowIndex, ColumnCount, (x, y) => this[rowIndex + x, y]);
            return (new NumericsMatrix(ret1), new NumericsMatrix(ret2));
        }

        public IComputableMatrix Sqrt(float valueAdjustment = 1e-8f)
        {
            return new NumericsMatrix((DenseMatrix)_matrix.Map(v => Convert.ToSingle(Math.Sqrt(v + valueAdjustment))));
        }

        public IComputableMatrix PointwiseDivide(IComputableMatrix matrix)
        {
            var other = (NumericsMatrix)matrix;
            return new NumericsMatrix(_matrix.PointwiseDivide(other._matrix));
        }

        public void L1Regularisation(float coefficient)
        {
            _matrix.MapInplace(v => v - ((v > 0 ? 1 : v < 0 ? -1 : 0) * coefficient));
        }

        public void Constrain(float min, float max)
        {
            _matrix.MapInplace(v => v < min ? min : v > max ? max : v);
        }

        public IComputableVector ColumnL2Norm()
        {
            var ret = _matrix.ColumnNorms(2.0);
            return new NumericsVector(DenseVector.Create(ret.Count, i => Convert.ToSingle(ret[i])));
        }

        public IComputableVector RowL2Norm()
        {
            var ret = _matrix.RowNorms(2.0);
            return new NumericsVector(DenseVector.Create(ret.Count, i => Convert.ToSingle(ret[i])));
        }

        public void PointwiseDivideRows(IComputableVector vector)
        {
            _matrix.MapIndexedInplace((x, y, v) => v /= vector[x]);
        }

        public void PointwiseDivideColumns(IComputableVector vector)
        {
            _matrix.MapIndexedInplace((x, y, v) => v /= vector[y]);
        }

        public IComputableVector Diagonal()
        {
            return new NumericsVector((DenseVector)_matrix.Diagonal());
        }

        public IComputableMatrix Pow(float power)
        {
            return new NumericsMatrix(_matrix.Map(v => Convert.ToSingle(Math.Pow(v, power))));
        }

        public IComputableVector GetRowSegment(int index, int columnIndex, int length)
        {
            var buffer = new float[length];
            for (var i = 0; i < length; i++)
                buffer[i] = _matrix[index, columnIndex + i];
            return new NumericsVector(DenseVector.OfArray(buffer));
        }

        public IComputableVector GetColumnSegment(int columnIndex, int rowIndex, int length)
        {
            var buffer = new float[length];
            for (var i = 0; i < length; i++)
                buffer[i] = _matrix[rowIndex + i, columnIndex];
            return new NumericsVector(DenseVector.OfArray(buffer));
        }

        public IComputableMatrix Multiply(IComputableVector vector)
        {
            using var column = vector.ReshapeAsColumnMatrix();
            return Multiply(column);
        }

        public (IComputableMatrix U, IComputableVector S, IComputableMatrix VT) Svd()
        {
            var svd = _matrix.Svd(true);
            return (new NumericsMatrix(svd.U), new NumericsVector(svd.S), new NumericsMatrix(svd.VT));
        }

        public IComputableVector ReshapeAsVector()
        {
            return new NumericsVector(_matrix.AsColumnMajorArray());
        }

        public IComputable3DTensor ReshapeAs3DTensor(int rows, int columns)
        {
            Debug.Assert(rows * columns == RowCount);
            var matrixList = new List<IComputableMatrix>();
            for (int i = 0, len = ColumnCount; i < len; i++)
                matrixList.Add(Column(i).ReshapeAsMatrix(rows, columns));
            return new Numerics3DTensor(matrixList);
        }

        public IComputable4DTensor ReshapeAs4DTensor(int rows, int columns, int depth)
        {
            var list = new List<IComputable3DTensor>();
            for (var i = 0; i < ColumnCount; i++)
                list.Add(new Numerics3DTensor(Column(i).Split(depth).Select(v => v.ReshapeAsMatrix(rows, columns)).ToList()));
            return new Numerics4DTensor(list);
        }

        public float GetAt(int row, int column)
        {
            return this[row, column];
        }

        public void SetAt(int row, int column, float value)
        {
            this[row, column] = value;
        }

        public IReadOnlyList<IComputableVector> ColumnVectors() => Columns.ToList();

        public IReadOnlyList<IComputableVector> RowVectors() => Rows.ToList();

        public string AsXml
        {
            get {
                var ret = new StringBuilder();
                var settings = new XmlWriterSettings {
                    OmitXmlDeclaration = true
                };
                using (var writer = XmlWriter.Create(new StringWriter(ret), settings)) {
                    writer.WriteStartElement("matrix");
                    for (var i = 0; i < RowCount; i++) {
                        writer.WriteStartElement("row");

                        var row = new StringBuilder();
                        for (var j = 0; j < ColumnCount; j++) {
                            if (j > 0)
                                row.Append("|");
                            row.Append(_matrix[i, j]);
                        }
                        writer.WriteValue(row.ToString());
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }
                return ret.ToString();
            }
        }
    }
}
