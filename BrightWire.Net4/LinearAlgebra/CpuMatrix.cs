using BrightWire.Helper;
using BrightWire.LinearAlgebra.Helper;
using BrightWire.Models;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace BrightWire.LinearAlgebra
{
    internal class CpuMatrix : IIndexableMatrix
    {
        readonly Matrix<float> _matrix;

        public bool IsValid { get { return true; } }

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

        public int AddRef()
        {
            // nop
            return 1;
        }

        public int Release()
        {
            // nop
            return 1;
        }

        public float this[int row, int column]
        {
            get
            {
                return _matrix[row, column];
            }

            set
            {
                _matrix[row, column] = value;
            }
        }

        public int ColumnCount
        {
            get
            {
                return _matrix.ColumnCount;
            }
        }

        public int RowCount
        {
            get
            {
                return _matrix.RowCount;
            }
        }

        public object WrappedObject
        {
            get
            {
                return _matrix;
            }
        }

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

        //public void AddInPlace(float delta)
        //{
        //    _matrix.MapInplace(v => v + delta);
        //}

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
                var ret = new FloatMatrix {
                    Row = new FloatVector[_matrix.RowCount]
                };
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

        public IEnumerable<float> Values
        {
            get
            {
                return _matrix.Enumerate();
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
            return new CpuMatrix((DenseMatrix)_matrix.Map(v => {
                return Convert.ToSingle(Math.Sqrt(v + valueAdjustment));
            }));
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

        //public void Normalise(MatrixGrouping group, NormalisationType type)
        //{
        //    if (type == NormalisationType.FeatureScale) {
        //        IEnumerable<Vector<float>> list = (group == MatrixGrouping.ByRow) ? _matrix.EnumerateRows() : _matrix.EnumerateColumns();
        //        var norm = list.Select(row => {
        //            float min = 0f, max = 0f;
        //            foreach (var val in row.Enumerate(Zeros.AllowSkip)) {
        //                if (val > max)
        //                    max = val;
        //                if (val < min)
        //                    min = val;
        //            }
        //            float range = max - min;
        //            return Tuple.Create(min, range);
        //        }).ToList();

        //        if (group == MatrixGrouping.ByRow)
        //            _matrix.MapIndexedInplace((x, y, v) => norm[x].Item2 > 0 ? (v - norm[x].Item1) / norm[x].Item2 : v);
        //        else
        //            _matrix.MapIndexedInplace((x, y, v) => norm[y].Item2 > 0 ? (v - norm[y].Item1) / norm[y].Item2 : v);
        //    }
        //    else if(type == NormalisationType.Standard) {
        //        IEnumerable<Vector<float>> list = (group == MatrixGrouping.ByRow) ? _matrix.EnumerateRows() : _matrix.EnumerateColumns();
        //        var norm = list.Select(row => {
        //            var mean = row.Average();
        //            var stdDev = Convert.ToSingle(Math.Sqrt(row.Average(c => Math.Pow(c - mean, 2))));
        //            return Tuple.Create(mean, stdDev);
        //        }).ToList();

        //        if(group == MatrixGrouping.ByRow)
        //            _matrix.MapIndexedInplace((x, y, v) => norm[x].Item2 != 0 ? (v - norm[x].Item1) / norm[x].Item2 : v);
        //        else
        //            _matrix.MapIndexedInplace((x, y, v) => norm[y].Item2 != 0 ? (v - norm[y].Item1) / norm[y].Item2 : v);
        //    }else if(type == NormalisationType.Euclidean || type == NormalisationType.Infinity || type == NormalisationType.Manhattan) {
        //        var p = (type == NormalisationType.Manhattan) ? 1.0 : (type == NormalisationType.Manhattan) ? 2.0 : double.PositiveInfinity;
        //        var norm = (group == MatrixGrouping.ByColumn) ? _matrix.NormalizeColumns(p) : _matrix.NormalizeRows(p);
        //        norm.CopyTo(_matrix);
        //    }
        //}

        public void UpdateRow(int index, IIndexableVector vector, int columnIndex)
        {
            for (var i = 0; i < vector.Count; i++)
                _matrix[index, columnIndex + i] = vector[i];
        }

        public void UpdateColumn(int index, IIndexableVector vector, int rowIndex)
        {
            for (var i = 0; i < vector.Count; i++)
                _matrix[rowIndex + i, index] = vector[i];
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
            using (var column = vector.ToColumnMatrix())
                return Multiply(column);
        }

        public (IMatrix U, IVector S, IMatrix VT) Svd()
        {
            var svd = _matrix.Svd(true);
            return (new CpuMatrix(svd.U), new CpuVector(svd.S), new CpuMatrix(svd.VT));
        }

        public IMatrix Rotate180()
        {
            var rowCount = RowCount;
            var columnCount = ColumnCount;
            var ret = DenseMatrix.Create(rowCount, columnCount, (i, j) => _matrix[rowCount - i - 1, columnCount - j - 1]);
            return new CpuMatrix(ret);
        }

        public IVector ConvertInPlaceToVector()
        {
            return new CpuVector(_matrix.ToColumnMajorArray());
        }

        //public I3DTensor MaxPool(int filterDepth, List<int[]> indexList)
        //{
        //    var size = (int)Math.Sqrt(RowCount);
        //    var output = Enumerable.Range(0, filterDepth).Select(i => new List<float>()).ToArray();

        //    for (int i = 0, len = RowCount; i < len; i++) {
        //        var row = Row(i);
        //        var parts = row.Split(filterDepth);
        //        var maxIndex = parts.Select(v => v.MaximumIndex()).ToArray();
        //        for (var j = 0; j < filterDepth; j++) {
        //            var index = maxIndex[j];
        //            var slice = parts[j].AsIndexable();
        //            output[j].Add(slice[index]);
        //        }
        //        indexList.Add(maxIndex);
        //    }
        //    var matrixList = new List<IMatrix>();
        //    foreach (var slice in output) {
        //        var rowList = new List<float[]>();
        //        for (var i = 0; i < size; i++)
        //            rowList.Add(slice.Skip(i * size).Take(size).ToArray());
        //        var matrix = DenseMatrix.Create(rowList.Count, size, (i, j) => rowList[i][j]);
        //        matrixList.Add(new CpuMatrix(matrix));
        //    }
        //    return new Cpu3DTensor(matrixList);
        //}

        //public IMatrix ReverseMaxPool(IMatrix error, int size, int filterSize, int filterDepth, IReadOnlyList<int[]> indexList)
        //{
        //    var filterIndex = 0;
        //    var filters = error.ConvertInPlaceToVector().Split(filterDepth);
        //    var sparseDictionary = Enumerable.Range(0, filterDepth).Select(i => new Dictionary<Tuple<int, int>, float>()).ToList();

        //    foreach (var item in filters) {
        //        var itemIndex = 0;
        //        int xOffset = 0, yOffset = 0;
        //        foreach (var value in item.AsIndexable().Values) {
        //            var maxIndex = indexList[itemIndex][filterIndex];
        //            var yIndex = maxIndex / filterSize;
        //            var xIndex = maxIndex % filterSize;
        //            sparseDictionary[filterIndex].Add(Tuple.Create(xOffset + xIndex, yOffset + yIndex), value);
        //            xOffset += filterSize;
        //            if (xOffset >= size) {
        //                yOffset += filterSize;
        //                xOffset = 0;
        //            }
        //            ++itemIndex;
        //        }
        //        ++filterIndex;
        //    }

        //    var ret = DenseMatrix.Create(size * size, filterDepth, (i, j) => {
        //        var y = i / size;
        //        var x = i % size;
        //        float val;
        //        if (sparseDictionary[j].TryGetValue(Tuple.Create(x, y), out val))
        //            return val;
        //        return 0f;
        //    });
        //    return new CpuMatrix(ret);
        //}

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
                        //for (var j = 0; j < ColumnCount; j++)
                        //    writer.WriteElementString("val", _matrix[i, j].ToString());
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }
                return ret.ToString();
            }
        }

        // down first
        //public IMatrix Im2Col(int filterWidth, int filterHeight, int stride)
        //{
        //    var rowList = new List<List<float>>();
        //    foreach(var filter in ConvolutionHelper.Default(ColumnCount, RowCount, filterWidth, filterHeight, stride)) {
        //        var row = new List<float>();
        //        foreach (var item in filter)
        //            row.Add(this[item.Y, item.X]);
        //    }
        //    var firstRow = rowList.First();
        //    return new CpuMatrix(DenseMatrix.Create(rowList.Count, firstRow.Count, (i, j) => rowList[i][j]));
        //}

        //public IMatrix AddPadding(int padding)
        //{
        //    if (padding > 0) {
        //        var newRows = RowCount + padding * 2;
        //        var newColumns = ColumnCount + padding * 2;
        //        var ret = new CpuMatrix(DenseMatrix.Create(newRows, newColumns, 0f));

        //        for (var j = 0; j < newRows; j++) {
        //            for (var i = 0; i < newColumns; i++) {
        //                if (i < padding || j < padding)
        //                    continue;
        //                else if (i >= newRows - padding || j >= newColumns - padding)
        //                    continue;
        //                ret[i, j] = this[i - padding, j - padding];
        //            }
        //        }
        //        return ret;
        //    }
        //    return this;
        //}

        //public IMatrix RemovePadding(int padding)
        //{
        //    if (padding > 0) {
        //        var newRows = RowCount - padding * 2;
        //        var newColumns = ColumnCount - padding * 2;
        //        var ret = new CpuMatrix(new DenseMatrix(newRows, newColumns));
        //        for (var j = 0; j < newRows; j++) {
        //            for (var i = 0; i < newColumns; i++) {
        //                ret[i, j] = this[i + padding, j + padding];
        //            }
        //        }
        //        return ret;
        //    }
        //    return this;
        //}
    }
}
