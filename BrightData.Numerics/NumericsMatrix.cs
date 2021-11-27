using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using BrightData.Helper;
using BrightData.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

namespace BrightData.Numerics
{
    /// <summary>
    /// Matrix that uses the CPU based math.net numerics library
    /// </summary>
    internal class NumericsMatrix : IIndexableFloatMatrix
    {
        readonly MathNet.Numerics.LinearAlgebra.Matrix<float> _matrix;

        public bool IsValid => true;

	    public NumericsMatrix(IBrightDataContext context, DenseMatrix matrix)
        {
            Context = context;
            _matrix = matrix;
        }

        public NumericsMatrix(IBrightDataContext context, MathNet.Numerics.LinearAlgebra.Matrix<float> matrix)
        {
            _matrix = matrix;
            Context = context;
        }

        protected virtual void Dispose(bool disposing)
        {
            // nop
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public float this[uint row, uint column]
        {
            get => _matrix[(int)row, (int)column];
	        set => _matrix[(int)row, (int)column] = value;
        }

        public ILinearAlgebraProvider LinearAlgebraProvider => Context.LinearAlgebraProvider;
        public uint ColumnCount => (uint)_matrix.ColumnCount;
	    public uint RowCount => (uint)_matrix.RowCount;
	    public IEnumerable<float> Values => _matrix.Enumerate();
	    public float[] GetInternalArray() => _matrix.AsColumnMajorArray();
        public IBrightDataContext Context { get; }

        public IFloatVector Column(uint index)
        {
            return new NumericsVector(Context, _matrix.Column((int)index));
        }

        public IIndexableFloatMatrix Map(Func<float, float> mutator)
        {
            return new NumericsMatrix(Context, _matrix.Map(mutator));
        }

        public IIndexableFloatMatrix MapIndexed(Func<uint, uint, float, float> mutator)
        {
            return new NumericsMatrix(Context, _matrix.MapIndexed((i, j, v) => mutator((uint)i, (uint)j, v)));
        }

        public IFloatMatrix Multiply(IFloatMatrix matrix)
        {
            var other = (NumericsMatrix)matrix;
            return new NumericsMatrix(Context, _matrix.Multiply(other._matrix));
        }

        public IFloatMatrix PointwiseMultiply(IFloatMatrix matrix)
        {
            var other = (NumericsMatrix)matrix;
            Debug.Assert(RowCount == matrix.RowCount && ColumnCount == matrix.ColumnCount);
            return new NumericsMatrix(Context, _matrix.PointwiseMultiply(other._matrix));
        }

        public IFloatVector RowSums()
        {
            var ret = _matrix.RowSums();
            return new NumericsVector(Context, ret);
        }

        public IFloatVector ColumnSums()
        {
            var ret = _matrix.ColumnSums();
            return new NumericsVector(Context, ret);
        }

        public IFloatMatrix Add(IFloatMatrix matrix)
        {
            var other = (NumericsMatrix)matrix;
            return new NumericsMatrix(Context, _matrix.Add(other._matrix));
        }

        public IFloatMatrix Subtract(IFloatMatrix matrix)
        {
            var other = (NumericsMatrix)matrix;
            return new NumericsMatrix(Context, _matrix.Subtract(other._matrix));
        }

        public IFloatMatrix TransposeAndMultiply(IFloatMatrix matrix)
        {
            var other = (NumericsMatrix)matrix;
            return new NumericsMatrix(Context, _matrix.TransposeAndMultiply(other._matrix));
        }

        public IFloatMatrix TransposeThisAndMultiply(IFloatMatrix matrix)
        {
            var other = (NumericsMatrix)matrix;
            return new NumericsMatrix(Context, _matrix.TransposeThisAndMultiply(other._matrix));
        }

        public IFloatMatrix Transpose()
        {
            return new NumericsMatrix(Context, _matrix.Transpose());
        }

        public void Multiply(float scalar)
        {
            _matrix.MapInplace(v => v * scalar);
        }

        public override string ToString() => $"Matrix (Rows: {RowCount}, Columns: {ColumnCount}): {_matrix.ToMatrixString()}";

        public void AddInPlace(IFloatMatrix matrix, float coefficient1 = 1.0f, float coefficient2 = 1.0f)
        {
            Debug.Assert(RowCount == matrix.RowCount && ColumnCount == matrix.ColumnCount);
            var other = (NumericsMatrix)matrix;
            _matrix.MapIndexedInplace((i, j, v) => (v * coefficient1) + (other[(uint)i, (uint)j] * coefficient2));
        }

        public void SubtractInPlace(IFloatMatrix matrix, float coefficient1 = 1.0f, float coefficient2 = 1.0f)
        {
            Debug.Assert(RowCount == matrix.RowCount && ColumnCount == matrix.ColumnCount);
            var other = (NumericsMatrix)matrix;
            _matrix.MapIndexedInplace((i, j, v) => (v * coefficient1) - (other[(uint)i, (uint)j] * coefficient2));
        }

        internal static float Sigmoid(float val)
        {
            return FloatMath.Constrain(1.0f / (1.0f + FloatMath.Exp(-1.0f * val)));
        }

        internal static float SigmoidDerivative(float val)
        {
            var score = Sigmoid(val);
            return FloatMath.Constrain(score * (1.0f - score));
        }

        internal static float Tanh(float val)
        {
            return Convert.ToSingle(Math.Tanh(val));
        }

        internal static float TanhDerivative(float val)
        {
            return 1.0f - Convert.ToSingle(Math.Pow(Tanh(val), 2));
        }

        internal static float Relu(float val)
        {
            return (val <= 0) ? 0 : FloatMath.Constrain(val);
        }

        internal static float ReluDerivative(float val)
        {
            return (val <= 0) ? 0f : 1;
        }

        internal static float LeakyRelu(float val)
        {
            return (val <= 0) ? 0.01f * val : FloatMath.Constrain(val);
        }

        internal static float LeakyReluDerivative(float val)
        {
            return (val <= 0) ? 0.01f : 1;
        }

        public IFloatMatrix ReluActivation()
        {
            return new NumericsMatrix(Context, _matrix.Map(Relu));
        }

        public IFloatMatrix ReluDerivative()
        {
            return new NumericsMatrix(Context, _matrix.Map(ReluDerivative));
        }

        public IFloatMatrix LeakyReluActivation()
        {
            return new NumericsMatrix(Context, _matrix.Map(LeakyRelu));
        }

        public IFloatMatrix LeakyReluDerivative()
        {
            return new NumericsMatrix(Context, _matrix.Map(LeakyReluDerivative));
        }

        public IFloatMatrix SigmoidActivation()
        {
            return new NumericsMatrix(Context, _matrix.Map(Sigmoid));
        }

        public IFloatMatrix SigmoidDerivative()
        {
            return new NumericsMatrix(Context, _matrix.Map(SigmoidDerivative));
        }

        public IFloatMatrix TanhActivation()
        {
            return new NumericsMatrix(Context, _matrix.Map(Tanh));
        }

        public IFloatMatrix TanhDerivative()
        {
            return new NumericsMatrix(Context, _matrix.Map(TanhDerivative));
        }

        public IFloatMatrix SoftmaxActivation()
        {
            var activation = Rows.Select(r => r.Softmax().AsIndexable()).ToList();
            return new NumericsMatrix(Context, DenseMatrix.Create((int)RowCount, (int)ColumnCount, (x, y) => activation[x][(uint)y]));
        }

        public void AddToEachRow(IFloatVector vector)
        {
            var other = (NumericsVector)vector;
            _matrix.MapIndexedInplace((_, k, v) => v + other[(uint)k]);
        }

        public void AddToEachColumn(IFloatVector vector)
        {
            var other = (NumericsVector)vector;
            _matrix.MapIndexedInplace((j, _, v) => v + other[(uint)j]);
        }

        public Matrix<float> Data
        {
            get
            {
                var ret = new Vector<float>[_matrix.RowCount];
                for (var i = 0; i < _matrix.RowCount; i++) {
                    var row = new float[_matrix.ColumnCount];
                    for (var j = 0; j < _matrix.ColumnCount; j++)
                        row[j] = _matrix[i, j];
                    ret[i] = Context.CreateVector(row);
                }
                return Context.CreateMatrixFromRows(ret);
            }

            set
            {
                var arrayList = value.Rows.ToArray();
                var rowCount = arrayList.Length;
                for (var i = 0; i < rowCount; i++) {
                    var row = arrayList[i];
                    var data = row.Segment;
                    var columnCount = data.Size;
                    for (var j = 0; j < columnCount; j++) {
                        if (i < _matrix.RowCount && j < _matrix.ColumnCount)
                            _matrix[i, j] = data[j];
                    }
                }
            }
        }

        public IIndexableFloatMatrix AsIndexable()
        {
            return this;
        }

        public IFloatVector Row(uint index)
        {
            return new NumericsVector(Context, _matrix.Row((int)index));
        }

        public IEnumerable<IIndexableFloatVector> Rows
        {
            get
            {
                return _matrix.EnumerateRows().Select(v => new NumericsVector(Context, v));
            }
        }

        public IEnumerable<IIndexableFloatVector> Columns
        {
            get
            {
                return _matrix.EnumerateColumns().Select(v => new NumericsVector(Context, v));
            }
        }

	    public IFloatMatrix GetNewMatrixFromRows(IEnumerable<uint> rowIndexes)
        {
            var indices = rowIndexes.ToList();
            return new NumericsMatrix(Context, DenseMatrix.Create(indices.Count, (int)ColumnCount, (x, y) => _matrix[(int)indices[x], y]));
        }

        public IFloatMatrix GetNewMatrixFromColumns(IEnumerable<uint> columnIndexes)
        {
            var indices = columnIndexes.ToList();
            return new NumericsMatrix(Context, DenseMatrix.Create((int)RowCount, indices.Count, (x, y) => _matrix[x, (int)indices[y]]));
        }

        public void ClearRows(IEnumerable<uint> indexes)
        {
            _matrix.ClearRows(indexes.Select(i => (int)i).ToArray());
        }

        public void ClearColumns(IEnumerable<uint> indexes)
        {
            _matrix.ClearColumns(indexes.Select(i => (int)i).ToArray());
        }

        public IFloatMatrix Clone()
        {
            return new NumericsMatrix(Context, DenseMatrix.OfMatrix(_matrix));
        }

        public void Clear()
        {
            _matrix.Clear();
        }

        public IFloatMatrix ConcatColumns(IFloatMatrix bottom)
        {
            var t = this;
            var b = (NumericsMatrix)bottom;
            Debug.Assert(ColumnCount == bottom.ColumnCount);

            var ret = DenseMatrix.Create((int)(t.RowCount + b.RowCount), (int)t.ColumnCount, (x, y) => {
                var m = x >= t.RowCount ? b._matrix : t._matrix;
                return m[(int)(x >= t.RowCount ? x - t.RowCount : x), y];
            });
            return new NumericsMatrix(Context, ret);
        }

        public IFloatMatrix ConcatRows(IFloatMatrix right)
        {
            var t = this;
            var b = (NumericsMatrix)right;
            Debug.Assert(RowCount == right.RowCount);

            var ret = DenseMatrix.Create((int)t.RowCount, (int)(t.ColumnCount + b.ColumnCount), (x, y) => {
                var m = y >= t.ColumnCount ? b._matrix : t._matrix;
                return m[x, (int)(y >= t.ColumnCount ? y - t.ColumnCount : y)];
            });
            return new NumericsMatrix(Context, ret);
        }

        public (IFloatMatrix Left, IFloatMatrix Right) SplitAtColumn(uint columnIndex)
        {
            var ret1 = DenseMatrix.Create((int)RowCount, (int)columnIndex, (x, y) => this[(uint)x, (uint)y]);
            var ret2 = DenseMatrix.Create((int)RowCount, (int)(ColumnCount - columnIndex), (x, y) => this[(uint)x, (uint)(columnIndex + y)]);
            return (new NumericsMatrix(Context, ret1), new NumericsMatrix(Context, ret2));
        }

        public (IFloatMatrix Top, IFloatMatrix Bottom) SplitAtRow(uint rowIndex)
        {
            var ret1 = DenseMatrix.Create((int)rowIndex, (int)ColumnCount, (x, y) => this[(uint)x, (uint)y]);
            var ret2 = DenseMatrix.Create((int)(RowCount - rowIndex), (int)ColumnCount, (x, y) => this[(uint)(rowIndex + x), (uint)y]);
            return (new NumericsMatrix(Context, ret1), new NumericsMatrix(Context, ret2));
        }

        public IFloatMatrix Sqrt()
        {
            return new NumericsMatrix(Context, (DenseMatrix)_matrix.Map(v => FloatMath.Sqrt(v)));
        }

        public IFloatMatrix PointwiseDivide(IFloatMatrix matrix)
        {
            var other = (NumericsMatrix)matrix;
            return new NumericsMatrix(Context, _matrix.PointwiseDivide(other._matrix));
        }

        public void L1Regularisation(float coefficient)
        {
            _matrix.MapInplace(v => v - ((v > 0 ? 1 : v < 0 ? -1 : 0) * coefficient));
        }

        public void Constrain(float min, float max)
        {
            _matrix.MapInplace(v => v < min ? min : v > max ? max : v);
        }

        public IFloatVector ColumnL2Norm()
        {
            var ret = _matrix.ColumnNorms(2.0);
            return new NumericsVector(Context, DenseVector.Create(ret.Count, i => Convert.ToSingle(ret[i])));
        }

        public IFloatVector RowL2Norm()
        {
            var ret = _matrix.RowNorms(2.0);
            return new NumericsVector(Context, DenseVector.Create(ret.Count, i => Convert.ToSingle(ret[i])));
        }

        public void PointwiseDivideRows(IFloatVector vector)
        {
            var v2 = vector.AsIndexable();
            _matrix.MapIndexedInplace((x, _, v) => v / v2[(uint) x]);
        }

        public void PointwiseDivideColumns(IFloatVector vector)
        {
            var v2 = vector.AsIndexable();
            _matrix.MapIndexedInplace((_, y, v) => v / v2[(uint)y]);
        }

        public IFloatVector Diagonal()
        {
            return new NumericsVector(Context, (DenseVector)_matrix.Diagonal());
        }

        public IFloatMatrix Pow(float power)
        {
            return new NumericsMatrix(Context, _matrix.Map(v => Convert.ToSingle(Math.Pow(v, power))));
        }

        public IFloatVector GetRowSegment(uint index, uint columnIndex, uint length)
        {
            var buffer = new float[length];
            for (uint i = 0; i < length; i++)
                buffer[i] = _matrix[(int)index, (int)(columnIndex + i)];
            return new NumericsVector(Context, DenseVector.OfArray(buffer));
        }

        public IFloatVector GetColumnSegment(uint columnIndex, uint rowIndex, uint length)
        {
            var buffer = new float[length];
            for (var i = 0; i < length; i++)
                buffer[i] = _matrix[(int)(rowIndex + i), (int)columnIndex];
            return new NumericsVector(Context, DenseVector.OfArray(buffer));
        }

        public IFloatMatrix Multiply(IFloatVector vector)
        {
            using var column = vector.ReshapeAsColumnMatrix();
            return Multiply(column);
        }

        public (IFloatMatrix U, IFloatVector S, IFloatMatrix VT) Svd()
        {
            var svd = _matrix.Svd();
            return (new NumericsMatrix(Context, svd.U), new NumericsVector(Context, svd.S), new NumericsMatrix(Context, svd.VT));
        }

        public IFloatVector ReshapeAsVector()
        {
            return new NumericsVector(Context, _matrix.AsColumnMajorArray());
        }

		public I3DFloatTensor ReshapeAs3DTensor(uint rows, uint columns)
		{
			Debug.Assert(rows * columns == RowCount);
			var matrixList = new IIndexableFloatMatrix[ColumnCount];
			for (uint i = 0, len = ColumnCount; i < len; i++)
				matrixList[i] = Column(i).ReshapeAsMatrix(rows, columns).AsIndexable();
			return new Numerics3DTensor(Context, matrixList);
		}

		public I4DFloatTensor ReshapeAs4DTensor(uint rows, uint columns, uint depth)
		{
			var list = new IIndexable3DFloatTensor[ColumnCount];
			for (uint i = 0; i < ColumnCount; i++)
				list[i] = new Numerics3DTensor(Context, Column(i).Split(depth).Select(v => v.ReshapeAsMatrix(rows, columns).AsIndexable()).ToArray());
			return new Numerics4DTensor(Context, list);
		}

	    public float GetAt(uint row, uint column)
	    {
		    return this[row, column];
	    }

	    public void SetAt(uint row, uint column, float value)
	    {
		    this[row, column] = value;
	    }

	    public IFloatVector[] ColumnVectors() => Columns.Cast<IFloatVector>().ToArray();

	    public IFloatVector[] RowVectors() => Rows.Cast<IFloatVector>().ToArray();

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
                            row.Append('|');
                        row.Append(_matrix[i, j]);
                    }
                    writer.WriteValue(row.ToString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                return ret.ToString();
            }
        }
    }
}
