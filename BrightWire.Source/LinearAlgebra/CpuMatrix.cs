using BrightWire.LinearAlgebra.Helper;
using BrightWire.Models;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BrightWire.LinearAlgebra
{
    /// <summary>
    /// Matrix that uses the CPU based math.net numerics library
    /// </summary>
    class CpuMatrix : IIndexableMatrix
    {
        readonly Matrix<float> _matrix;

        public bool IsValid => true;

	    public CpuMatrix(DenseMatrix matrix)
        {
            _matrix = matrix;
        }

        public CpuMatrix(Matrix<float> matrix)
        {
            _matrix = matrix;
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

        public float this[int row, int column]
        {
            get => _matrix[row, column];
	        set => _matrix[row, column] = value;
        }

        public int ColumnCount => _matrix.ColumnCount;
	    public int RowCount => _matrix.RowCount;
	    public IEnumerable<float> Values => _matrix.Enumerate();

	    public IVector Column(int index)
        {
            return new CpuVector(_matrix.Column(index));
        }

        public IIndexableMatrix Map(Func<float, float> mutator)
        {
            return new CpuMatrix(_matrix.Map(mutator));
        }

        public IIndexableMatrix MapIndexed(Func<int, int, float, float> mutator)
        {
            return new CpuMatrix(_matrix.MapIndexed(mutator));
        }

        public IMatrix Multiply(IMatrix matrix)
        {
            var other = (CpuMatrix)matrix;
            return new CpuMatrix(_matrix.Multiply(other._matrix));
        }

        public IMatrix PointwiseMultiply(IMatrix matrix)
        {
            var other = (CpuMatrix)matrix;
            Debug.Assert(RowCount == matrix.RowCount && ColumnCount == matrix.ColumnCount);
            return new CpuMatrix(_matrix.PointwiseMultiply(other._matrix));
        }

        public IVector RowSums()
        {
            var ret = _matrix.RowSums();
            return new CpuVector(ret);
        }

        public IVector ColumnSums()
        {
            var ret = _matrix.ColumnSums();
            return new CpuVector(ret);
        }

        public IMatrix Add(IMatrix matrix)
        {
            var other = (CpuMatrix)matrix;
            return new CpuMatrix(_matrix.Add(other._matrix));
        }

        public IMatrix Subtract(IMatrix matrix)
        {
            var other = (CpuMatrix)matrix;
            return new CpuMatrix(_matrix.Subtract(other._matrix));
        }

        public IMatrix TransposeAndMultiply(IMatrix matrix)
        {
            var other = (CpuMatrix)matrix;
            return new CpuMatrix(_matrix.TransposeAndMultiply(other._matrix));
        }

        public IMatrix TransposeThisAndMultiply(IMatrix matrix)
        {
            var other = (CpuMatrix)matrix;
            return new CpuMatrix(_matrix.TransposeThisAndMultiply(other._matrix));
        }

        public IMatrix Transpose()
        {
            return new CpuMatrix(_matrix.Transpose());
        }

        public void Multiply(float scalar)
        {
            _matrix.MapInplace(v => v * scalar);
        }

        public override string ToString()
        {
            return _matrix.ToMatrixString();
        }

        public void AddInPlace(IMatrix matrix, float coefficient1 = 1.0f, float coefficient2 = 1.0f)
        {
            Debug.Assert(RowCount == matrix.RowCount && ColumnCount == matrix.ColumnCount);
            var other = (CpuMatrix)matrix;
            _matrix.MapIndexedInplace((i, j, v) => (v * coefficient1) + (other[i, j] * coefficient2));
        }

        public void SubtractInPlace(IMatrix matrix, float coefficient1 = 1.0f, float coefficient2 = 1.0f)
        {
            Debug.Assert(RowCount == matrix.RowCount && ColumnCount == matrix.ColumnCount);
            var other = (CpuMatrix)matrix;
            _matrix.MapIndexedInplace((i, j, v) => (v * coefficient1) - (other[i, j] * coefficient2));
        }

        internal static float _Sigmoid(float val)
        {
            return BoundMath.Constrain(1.0f / (1.0f + BoundMath.Exp(-1.0f * val)));
        }

        internal static float _SigmoidDerivative(float val)
        {
            var score = _Sigmoid(val);
            return BoundMath.Constrain(score * (1.0f - score));
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
            return (val <= 0) ? 0 : BoundMath.Constrain(val);
        }

        internal static float _ReluDerivative(float val)
        {
            return (val <= 0) ? 0f : 1;
        }

        internal static float _LeakyRelu(float val)
        {
            return (val <= 0) ? 0.01f * val : BoundMath.Constrain(val);
        }

        internal static float _LeakyReluDerivative(float val)
        {
            return (val <= 0) ? 0.01f : 1;
        }

        public IMatrix ReluActivation()
        {
            return new CpuMatrix(_matrix.Map(_Relu));
        }

        public IMatrix ReluDerivative()
        {
            return new CpuMatrix(_matrix.Map(_ReluDerivative));
        }

        public IMatrix LeakyReluActivation()
        {
            return new CpuMatrix(_matrix.Map(_LeakyRelu));
        }

        public IMatrix LeakyReluDerivative()
        {
            return new CpuMatrix(_matrix.Map(_LeakyReluDerivative));
        }

        public IMatrix SigmoidActivation()
        {
            return new CpuMatrix(_matrix.Map(_Sigmoid));
        }

        public IMatrix SigmoidDerivative()
        {
            return new CpuMatrix(_matrix.Map(_SigmoidDerivative));
        }

        public IMatrix TanhActivation()
        {
            return new CpuMatrix(_matrix.Map(_Tanh));
        }

        public IMatrix TanhDerivative()
        {
            return new CpuMatrix(_matrix.Map(_TanhDerivative));
        }

        public IMatrix SoftmaxActivation()
        {
            var activation = Rows.Select(r => r.Softmax().AsIndexable()).ToList();
            return new CpuMatrix(DenseMatrix.Create(RowCount, ColumnCount, (x, y) => activation[x][y]));
        }

        public void AddToEachRow(IVector vector)
        {
            var other = (CpuVector)vector;
            _matrix.MapIndexedInplace((j, k, v) => v + other[k]);
        }

        public void AddToEachColumn(IVector vector)
        {
            var other = (CpuVector)vector;
            _matrix.MapIndexedInplace((j, k, v) => v + other[j]);
        }

        public FloatMatrix Data
        {
            get
            {
                var ret = FloatMatrix.Create(new FloatVector[_matrix.RowCount]);
                for (var i = 0; i < _matrix.RowCount; i++) {
                    var row = new float[_matrix.ColumnCount];
                    for (var j = 0; j < _matrix.ColumnCount; j++) {
                        row[j] = _matrix[i, j];
                    }
                    ret.Row[i] = new FloatVector {
                        Data = row
                    };
                }
                return ret;
            }

            set
            {
                var arrayList = value.Row;
                var rowCount = arrayList.Length;
                for (var i = 0; i < rowCount; i++) {
                    var row = arrayList[i];
                    if (row.Data != null) {
                        var data = row.Data;
                        var columnCount = data.Length;
                        for (var j = 0; j < columnCount; j++) {
                            if (i < _matrix.RowCount && j < _matrix.ColumnCount)
                                _matrix[i, j] = data[j];
                        }
                    }
                }
            }
        }

        public IIndexableMatrix AsIndexable()
        {
            return this;
        }

        public IVector Row(int index)
        {
            return new CpuVector(_matrix.Row(index));
        }

        public IEnumerable<IIndexableVector> Rows
        {
            get
            {
                return _matrix.EnumerateRows().Select(v => new CpuVector(v));
            }
        }

        public IEnumerable<IIndexableVector> Columns
        {
            get
            {
                return _matrix.EnumerateColumns().Select(v => new CpuVector(v));
            }
        }

	    public IMatrix GetNewMatrixFromRows(IReadOnlyList<int> rowIndexes)
        {
            return new CpuMatrix(DenseMatrix.Create(rowIndexes.Count, ColumnCount, (x, y) => _matrix[rowIndexes[x], y]));
        }

        public IMatrix GetNewMatrixFromColumns(IReadOnlyList<int> columnIndexes)
        {
            return new CpuMatrix(DenseMatrix.Create(RowCount, columnIndexes.Count, (x, y) => _matrix[x, columnIndexes[y]]));
        }

        public void ClearRows(IReadOnlyList<int> indexes)
        {
            _matrix.ClearRows(indexes.ToArray());
        }

        public void ClearColumns(IReadOnlyList<int> indexes)
        {
            _matrix.ClearColumns(indexes.ToArray());
        }

        public IMatrix Clone()
        {
            return new CpuMatrix(DenseMatrix.OfMatrix(_matrix));
        }

        public void Clear()
        {
            _matrix.Clear();
        }

        public IMatrix ConcatColumns(IMatrix bottom)
        {
            var t = this;
            var b = (CpuMatrix)bottom;
            Debug.Assert(ColumnCount == bottom.ColumnCount);

            var ret = DenseMatrix.Create(t.RowCount + b.RowCount, t.ColumnCount, (x, y) => {
                var m = x >= t.RowCount ? b._matrix : t._matrix;
                return m[x >= t.RowCount ? x - t.RowCount : x, y];
            });
            return new CpuMatrix(ret);
        }

        public IMatrix ConcatRows(IMatrix right)
        {
            var t = this;
            var b = (CpuMatrix)right;
            Debug.Assert(RowCount == right.RowCount);

            var ret = DenseMatrix.Create(t.RowCount, t.ColumnCount + b.ColumnCount, (x, y) => {
                var m = y >= t.ColumnCount ? b._matrix : t._matrix;
                return m[x, y >= t.ColumnCount ? y - t.ColumnCount : y];
            });
            return new CpuMatrix(ret);
        }

        public (IMatrix Left, IMatrix Right) SplitAtColumn(int columnIndex)
        {
            var ret1 = DenseMatrix.Create(RowCount, columnIndex, (x, y) => this[x, y]);
            var ret2 = DenseMatrix.Create(RowCount, ColumnCount - columnIndex, (x, y) => this[x, columnIndex + y]);
            return (new CpuMatrix(ret1), new CpuMatrix(ret2));
        }

        public (IMatrix Top, IMatrix Bottom) SplitAtRow(int rowIndex)
        {
            var ret1 = DenseMatrix.Create(rowIndex, ColumnCount, (x, y) => this[x, y]);
            var ret2 = DenseMatrix.Create(RowCount - rowIndex, ColumnCount, (x, y) => this[rowIndex + x, y]);
            return (new CpuMatrix(ret1), new CpuMatrix(ret2));
        }

        public IMatrix Sqrt(float valueAdjustment = 0)
        {
            return new CpuMatrix((DenseMatrix)_matrix.Map(v => Convert.ToSingle(Math.Sqrt(v + valueAdjustment))));
        }

        public IMatrix PointwiseDivide(IMatrix matrix)
        {
            var other = (CpuMatrix)matrix;
            return new CpuMatrix(_matrix.PointwiseDivide(other._matrix));
        }

        public void L1Regularisation(float coefficient)
        {
            _matrix.MapInplace(v => v - ((v > 0 ? 1 : v < 0 ? -1 : 0) * coefficient));
        }

        public void Constrain(float min, float max)
        {
            _matrix.MapInplace(v => v < min ? min : v > max ? max : v);
        }

        public IVector ColumnL2Norm()
        {
            var ret = _matrix.ColumnNorms(2.0);
            return new CpuVector(DenseVector.Create(ret.Count, i => Convert.ToSingle(ret[i])));
        }

        public IVector RowL2Norm()
        {
            var ret = _matrix.RowNorms(2.0);
            return new CpuVector(DenseVector.Create(ret.Count, i => Convert.ToSingle(ret[i])));
        }

        public void PointwiseDivideRows(IVector vector)
        {
            var v2 = vector.AsIndexable();
            _matrix.MapIndexedInplace((x, y, v) => v /= v2[x]);
        }

        public void PointwiseDivideColumns(IVector vector)
        {
            var v2 = vector.AsIndexable();
            _matrix.MapIndexedInplace((x, y, v) => v /= v2[y]);
        }

        public IVector Diagonal()
        {
            return new CpuVector((DenseVector)_matrix.Diagonal());
        }

        public IMatrix Pow(float power)
        {
            return new CpuMatrix(_matrix.Map(v => Convert.ToSingle(Math.Pow(v, power))));
        }

        public IVector GetRowSegment(int index, int columnIndex, int length)
        {
            var buffer = new float[length];
            for (var i = 0; i < length; i++)
                buffer[i] = _matrix[index, columnIndex + i];
            return new CpuVector(DenseVector.OfArray(buffer));
        }

        public IVector GetColumnSegment(int columnIndex, int rowIndex, int length)
        {
            var buffer = new float[length];
            for (var i = 0; i < length; i++)
                buffer[i] = _matrix[rowIndex + i, columnIndex];
            return new CpuVector(DenseVector.OfArray(buffer));
        }

        public IMatrix Multiply(IVector vector)
        {
            using (var column = vector.ReshapeAsColumnMatrix())
                return Multiply(column);
        }

        public (IMatrix U, IVector S, IMatrix VT) Svd()
        {
            var svd = _matrix.Svd(true);
            return (new CpuMatrix(svd.U), new CpuVector(svd.S), new CpuMatrix(svd.VT));
        }

        public IVector ReshapeAsVector()
        {
            return new CpuVector(_matrix.ToColumnMajorArray());
        }

		public I3DTensor ReshapeAs3DTensor(int rows, int columns)
		{
			Debug.Assert(rows * columns == RowCount);
			var matrixList = new List<IMatrix>();
			for (int i = 0, len = ColumnCount; i < len; i++)
				matrixList.Add(Column(i).ReshapeAsMatrix(rows, columns));
			return new Cpu3DTensor(matrixList);
		}

		public I4DTensor ReshapeAs4DTensor(int rows, int columns, int depth)
		{
			var list = new List<I3DTensor>();
			for (var i = 0; i < ColumnCount; i++)
				list.Add(new Cpu3DTensor(Column(i).Split(depth).Select(v => v.ReshapeAsMatrix(rows, columns)).ToList()));
			return new Cpu4DTensor(list);
		}

	    public IMatrix CalculateDistance(IReadOnlyList<IVector> comparison, DistanceMetric distanceMetric)
	    {
		    var columns = Enumerable.Range(0, ColumnCount).Select(Column).ToList();
		    var result = new ConcurrentDictionary<(long, long), float>();
		    Parallel.ForEach(columns, (column1, _, i) => { 
			    Parallel.ForEach(comparison, (column2, __, j) => {
				    result[(i,j)] = column1.FindDistance(column2, distanceMetric);
			    });
		    });
		    return new CpuMatrix(DenseMatrix.Create(comparison.Count, ColumnCount, (i, j) => result[(j, i)]));
	    }

	    public string AsXml
        {
            get
            {
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
