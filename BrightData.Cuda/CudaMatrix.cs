using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using BrightData.LinearAlgebra;
using ManagedCuda;
using ManagedCuda.BasicTypes;
using ManagedCuda.CudaBlas;

namespace BrightData.Cuda
{
    /// <summary>
    /// GPU backed matrix
    /// </summary>
    internal class CudaMatrix : IFloatMatrix, IHaveDeviceMemory
    {
        readonly CudaProvider _cuda;
        readonly IDeviceMemoryPtr _data;
        readonly uint _rows, _columns;
        bool _disposed = false;
#if DEBUG
        static int _gid = 0;
        static int GetNextIndex() => Interlocked.Increment(ref _gid);
        readonly int _id = GetNextIndex();
        public static int _badAlloc = -1;
        public static int _badDispose = -1;

        public bool IsValid => !_disposed;
#else
        public bool IsValid => true;
#endif

        public CudaMatrix(CudaProvider cuda, uint rows, uint columns, IDeviceMemoryPtr data, bool isOwner)
        {
	        Debug.Assert(rows * columns == data.Size);
            _cuda = cuda;
            _rows = rows;
            _columns = columns;
            _data = data;
            cuda.Register(this);
#if DEBUG
            if (_id == _badAlloc)
                Debugger.Break();
#endif
        }

#if DEBUG
        ~CudaMatrix()
        {
            if (!_disposed)
                Debug.WriteLine("\tMatrix {0} was not disposed !!", _id);
        }
#endif

        protected virtual void Dispose(bool disposing)
        {
#if DEBUG
            if (_id == _badDispose)
                Debugger.Break();
#endif
            if (disposing && !_disposed) {
				_data.Free();
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
#if DEBUG
            GC.SuppressFinalize(this);
#endif
        }

        public override string ToString()
        {
            return AsIndexable().ToString();
        }

        public ILinearAlgebraProvider LinearAlgebraProvider => _cuda.DataContext.LinearAlgebraProvider;
        public uint ColumnCount => _columns;
	    public uint RowCount => _rows;
	    public IDeviceMemoryPtr Memory => _data;

        public IFloatMatrix Add(IFloatMatrix matrix)
        {
            Debug.Assert(IsValid && matrix.IsValid);
            var other = (CudaMatrix)matrix;
            Debug.Assert(other._rows == _rows && other._columns == _columns);

            var ret = _cuda.Allocate(other._data.Size);
            ret.CopyToDevice(other._data);
            _cuda.Blas.Axpy(1.0f, _data.DeviceVariable, 1, ret.DeviceVariable, 1);
            return new CudaMatrix(_cuda, _rows, _columns, ret, true);
        }

        public void AddInPlace(IFloatMatrix matrix, float coefficient1 = 1, float coefficient2 = 1)
        {
            Debug.Assert(IsValid && matrix.IsValid);
            var other = (CudaMatrix)matrix;
            Debug.Assert(other._rows == _rows && other._columns == _columns);

            _cuda.AddInPlace(_data, other._data, _rows * _columns, coefficient1, coefficient2);
        }

        public void AddToEachColumn(IFloatVector vector)
        {
            Debug.Assert(IsValid && vector.IsValid);
            var other = (CudaVector)vector;
            _cuda.AddToEachColumn(_data, other.Memory, _rows, _columns);
        }

        public void AddToEachRow(IFloatVector vector)
        {
            Debug.Assert(IsValid && vector.IsValid);
            var other = (CudaVector)vector;
            _cuda.AddToEachRow(_data, other.Memory, _rows, _columns);
        }

        public IIndexableFloatMatrix AsIndexable()
        {
            Debug.Assert(IsValid);
            var data = new float[_rows * _columns];
            _data.CopyToHost(data);
            return _cuda.NumericsProvider.CreateMatrix(_rows, _columns, (j, k) => data[k * _rows + j]).AsIndexable();
        }

        public void Clear()
        {
            Debug.Assert(IsValid);
            _data.DeviceVariable.Memset(0);
        }

        public void ClearColumns(IEnumerable<uint> indices)
        {
            Debug.Assert(IsValid);
            foreach (var item in indices)
                _cuda.MemClear(_data, _rows, item * _rows);
        }

        public void ClearRows(IEnumerable<uint> indices)
        {
            Debug.Assert(IsValid);
            foreach (var item in indices)
                _cuda.MemClear(_data, _columns, item, _rows);
        }

        public IFloatMatrix Clone()
        {
            Debug.Assert(IsValid);
            var ret = _cuda.Allocate(_rows * _columns);
            ret.CopyToDevice(_data);
            return new CudaMatrix(_cuda, _rows, _columns, ret, true);
        }

        public IFloatVector Column(uint index)
        {
            Debug.Assert(IsValid);
	        var ptr = _cuda.OffsetByBlock(_data, index, _rows);
            return new CudaVector(_cuda, ptr, false);
        }

        public IFloatVector ColumnL2Norm()
        {
            Debug.Assert(IsValid);
            var norm = new List<float>();
            for(uint i = 0; i < _columns; i++) {
                using var col = Column(i);
                norm.Add(col.L2Norm());
            }
            return _cuda.CreateVector((uint)norm.Count, x => norm[(int)x]);
        }

        public IFloatVector ColumnSums()
        {
            Debug.Assert(IsValid);
            return new CudaVector(_cuda, _cuda.SumColumns(_data, _rows, _columns), true);
        }

        public IFloatMatrix ConcatColumns(IFloatMatrix bottom)
        {
            Debug.Assert(IsValid && bottom.IsValid);
            var t = this;
            var b = (CudaMatrix)bottom;
            Debug.Assert(ColumnCount == bottom.ColumnCount);
            var size = t.RowCount + b.RowCount;
            var ret = _cuda.Allocate(size * t.ColumnCount);
            _cuda.ConcatColumns(t._data, b._data, ret, size, t.ColumnCount, t.RowCount, b.RowCount);
            return new CudaMatrix(_cuda, size, t.ColumnCount, ret, true);
        }

        public IFloatMatrix ConcatRows(IFloatMatrix right)
        {
            Debug.Assert(IsValid && right.IsValid);
            var t = this;
            var b = (CudaMatrix)right;
            Debug.Assert(RowCount == right.RowCount);
            var size = t.ColumnCount + b.ColumnCount;
            var ret = _cuda.Allocate(t.RowCount * size);
            _cuda.ConcatRows(t._data, b._data, ret, t.RowCount, size, t.ColumnCount);
            return new CudaMatrix(_cuda, t.RowCount, size, ret, true);
        } 

        public void Constrain(float min, float max)
        {
            Debug.Assert(IsValid);
            _cuda.Constrain(_data, _rows * _columns, min, max);
        }

        public IFloatVector Diagonal()
        {
            Debug.Assert(IsValid);
            var ret = _cuda.Diagonal(_data, _rows, _columns);
            return new CudaVector(_cuda, ret, true);
        }

        public IFloatVector GetColumnSegment(uint columnIndex, uint rowIndex, uint length)
        {
            Debug.Assert(IsValid);

            var ret = _cuda.Allocate(length);
            ret.DeviceVariable.CopyToDevice(_data.DeviceVariable, ((columnIndex * _rows) + rowIndex) * CudaProvider.FloatSize, 0, length * CudaProvider.FloatSize);

            return new CudaVector(_cuda, ret, true);
        }

        public IFloatMatrix GetNewMatrixFromColumns(IEnumerable<uint> columnIndices)
        {
            Debug.Assert(IsValid);
            uint offset = 0;
            var indices = columnIndices.ToList();
            var ret = _cuda.Allocate(_rows * (uint)indices.Count);
            foreach (var item in indices) {
                ret.DeviceVariable.CopyToDevice(_data.DeviceVariable, item * _rows * CudaProvider.FloatSize, offset * CudaProvider.FloatSize, _rows * CudaProvider.FloatSize);
                offset += _rows;
            }
            return new CudaMatrix(_cuda, _rows, (uint)indices.Count, ret, true);
        }

        public IFloatMatrix GetNewMatrixFromRows(IEnumerable<uint> rowIndices)
        {
            Debug.Assert(IsValid);
            int offset = 0;
            var indices = rowIndices.ToList();
            var ret = _cuda.Allocate(_columns * (uint)indices.Count);
            foreach (var item in indices) {
                CudaBlasNativeMethods.cublasScopy_v2(_cuda.Blas.CublasHandle,
                    n: (int)_columns,
                    x: _data.DevicePointer + (item * CudaProvider.FloatSize),
                    incx: (int)_rows,
                    y: ret.DevicePointer + (offset * CudaProvider.FloatSize),
                    incy: indices.Count
                );
                offset += 1;
            }
            return new CudaMatrix(_cuda, (uint)indices.Count, _columns, ret, true);
        }

        public IFloatVector GetRowSegment(uint rowIndex, uint columnIndex, uint length)
        {
            Debug.Assert(IsValid);
            var offset = (rowIndex + (columnIndex * _rows)) * CudaProvider.FloatSize;
            var ret = _cuda.Allocate(length);
            CudaBlasNativeMethods.cublasScopy_v2(_cuda.Blas.CublasHandle, (int)length, _data.DevicePointer + offset, (int)_rows, ret.DevicePointer, 1);
            return new CudaVector(_cuda, ret, true);
        }

        public void L1Regularisation(float coefficient)
        {
            Debug.Assert(IsValid);
            _cuda.L1Regularisation(_data, _rows * _columns, coefficient);
        }

        public IFloatMatrix LeakyReluActivation()
        {
            Debug.Assert(IsValid);
            var ret = _cuda.LeakyRelu(_data, _rows * _columns);
            return new CudaMatrix(_cuda, _rows, _columns, ret, true);
        }

        public IFloatMatrix LeakyReluDerivative()
        {
            Debug.Assert(IsValid);
            var ret = _cuda.LeakyReluDerivative(_data, _rows * _columns);
            return new CudaMatrix(_cuda, _rows, _columns, ret, true);
        }

        public void Multiply(float scalar)
        {
            Debug.Assert(IsValid);
            _cuda.Blas.Scale(scalar, _data.DeviceVariable, 1);
        }

        public IFloatMatrix Multiply(IFloatMatrix matrix)
        {
            Debug.Assert(IsValid && matrix.IsValid);
            var other = (CudaMatrix)matrix;
            Debug.Assert(_columns == other._rows);
            var ret = _cuda.Allocate(_rows * other.ColumnCount);
            int rowsA = (int)_rows, columnsArowsB = (int)_columns, columnsB = (int)other.ColumnCount;

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
            return new CudaMatrix(_cuda, _rows, other.ColumnCount, ret, true);
        }

        public IFloatMatrix PointwiseDivide(IFloatMatrix matrix)
        {
            Debug.Assert(IsValid && matrix.IsValid);
            var other = (CudaMatrix)matrix;
            Debug.Assert(other._rows == _rows && other._columns == _columns);

            var size = _rows * _columns;
            var ret = _cuda.PointwiseDivide(_data, other._data, size);
            return new CudaMatrix(_cuda, _rows, _columns, ret, true);
        }

        public void PointwiseDivideColumns(IFloatVector vector)
        {
            Debug.Assert(IsValid && vector.IsValid);
            var other = (CudaVector)vector;
            _cuda.PointwiseDivideColumns(_data, other.Memory, _rows, _columns);
        }

        public void PointwiseDivideRows(IFloatVector vector)
        {
            Debug.Assert(IsValid && vector.IsValid);
            var other = (CudaVector)vector;
            _cuda.PointwiseDivideRows(_data, other.Memory, _rows, _columns);
        }

        public IFloatMatrix PointwiseMultiply(IFloatMatrix matrix)
        {
            Debug.Assert(IsValid && matrix.IsValid);
            var other = (CudaMatrix)matrix;
            Debug.Assert(other._rows == _rows && other._columns == _columns);

            var size = _rows * _columns;
            var ret = _cuda.PointwiseMultiply(_data, other._data, size);
            return new CudaMatrix(_cuda, _rows, _columns, ret, true);
        }

        public IFloatMatrix Pow(float power)
        {
            Debug.Assert(IsValid);
            var ret = _cuda.Pow(_data, _rows * _columns, power);
            return new CudaMatrix(_cuda, _rows, _columns, ret, true);
        }

        public IFloatMatrix ReluActivation()
        {
            Debug.Assert(IsValid);
            return new CudaMatrix(_cuda, _rows, _columns, _cuda.Relu(_data, _rows * _columns), true);
        }

        public IFloatMatrix ReluDerivative()
        {
            Debug.Assert(IsValid);
            return new CudaMatrix(_cuda, _rows, _columns, _cuda.ReluDerivative(_data, _rows * _columns), true);
        }

        public IFloatVector Row(uint index)
        {
            Debug.Assert(IsValid);
            var ret = _cuda.Allocate(_columns);
            var offset = index * CudaProvider.FloatSize;
            CudaBlasNativeMethods.cublasScopy_v2(_cuda.Blas.CublasHandle, (int)_columns, _data.DevicePointer + offset, (int)_rows, ret.DevicePointer, 1);
            return new CudaVector(_cuda, ret, true);
        }

        public IFloatVector RowL2Norm()
        {
            Debug.Assert(IsValid);
            var norm = new List<float>();
            for (uint i = 0; i < _rows; i++) {
                using var row = Row(i);
                norm.Add(row.L2Norm());
            }
            return _cuda.CreateVector((uint)norm.Count, x => norm[(int)x]);
        }

        public IFloatVector RowSums()
        {
            Debug.Assert(IsValid);
            return new CudaVector(_cuda, _cuda.SumRows(_data, _rows, _columns), true);
        }

        public IFloatMatrix SigmoidActivation()
        {
            Debug.Assert(IsValid);
            return new CudaMatrix(_cuda, _rows, _columns, _cuda.Sigmoid(_data, _rows * _columns), true);
        }

        public IFloatMatrix SigmoidDerivative()
        {
            Debug.Assert(IsValid);
            return new CudaMatrix(_cuda, _rows, _columns, _cuda.SigmoidDerivative(_data, _rows * _columns), true);
        }

        public IFloatMatrix SoftmaxActivation()
        {
            Debug.Assert(IsValid);
            var rowOutput = new List<CudaVector>();
            for (uint i = 0; i < _rows; i++) {
                using var row = Row(i);
                rowOutput.Add(row.Softmax() as CudaVector);
            }
            var ret = _cuda.Allocate(_rows * _columns);
            for (var i = 0; i < _rows; i++) {
                using var row = rowOutput[i];
                ret.DeviceVariable.CopyToDevice(row.Memory.DeviceVariable, 0, _columns * i * CudaProvider.FloatSize, _columns * CudaProvider.FloatSize);
            }

            using var temp = new CudaMatrix(_cuda, _columns, _rows, ret, true);
            return temp.Transpose();
        }

        public (IFloatMatrix Top, IFloatMatrix Bottom) SplitAtRow(uint rowIndex)
        {
            Debug.Assert(IsValid);
            var size = _rows - rowIndex;
            var ret1 = _cuda.Allocate(rowIndex * _columns);
            var ret2 = _cuda.Allocate(size * _columns);
            _cuda.SplitColumns(_data, ret1, ret2, _rows, _columns, rowIndex);
            return (new CudaMatrix(_cuda, rowIndex, _columns, ret1, true), new CudaMatrix(_cuda, size, _columns, ret2, true));
        }

        public (IFloatMatrix Left, IFloatMatrix Right) SplitAtColumn(uint columnIndex)
        {
            Debug.Assert(IsValid);
            var size = _columns - columnIndex;
            var ret1 = _cuda.Allocate(_rows * columnIndex);
            var ret2 = _cuda.Allocate(_rows * size);
            _cuda.SplitRows(_data, ret1, ret2, _rows, _columns, columnIndex);
            return (new CudaMatrix(_cuda, _rows, columnIndex, ret1, true), new CudaMatrix(_cuda, _rows, size, ret2, true));
        }

        public IFloatMatrix Sqrt()
        {
            Debug.Assert(IsValid);
            var size = _rows * _columns;
            var ret = _cuda.Sqrt(_data, size, 1e-8f);
            return new CudaMatrix(_cuda, _rows, _columns, ret, true);
        }

        public IFloatMatrix Subtract(IFloatMatrix matrix)
        {
            Debug.Assert(IsValid && matrix.IsValid);
            var other = (CudaMatrix)matrix;
            Debug.Assert(other._rows == _rows && other._columns == _columns);

            var ret = _cuda.Allocate(_data.Size);
            ret.CopyToDevice(_data);
            _cuda.Blas.Axpy(-1.0f, other.Memory.DeviceVariable, 1, ret.DeviceVariable, 1);
            return new CudaMatrix(_cuda, _rows, _columns, ret, true);
        }

        public void SubtractInPlace(IFloatMatrix matrix, float coefficient1 = 1, float coefficient2 = 1)
        {
            Debug.Assert(IsValid && matrix.IsValid);
            var other = (CudaMatrix)matrix;
            Debug.Assert(other._rows == _rows && other._columns == _columns);

            _cuda.SubtractInPlace(_data, other._data, _rows * _columns, coefficient1, coefficient2);
        }

        public IFloatMatrix TanhActivation()
        {
            Debug.Assert(IsValid);
            return new CudaMatrix(_cuda, _rows, _columns, _cuda.TanH(_data, _rows * _columns), true);
        }

        public IFloatMatrix TanhDerivative()
        {
            Debug.Assert(IsValid);
            return new CudaMatrix(_cuda, _rows, _columns, _cuda.TanHDerivative(_data, _rows * _columns), true);
        }

        public IFloatMatrix Transpose()
        {
            Debug.Assert(IsValid);
            var ret = _cuda.Allocate(_rows * _columns);
            float alpha = 1.0f, beta = 0.0f;
            CudaBlasNativeMethods.cublasSgeam(_cuda.Blas.CublasHandle,
                Operation.Transpose,
                Operation.NonTranspose,
                (int)_columns,
                (int)_rows,
                ref alpha,
                _data.DevicePointer,
                (int)_rows,
                ref beta,
                new CUdeviceptr(0),
                (int)_columns,
                ret.DevicePointer,
                (int)_columns
            );
            return new CudaMatrix(_cuda, _columns, _rows, ret, true);
        }

        public IFloatMatrix TransposeAndMultiply(IFloatMatrix matrix)
        {
            Debug.Assert(IsValid && matrix.IsValid);
            var other = (CudaMatrix)matrix;
            Debug.Assert(_columns == other._columns);
            var ret = _cuda.Allocate(_rows * other.RowCount);
            int rowsA = (int)_rows, columnsArowsB = (int)_columns, rowsB = (int)other.RowCount;

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
            return new CudaMatrix(_cuda, _rows, other.RowCount, ret, true);
        }

        public IFloatMatrix TransposeThisAndMultiply(IFloatMatrix matrix)
        {
            Debug.Assert(IsValid && matrix.IsValid);
            var other = (CudaMatrix)matrix;
            Debug.Assert(_rows == other._rows);

            var ret = _cuda.Allocate(_columns * other.ColumnCount);
            int rowsA = (int)_rows, columnsA = (int)_columns, columnsB = (int)other.ColumnCount, rowsB = (int)other.RowCount;

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
            return new CudaMatrix(_cuda, _columns, other.ColumnCount, ret, true);
        }

		public Matrix<float> Data
        {
            get => AsIndexable().Data;

	        set
            {
                Debug.Assert(IsValid);
                var buffer = new float[_rows * _columns];
                _data.CopyToHost(buffer);

                var rowCount = value.RowCount;
                for (uint i = 0; i < rowCount && i < _rows; i++) {
                    var row = value.Row(i);
                    var data2 = row.Segment;
                    var columnCount = data2.Size;
                    for (uint j = 0; j < columnCount && j < _columns; j++) {
                        buffer[j * _rows + i] = data2[j];
                    }
                }

                _data.CopyToDevice(buffer);
            }
        }

        public IFloatMatrix Multiply(IFloatVector vector)
        {
            Debug.Assert(IsValid && vector.IsValid);
            using var column = vector.ReshapeAsColumnMatrix();
            return Multiply(column);
        }

        public (IFloatMatrix U, IFloatVector S, IFloatMatrix VT) Svd()
        {
            Debug.Assert(IsValid);
            var solver = _cuda.Solver;

            // find the size of the required buffer
            var bufferSize = solver.GesvdBufferSizeFloat((int)_rows, (int)_columns);
            var mn = System.Math.Min(_rows, _columns);

            // allocate output buffers
            var s = _cuda.Allocate(mn);
            var u = _cuda.Allocate(_rows * _rows);
            var vt = _cuda.Allocate(_columns * _columns);

            // call cusolver to find the SVD
            try {
                var buffer = _cuda.Allocate((uint)bufferSize);
                var rwork = _cuda.Allocate(mn);
                var a = _cuda.Allocate(_rows * _columns);
                try {
                    using var devInfo = new CudaDeviceVariable<int>(1);
                    a.CopyToDevice(_data);
                    solver.Gesvd('A', 'A', (int)_rows, (int)_columns, a.DeviceVariable, (int)_rows, s.DeviceVariable, u.DeviceVariable, (int)_rows, vt.DeviceVariable, (int)_columns, buffer.DeviceVariable, bufferSize, rwork.DeviceVariable, devInfo);
                    return (
                        new CudaMatrix(_cuda, _rows, _rows, u, true),
                        new CudaVector(_cuda, s, true),
                        new CudaMatrix(_cuda, _columns, _columns, vt, true)
                    );
                }finally {
                    buffer.Free();
                    rwork.Free();
                    a.Free();
                }
            }catch {
                s.Free();
                u.Free();
                vt.Free();
                throw;
            }
        }

        public IFloatVector ReshapeAsVector()
        {
            Debug.Assert(IsValid);
            return new CudaVector(_cuda, _data, false);
        }

		public I3DFloatTensor ReshapeAs3DTensor(uint rows, uint columns)
		{
			Debug.Assert(IsValid && rows * columns == _rows);
			return new Cuda3DTensor(_cuda, rows, columns, _columns, _data, false);
		}

		public I4DFloatTensor ReshapeAs4DTensor(uint rows, uint columns, uint depth)
        {
	        Debug.Assert(IsValid && rows * columns * depth == _rows);
            return new Cuda4DTensor(_cuda, rows, columns, depth, _columns, _data, false);
        }

	    public float GetAt(uint row, uint column)
	    {
		    return _data.DeviceVariable[column * _rows + row];
	    }

	    public void SetAt(uint row, uint column, float value)
	    {
		    _data.DeviceVariable[column * _rows + row] = value;
	    }

	    public IFloatVector[] ColumnVectors()
	    {
		    var ret = new IFloatVector[ColumnCount];
		    for (uint i = 0; i < ColumnCount; i++)
			    ret[i] = Column(i);
		    return ret;
	    }

	    public IFloatVector[] RowVectors()
	    {
		    var ret = new IFloatVector[RowCount];
		    for (uint i = 0; i < RowCount; i++)
			    ret[i] = Row(i);
		    return ret;
	    }
    }
}
