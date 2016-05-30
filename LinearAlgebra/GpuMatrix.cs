using ManagedCuda;
using ManagedCuda.CudaBlas;
using MathNet.Numerics.LinearAlgebra.Single;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icbld.BrightWire.LinearAlgebra
{
    internal class GpuMatrix : IMatrix
    {
        readonly CudaProvider _cuda;
        readonly CudaDeviceVariable<float> _data;
        readonly int _rows, _columns;
        bool _disposed = false;
#if DEBUG
        static int _gid = 0;
        int _id = _gid++;
        public static int _badAlloc = -1;
#endif

        public GpuMatrix(CudaProvider cuda, int rows, int columns, Func<int, int, float> init)
        {
            _cuda = cuda;
            _rows = rows;
            _columns = columns;

            var count = rows * columns;
            var data = new float[count];
            for (var i = 0; i < rows; i++) {
                for (var j = 0; j < columns; j++) {
                    data[j * rows + i] = init(i, j);
                }
            }
            _data = new CudaDeviceVariable<float>(count);
            _data.CopyToDevice(data);

#if DEBUG
            if (_id == _badAlloc)
                Debugger.Break();
#endif
        }

        public GpuMatrix(CudaProvider cuda, IIndexableMatrix matrix) : this(cuda, matrix.RowCount, matrix.ColumnCount, (j, k) => matrix[j, k])
        {
        }

        internal GpuMatrix(CudaProvider cuda, int rows, int columns, CudaDeviceVariable<float> gpuData)
        {
            _cuda = cuda;
            _rows = rows;
            _columns = columns;
            _data = gpuData;

#if DEBUG
            if (_id == _badAlloc)
                Debugger.Break();
#endif
        }

#if DEBUG
        ~GpuMatrix()
        {
            Debug.Assert(_disposed, String.Format("Matrix {0} was not disposed", _id));
        }
#endif

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed) {
                _data.Dispose();
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public override string ToString()
        {
            return AsIndexable().ToString();
        }

        public int ColumnCount
        {
            get
            {
                return _columns;
            }
        }

        public int RowCount
        {
            get
            {
                return _rows;
            }
        }

        public object WrappedObject
        {
            get
            {
                return _data;
            }
        }

        public IMatrix Add(IMatrix matrix)
        {
            var other = (GpuMatrix)matrix;
            var ret = new CudaDeviceVariable<float>(other._data.Size);
            ret.CopyToDevice(other._data);
            _cuda.Blas.Axpy(1.0f, _data, 1, ret, 1);
            return new GpuMatrix(_cuda, _rows, _columns, ret);
        }

        public void AddInPlace(IMatrix matrix, float coefficient1 = 1, float coefficient2 = 1)
        {
            var other = (GpuMatrix)matrix;
            _cuda.AddInPlace(_data, other._data, _rows * _columns, coefficient1, coefficient2);
        }

        public void AddToEachColumn(IVector vector)
        {
            _cuda.AddToEachColumn(_data, (CudaDeviceVariable<float>)vector.WrappedObject, _rows, _columns);
        }

        public void AddToEachRow(IVector vector)
        {
            _cuda.AddToEachRow(_data, (CudaDeviceVariable<float>)vector.WrappedObject, _rows, _columns);
        }

        public IIndexableMatrix AsIndexable()
        {
            var data = new float[_rows * _columns];
            _data.CopyToHost(data);
            return new CpuMatrix(DenseMatrix.Create(_rows, _columns, (j, k) => data[k * _rows + j]));
        }

        public void Clear()
        {
            _data.Memset(0);
        }

        public void ClearColumns(int[] indices)
        {
            int offset = 0;
            foreach (var item in indices) {
                _cuda.MemClear(_data, _rows, item * _rows);
                offset += _rows;
            }
        }

        public void ClearRows(int[] indices)
        {
            int offset = 0;
            foreach (var item in indices) {
                _cuda.MemClear(_data, _columns, item, _rows);
                offset += 1;
            }
        }

        public IMatrix Clone()
        {
            var ret = new CudaDeviceVariable<float>(_rows * _columns);
            ret.CopyToDevice(_data);
            return new GpuMatrix(_cuda, _rows, _columns, ret);
        }

        public IVector Column(int index)
        {
            var ret = new CudaDeviceVariable<float>(_rows);
            ret.CopyToDevice(_data, index * _rows * sizeof(float), 0, _rows * sizeof(float));
            return new GpuVector(_cuda, _rows, ret);
        }

        public IVector ColumnL2Norm()
        {
            var norm = new List<float>();
            for(var i = 0; i < _columns; i++) {
                using (var col = Column(i))
                    norm.Add(col.L2Norm());
            }
            return _cuda.Create(norm.Count, x => norm[x]);
        }

        public IVector ColumnSums(float coefficient = 1)
        {
            return new GpuVector(_cuda, _columns, _cuda.SumColumns(_data, _rows, _columns));
        }

        public IMatrix ConcatColumns(IMatrix bottom)
        {
            var t = this;
            var b = (GpuMatrix)bottom;
            Debug.Assert(ColumnCount == bottom.ColumnCount);
            var size = t.RowCount + b.RowCount;
            var ret = new CudaDeviceVariable<float>(size * t.ColumnCount);
            _cuda.ConcatColumns(t._data, b._data, ret, size, t.ColumnCount, t.RowCount, b.RowCount);
            return new GpuMatrix(_cuda, size, t.ColumnCount, ret);
        }

        public IMatrix ConcatRows(IMatrix right)
        {
            var t = this;
            var b = (GpuMatrix)right;
            Debug.Assert(RowCount == right.RowCount);
            var size = t.ColumnCount + b.ColumnCount;
            var ret = new CudaDeviceVariable<float>(t.RowCount * size);
            _cuda.ConcatRows(t._data, b._data, ret, t.RowCount, size, t.ColumnCount);
            return new GpuMatrix(_cuda, t.RowCount, size, ret);
        }

        public void Constrain(float min, float max)
        {
            _cuda.Constrain(_data, _rows * _columns, min, max);
        }

        public IVector Diagonal()
        {
            var ret = _cuda.Diagonal(_data, _rows, _columns);
            return new GpuVector(_cuda, Math.Min(_rows, _columns), ret);
        }

        public IVector GetColumnSegment(int index, int rowIndex, int length)
        {
            return AsIndexable().GetColumnSegment(index, rowIndex, length);
        }

        public IMatrix GetNewMatrixFromColumns(int[] columnIndices)
        {
            int offset = 0;
            var ret = new CudaDeviceVariable<float>(_rows * columnIndices.Length);
            foreach (var item in columnIndices) {
                ret.CopyToDevice(_data, item * _rows * sizeof(float), offset * sizeof(float), _rows * sizeof(float));
                offset += _rows;
            }
            return new GpuMatrix(_cuda, _rows, columnIndices.Length, ret);
        }

        public IMatrix GetNewMatrixFromRows(int[] rowIndices)
        {
            int offset = 0;
            var ret = new CudaDeviceVariable<float>(rowIndices.Length * _columns);
            foreach (var item in rowIndices) {
                CudaBlasNativeMethods.cublasScopy_v2(_cuda.Blas.CublasHandle,
                    n: _columns,
                    x: _data.DevicePointer + item * sizeof(float),
                    incx: _rows,
                    y: ret.DevicePointer + offset * sizeof(float),
                    incy: _columns
                );
                offset += 1;
            }
            return new GpuMatrix(_cuda, rowIndices.Length, _columns, ret);
        }

        public IVector GetRowSegment(int index, int columnIndex, int length)
        {
            return AsIndexable().GetRowSegment(index, columnIndex, length);
        }

        public void L1Regularisation(float coefficient)
        {
            _cuda.L1Regularisation(_data, _rows * _columns, coefficient);
        }

        public IMatrix LeakyReluActivation()
        {
            var ret = _cuda.LeakyRELU(_data, _rows, _columns);
            return new GpuMatrix(_cuda, _rows, _columns, ret);
        }

        public IMatrix LeakyReluDerivative()
        {
            var ret = _cuda.LeakyRELUDerivative(_data, _rows, _columns);
            return new GpuMatrix(_cuda, _rows, _columns, ret);
        }

        public void Multiply(float scalar)
        {
            _cuda.Blas.Scale(scalar, _data, 1);
        }

        public IMatrix Multiply(IMatrix matrix)
        {
            var other = (GpuMatrix)matrix;
            Debug.Assert(_columns == other._rows);
            var ret = new CudaDeviceVariable<float>(_rows * other.ColumnCount);
            int rowsA = _rows, columnsArowsB = _columns, columnsB = other.ColumnCount;

            float alpha = 1.0f, beta = 0.0f;
            CudaBlasNativeMethods.cublasSgemm_v2(_cuda.Blas.CublasHandle,
                Operation.NonTranspose,
                Operation.NonTranspose,
                rowsA,
                columnsB,
                columnsArowsB,
                ref alpha,
                _data.DevicePointer,
                rowsA,
                other._data.DevicePointer,
                columnsArowsB,
                ref beta,
                ret.DevicePointer,
                rowsA
            );
            return new GpuMatrix(_cuda, _rows, other.ColumnCount, ret);
        }

        public IMatrix PointwiseDivide(IMatrix matrix)
        {
            var other = (GpuMatrix)matrix;
            var size = _rows * _columns;
            var ret = _cuda.PointwiseDivide(_data, other._data, size);
            return new GpuMatrix(_cuda, _rows, _columns, ret);
        }

        public void PointwiseDivideColumns(IVector vector)
        {
            _cuda.PointwiseDivideColumns(_data, (CudaDeviceVariable<float>)vector.WrappedObject, _rows, _columns);
        }

        public void PointwiseDivideRows(IVector vector)
        {
            _cuda.PointwiseDivideRows(_data, (CudaDeviceVariable<float>)vector.WrappedObject, _rows, _columns);
        }

        public IMatrix PointwiseMultiply(IMatrix matrix)
        {
            var other = (GpuMatrix)matrix;
            var size = _rows * _columns;
            var ret = _cuda.PointwiseMultiply(_data, other._data, size);
            return new GpuMatrix(_cuda, _rows, _columns, ret);
        }

        public IMatrix Pow(float power)
        {
            var ret = _cuda.Pow(_data, _rows * _columns, power);
            return new GpuMatrix(_cuda, _rows, _columns, ret);
        }

        public void ReadFrom(BinaryReader reader)
        {
            var rowCount = reader.ReadInt32();
            var columnCount = reader.ReadInt32();
            var data = new float[_rows * _columns];

            for (var i = 0; i < rowCount; i++) {
                for (var j = 0; j < columnCount; j++) {
                    var val = reader.ReadSingle();
                    if (i < _rows && j < _columns)
                        data[j * _rows + i] = val;
                }
            }
            _data.CopyToDevice(data);
        }

        public IMatrix ReluActivation()
        {
            return new GpuMatrix(_cuda, _rows, _columns, _cuda.RELU(_data, _rows, _columns));
        }

        public IMatrix ReluDerivative()
        {
            return new GpuMatrix(_cuda, _rows, _columns, _cuda.RELUDerivative(_data, _rows, _columns));
        }

        public IVector Row(int index)
        {
            var ret = new CudaDeviceVariable<float>(_columns);
            int offset = index * sizeof(float);
            CudaBlasNativeMethods.cublasScopy_v2(_cuda.Blas.CublasHandle, _columns * sizeof(float), _data.DevicePointer + offset, _rows, ret.DevicePointer, 1);
            return new GpuVector(_cuda, _columns, ret);
        }

        public IVector RowL2Norm()
        {
            var norm = new List<float>();
            for (var i = 0; i < _rows; i++) {
                using (var row = Row(i))
                    norm.Add(row.L2Norm());
            }
            return _cuda.Create(norm.Count, x => norm[x]);
        }

        public IVector RowSums(float coefficient = 1)
        {
            return new GpuVector(_cuda, _rows, _cuda.SumRows(_data, _rows, _columns));
        }

        public IMatrix SigmoidActivation()
        {
            return new GpuMatrix(_cuda, _rows, _columns, _cuda.Sigmoid(_data, _rows, _columns));
        }

        public IMatrix SigmoidDerivative()
        {
            return new GpuMatrix(_cuda, _rows, _columns, _cuda.SigmoidDerivative(_data, _rows, _columns));
        }

        public Tuple<IMatrix, IMatrix> SplitColumns(int position)
        {
            int size = _rows - position;
            var ret1 = new CudaDeviceVariable<float>(position * _columns);
            var ret2 = new CudaDeviceVariable<float>(size * _columns);
            _cuda.SplitColumns(_data, ret1, ret2, _rows, _columns, position);
            return Tuple.Create<IMatrix, IMatrix>(new GpuMatrix(_cuda, position, _columns, ret1), new GpuMatrix(_cuda, size, _columns, ret2));
        }

        public Tuple<IMatrix, IMatrix> SplitRows(int position)
        {
            int size = _columns - position;
            var ret1 = new CudaDeviceVariable<float>(_rows * position);
            var ret2 = new CudaDeviceVariable<float>(_rows * size);
            _cuda.SplitRows(_data, ret1, ret2, _rows, _columns, position);
            return Tuple.Create<IMatrix, IMatrix>(new GpuMatrix(_cuda, _rows, position, ret1), new GpuMatrix(_cuda, _rows, size, ret2));
        }

        public IMatrix Sqrt(float valueAdjustment = 0)
        {
            var size = _rows * _columns;
            var ret = _cuda.Sqrt(_data, size, valueAdjustment);
            return new GpuMatrix(_cuda, _rows, _columns, ret);
        }

        public IMatrix Subtract(IMatrix matrix)
        {
            var other = (GpuMatrix)matrix;
            var ret = new CudaDeviceVariable<float>(_data.Size);
            ret.CopyToDevice(_data);
            _cuda.Blas.Axpy(-1.0f, other._data, 1, ret, 1);
            return new GpuMatrix(_cuda, _rows, _columns, ret);
        }

        public void SubtractInPlace(IMatrix matrix, float coefficient1 = 1, float coefficient2 = 1)
        {
            var other = (GpuMatrix)matrix;
            _cuda.SubtractInPlace(_data, other._data, _rows * _columns, coefficient1, coefficient2);
        }

        public IMatrix TanhActivation()
        {
            return new GpuMatrix(_cuda, _rows, _columns, _cuda.TanH(_data, _rows, _columns));
        }

        public IMatrix TanhDerivative()
        {
            return new GpuMatrix(_cuda, _rows, _columns, _cuda.TanHDerivative(_data, _rows, _columns));
        }

        public IMatrix Transpose()
        {
            var ret = new CudaDeviceVariable<float>(_rows * _columns);
            float alpha = 1.0f, beta = 0.0f;
            CudaBlasNativeMethods.cublasSgeam(_cuda.Blas.CublasHandle,
                Operation.Transpose,
                Operation.NonTranspose,
                _columns,
                _rows,
                ref alpha,
                _data.DevicePointer,
                _rows,
                ref beta,
                new ManagedCuda.BasicTypes.CUdeviceptr(0),
                _columns,
                ret.DevicePointer,
                _columns
            );
            return new GpuMatrix(_cuda, _columns, _rows, ret);
        }

        public IMatrix TransposeAndMultiply(IMatrix matrix)
        {
            var other = (GpuMatrix)matrix;
            Debug.Assert(_columns == other._columns);
            var ret = new CudaDeviceVariable<float>(_rows * other.RowCount);
            int rowsA = _rows, columnsArowsB = _columns, rowsB = other.RowCount;

            float alpha = 1.0f, beta = 0.0f;
            CudaBlasNativeMethods.cublasSgemm_v2(_cuda.Blas.CublasHandle,
                Operation.NonTranspose,
                Operation.Transpose,
                rowsA,
                rowsB,
                columnsArowsB,
                ref alpha,
                _data.DevicePointer,
                rowsA,
                other._data.DevicePointer,
                rowsB,
                ref beta,
                ret.DevicePointer,
                rowsA
            );
            return new GpuMatrix(_cuda, _rows, other.RowCount, ret);
        }

        public IMatrix TransposeThisAndMultiply(IMatrix matrix)
        {
            var other = (GpuMatrix)matrix;
            Debug.Assert(_rows == other._rows);

            var ret = new CudaDeviceVariable<float>(_columns * other.ColumnCount);
            int rowsA = _rows, columnsA = _columns, columnsB = other.ColumnCount, rowsB = other.RowCount;

            float alpha = 1.0f, beta = 0.0f;
            CudaBlasNativeMethods.cublasSgemm_v2(_cuda.Blas.CublasHandle,
                Operation.Transpose,
                Operation.NonTranspose,
                columnsA,
                columnsB,
                rowsB,
                ref alpha,
                _data.DevicePointer,
                rowsA,
                other._data.DevicePointer,
                rowsB,
                ref beta,
                ret.DevicePointer,
                columnsA
            );
            return new GpuMatrix(_cuda, _columns, other.ColumnCount, ret);
        }

        public void UpdateColumn(int index, IIndexableVector vector, int rowIndex)
        {
            // TODO: use faster BLAS routine
            for (var i = 0; i < vector.Count; i++)
                _data[index * _rows + rowIndex + i] = vector[i];
        }

        public void UpdateRow(int index, IIndexableVector vector, int columnIndex)
        {
            // TODO: use faster BLAS routine
            for (var i = 0; i < vector.Count; i++)
                _data[(columnIndex+i) * _rows + index] = vector[i];
        }

        public void WriteTo(BinaryWriter writer)
        {
            var data = new float[_rows * _columns];
            _data.CopyToHost(data);

            writer.Write(_rows);
            writer.Write(_columns);
            for (var i = 0; i < _rows; i++) {
                for (var j = 0; j < _columns; j++) {
                    writer.Write(data[j * _rows + i]);
                }
            }
        }
    }
}
