using ManagedCuda;
using ManagedCuda.CudaBlas;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using BrightWire.Models;
using ManagedCuda.BasicTypes;
using BrightWire.Cuda.Helper;
using System.Threading;

namespace BrightWire.LinearAlgebra
{
    /// <summary>
    /// GPU backed matrix
    /// </summary>
    class GpuMatrix : IMatrix, IHaveDeviceMemory
    {
        readonly CudaProvider _cuda;
        readonly IDeviceMemoryPtr _data;
        readonly uint _rows, _columns;
        bool _disposed = false;
#if DEBUG
        static int _gid = 0;
        static int _GetNextIndex() => Interlocked.Increment(ref _gid);
        readonly int _id = _GetNextIndex();
        public static int _badAlloc = -1;
        public static int _badDispose = -1;

        public bool IsValid => !_disposed;
#else
        public bool IsValid => true;
#endif

        public GpuMatrix(CudaProvider cuda, uint rows, uint columns, IDeviceMemoryPtr data, bool isOwner)
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
        ~GpuMatrix()
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

        public uint ColumnCount => _columns;
	    public uint RowCount => _rows;
	    public IDeviceMemoryPtr Memory => _data;

        public IMatrix Add(IMatrix matrix)
        {
            Debug.Assert(IsValid && matrix.IsValid);
            var other = (GpuMatrix)matrix;
            Debug.Assert(other._rows == _rows && other._columns == _columns);

            var ret = _cuda.Allocate(other._data.Size);
            ret.CopyToDevice(other._data);
            _cuda.Blas.Axpy(1.0f, _data.DeviceVariable, 1, ret.DeviceVariable, 1);
            return new GpuMatrix(_cuda, _rows, _columns, ret, true);
        }

        public void AddInPlace(IMatrix matrix, float coefficient1 = 1, float coefficient2 = 1)
        {
            Debug.Assert(IsValid && matrix.IsValid);
            var other = (GpuMatrix)matrix;
            Debug.Assert(other._rows == _rows && other._columns == _columns);

            _cuda.AddInPlace(_data, other._data, _rows * _columns, coefficient1, coefficient2);
        }

        public void AddToEachColumn(IVector vector)
        {
            Debug.Assert(IsValid && vector.IsValid);
            var other = (GpuVector)vector;
            _cuda.AddToEachColumn(_data, other.Memory, _rows, _columns);
        }

        public void AddToEachRow(IVector vector)
        {
            Debug.Assert(IsValid && vector.IsValid);
            var other = (GpuVector)vector;
            _cuda.AddToEachRow(_data, other.Memory, _rows, _columns);
        }

        public IIndexableMatrix AsIndexable()
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

        public void ClearColumns(IReadOnlyList<uint> indices)
        {
            Debug.Assert(IsValid);
            foreach (var item in indices)
                _cuda.MemClear(_data, _rows, item * _rows);
        }

        public void ClearRows(IReadOnlyList<uint> indices)
        {
            Debug.Assert(IsValid);
            foreach (var item in indices)
                _cuda.MemClear(_data, _columns, item, _rows);
        }

        public IMatrix Clone()
        {
            Debug.Assert(IsValid);
            var ret = _cuda.Allocate(_rows * _columns);
            ret.CopyToDevice(_data);
            return new GpuMatrix(_cuda, _rows, _columns, ret, true);
        }

        public IVector Column(uint index)
        {
            Debug.Assert(IsValid);
	        var ptr = _cuda.OffsetByBlock(_data, index, _rows);
            return new GpuVector(_cuda, ptr, false);
        }

        public IVector ColumnL2Norm()
        {
            Debug.Assert(IsValid);
            var norm = new List<float>();
            for(var i = 0; i < _columns; i++) {
                using (var col = Column(i))
                    norm.Add(col.L2Norm());
            }
            return _cuda.CreateVector(norm.Count, x => norm[x]);
        }

        public IVector ColumnSums()
        {
            Debug.Assert(IsValid);
            return new GpuVector(_cuda, _cuda.SumColumns(_data, _rows, _columns), true);
        }

        public IMatrix ConcatColumns(IMatrix bottom)
        {
            Debug.Assert(IsValid && bottom.IsValid);
            var t = this;
            var b = (GpuMatrix)bottom;
            Debug.Assert(ColumnCount == bottom.ColumnCount);
            var size = t.RowCount + b.RowCount;
            var ret = _cuda.Allocate(size * t.ColumnCount);
            _cuda.ConcatColumns(t._data, b._data, ret, size, t.ColumnCount, t.RowCount, b.RowCount);
            return new GpuMatrix(_cuda, size, t.ColumnCount, ret, true);
        }

        public IMatrix ConcatRows(IMatrix right)
        {
            Debug.Assert(IsValid && right.IsValid);
            var t = this;
            var b = (GpuMatrix)right;
            Debug.Assert(RowCount == right.RowCount);
            var size = t.ColumnCount + b.ColumnCount;
            var ret = _cuda.Allocate(t.RowCount * size);
            _cuda.ConcatRows(t._data, b._data, ret, t.RowCount, size, t.ColumnCount);
            return new GpuMatrix(_cuda, t.RowCount, size, ret, true);
        } 

        public void Constrain(float min, float max)
        {
            Debug.Assert(IsValid);
            _cuda.Constrain(_data, _rows * _columns, min, max);
        }

        public IVector Diagonal()
        {
            Debug.Assert(IsValid);
            var ret = _cuda.Diagonal(_data, _rows, _columns);
            return new GpuVector(_cuda, ret, true);
        }

        public IVector GetColumnSegment(uint columnIndex, uint rowIndex, uint length)
        {
            Debug.Assert(IsValid);

            var ret = _cuda.Allocate(length);
            ret.DeviceVariable.CopyToDevice(_data.DeviceVariable, ((columnIndex * _rows) + rowIndex) * CudaProvider.FLOAT_SIZE, 0, length * CudaProvider.FLOAT_SIZE);

            return new GpuVector(_cuda, ret, true);
        }

        public IMatrix GetNewMatrixFromColumns(IReadOnlyList<uint> columnIndices)
        {
            Debug.Assert(IsValid);
            int offset = 0;
            var ret = _cuda.Allocate(_rows * columnIndices.Count);
            foreach (var item in columnIndices) {
                ret.DeviceVariable.CopyToDevice(_data.DeviceVariable, item * _rows * CudaProvider.FLOAT_SIZE, offset * CudaProvider.FLOAT_SIZE, _rows * CudaProvider.FLOAT_SIZE);
                offset += _rows;
            }
            return new GpuMatrix(_cuda, _rows, columnIndices.Count, ret, true);
        }

        public IMatrix GetNewMatrixFromRows(IReadOnlyList<uint> rowIndices)
        {
            Debug.Assert(IsValid);
            int offset = 0;
            var ret = _cuda.Allocate(_columns * rowIndices.Count);
            foreach (var item in rowIndices) {
                CudaBlasNativeMethods.cublasScopy_v2(_cuda.Blas.CublasHandle,
                    n: _columns,
                    x: _data.DevicePointer + (item * CudaProvider.FLOAT_SIZE),
                    incx: _rows,
                    y: ret.DevicePointer + (offset * CudaProvider.FLOAT_SIZE),
                    incy: rowIndices.Count
                );
                offset += 1;
            }
            return new GpuMatrix(_cuda, rowIndices.Count, _columns, ret, true);
        }

        public IVector GetRowSegment(uint rowIndex, uint columnIndex, uint length)
        {
            Debug.Assert(IsValid);
            int offset = (rowIndex + (columnIndex * _rows)) * CudaProvider.FLOAT_SIZE;
            var ret = _cuda.Allocate(length);
            CudaBlasNativeMethods.cublasScopy_v2(_cuda.Blas.CublasHandle, length, _data.DevicePointer + offset, _rows, ret.DevicePointer, 1);
            return new GpuVector(_cuda, ret, true);
        }

        public void L1Regularisation(float coefficient)
        {
            Debug.Assert(IsValid);
            _cuda.L1Regularisation(_data, _rows * _columns, coefficient);
        }

        public IMatrix LeakyReluActivation()
        {
            Debug.Assert(IsValid);
            var ret = _cuda.LeakyRELU(_data, _rows * _columns);
            return new GpuMatrix(_cuda, _rows, _columns, ret, true);
        }

        public IMatrix LeakyReluDerivative()
        {
            Debug.Assert(IsValid);
            var ret = _cuda.LeakyRELUDerivative(_data, _rows * _columns);
            return new GpuMatrix(_cuda, _rows, _columns, ret, true);
        }

        public void Multiply(float scalar)
        {
            Debug.Assert(IsValid);
            _cuda.Blas.Scale(scalar, _data.DeviceVariable, 1);
        }

        public IMatrix Multiply(IMatrix matrix)
        {
            Debug.Assert(IsValid && matrix.IsValid);
            var other = (GpuMatrix)matrix;
            Debug.Assert(_columns == other._rows);
            var ret = _cuda.Allocate(_rows * other.ColumnCount);
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
            return new GpuMatrix(_cuda, _rows, other.ColumnCount, ret, true);
        }

        public IMatrix PointwiseDivide(IMatrix matrix)
        {
            Debug.Assert(IsValid && matrix.IsValid);
            var other = (GpuMatrix)matrix;
            Debug.Assert(other._rows == _rows && other._columns == _columns);

            var size = _rows * _columns;
            var ret = _cuda.PointwiseDivide(_data, other._data, size);
            return new GpuMatrix(_cuda, _rows, _columns, ret, true);
        }

        public void PointwiseDivideColumns(IVector vector)
        {
            Debug.Assert(IsValid && vector.IsValid);
            var other = (GpuVector)vector;
            _cuda.PointwiseDivideColumns(_data, other.Memory, _rows, _columns);
        }

        public void PointwiseDivideRows(IVector vector)
        {
            Debug.Assert(IsValid && vector.IsValid);
            var other = (GpuVector)vector;
            _cuda.PointwiseDivideRows(_data, other.Memory, _rows, _columns);
        }

        public IMatrix PointwiseMultiply(IMatrix matrix)
        {
            Debug.Assert(IsValid && matrix.IsValid);
            var other = (GpuMatrix)matrix;
            Debug.Assert(other._rows == _rows && other._columns == _columns);

            var size = _rows * _columns;
            var ret = _cuda.PointwiseMultiply(_data, other._data, size);
            return new GpuMatrix(_cuda, _rows, _columns, ret, true);
        }

        public IMatrix Pow(float power)
        {
            Debug.Assert(IsValid);
            var ret = _cuda.Pow(_data, _rows * _columns, power);
            return new GpuMatrix(_cuda, _rows, _columns, ret, true);
        }

        public IMatrix ReluActivation()
        {
            Debug.Assert(IsValid);
            return new GpuMatrix(_cuda, _rows, _columns, _cuda.RELU(_data, _rows * _columns), true);
        }

        public IMatrix ReluDerivative()
        {
            Debug.Assert(IsValid);
            return new GpuMatrix(_cuda, _rows, _columns, _cuda.RELUDerivative(_data, _rows * _columns), true);
        }

        public IVector Row(uint index)
        {
            Debug.Assert(IsValid);
            var ret = _cuda.Allocate(_columns);
            int offset = index * CudaProvider.FLOAT_SIZE;
            CudaBlasNativeMethods.cublasScopy_v2(_cuda.Blas.CublasHandle, _columns, _data.DevicePointer + offset, _rows, ret.DevicePointer, 1);
            return new GpuVector(_cuda, ret, true);
        }

        public IVector RowL2Norm()
        {
            Debug.Assert(IsValid);
            var norm = new List<float>();
            for (var i = 0; i < _rows; i++) {
                using (var row = Row(i))
                    norm.Add(row.L2Norm());
            }
            return _cuda.CreateVector(norm.Count, x => norm[x]);
        }

        public IVector RowSums()
        {
            Debug.Assert(IsValid);
            return new GpuVector(_cuda, _cuda.SumRows(_data, _rows, _columns), true);
        }

        public IMatrix SigmoidActivation()
        {
            Debug.Assert(IsValid);
            return new GpuMatrix(_cuda, _rows, _columns, _cuda.Sigmoid(_data, _rows * _columns), true);
        }

        public IMatrix SigmoidDerivative()
        {
            Debug.Assert(IsValid);
            return new GpuMatrix(_cuda, _rows, _columns, _cuda.SigmoidDerivative(_data, _rows * _columns), true);
        }

        public IMatrix SoftmaxActivation()
        {
            Debug.Assert(IsValid);
            var rowOutput = new List<GpuVector>();
            for (var i = 0; i < _rows; i++) {
                using (var row = Row(i))
                    rowOutput.Add(row.Softmax() as GpuVector);
            }
            var ret = _cuda.Allocate(_rows * _columns);
            for (var i = 0; i < _rows; i++) {
                using (var row = rowOutput[i])
                    ret.DeviceVariable.CopyToDevice(row.Memory.DeviceVariable, 0, _columns * i * CudaProvider.FLOAT_SIZE, _columns * CudaProvider.FLOAT_SIZE);
            }
            using(var temp = new GpuMatrix(_cuda, _columns, _rows, ret, true))
                return temp.Transpose();
        }

        public (IMatrix Top, IMatrix Bottom) SplitAtRow(uint rowIndex)
        {
            Debug.Assert(IsValid);
            int size = _rows - rowIndex;
            var ret1 = _cuda.Allocate(rowIndex * _columns);
            var ret2 = _cuda.Allocate(size * _columns);
            _cuda.SplitColumns(_data, ret1, ret2, _rows, _columns, rowIndex);
            return (new GpuMatrix(_cuda, rowIndex, _columns, ret1, true), new GpuMatrix(_cuda, size, _columns, ret2, true));
        }

        public (IMatrix Left, IMatrix Right) SplitAtColumn(uint columnIndex)
        {
            Debug.Assert(IsValid);
            int size = _columns - columnIndex;
            var ret1 = _cuda.Allocate(_rows * columnIndex);
            var ret2 = _cuda.Allocate(_rows * size);
            _cuda.SplitRows(_data, ret1, ret2, _rows, _columns, columnIndex);
            return (new GpuMatrix(_cuda, _rows, columnIndex, ret1, true), new GpuMatrix(_cuda, _rows, size, ret2, true));
        }

        public IMatrix Sqrt(float valueAdjustment = 1e-8f)
        {
            Debug.Assert(IsValid);
            var size = _rows * _columns;
            var ret = _cuda.Sqrt(_data, size, valueAdjustment);
            return new GpuMatrix(_cuda, _rows, _columns, ret, true);
        }

        public IMatrix Subtract(IMatrix matrix)
        {
            Debug.Assert(IsValid && matrix.IsValid);
            var other = (GpuMatrix)matrix;
            Debug.Assert(other._rows == _rows && other._columns == _columns);

            var ret = _cuda.Allocate(_data.Size);
            ret.CopyToDevice(_data);
            _cuda.Blas.Axpy(-1.0f, other.Memory.DeviceVariable, 1, ret.DeviceVariable, 1);
            return new GpuMatrix(_cuda, _rows, _columns, ret, true);
        }

        public void SubtractInPlace(IMatrix matrix, float coefficient1 = 1, float coefficient2 = 1)
        {
            Debug.Assert(IsValid && matrix.IsValid);
            var other = (GpuMatrix)matrix;
            Debug.Assert(other._rows == _rows && other._columns == _columns);

            _cuda.SubtractInPlace(_data, other._data, _rows * _columns, coefficient1, coefficient2);
        }

        public IMatrix TanhActivation()
        {
            Debug.Assert(IsValid);
            return new GpuMatrix(_cuda, _rows, _columns, _cuda.TanH(_data, _rows * _columns), true);
        }

        public IMatrix TanhDerivative()
        {
            Debug.Assert(IsValid);
            return new GpuMatrix(_cuda, _rows, _columns, _cuda.TanHDerivative(_data, _rows * _columns), true);
        }

        public IMatrix Transpose()
        {
            Debug.Assert(IsValid);
            var ret = _cuda.Allocate(_rows * _columns);
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
                new CUdeviceptr(0),
                _columns,
                ret.DevicePointer,
                _columns
            );
            return new GpuMatrix(_cuda, _columns, _rows, ret, true);
        }

        public IMatrix TransposeAndMultiply(IMatrix matrix)
        {
            Debug.Assert(IsValid && matrix.IsValid);
            var other = (GpuMatrix)matrix;
            Debug.Assert(_columns == other._columns);
            var ret = _cuda.Allocate(_rows * other.RowCount);
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
            return new GpuMatrix(_cuda, _rows, other.RowCount, ret, true);
        }

        public IMatrix TransposeThisAndMultiply(IMatrix matrix)
        {
            Debug.Assert(IsValid && matrix.IsValid);
            var other = (GpuMatrix)matrix;
            Debug.Assert(_rows == other._rows);

            var ret = _cuda.Allocate(_columns * other.ColumnCount);
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
            return new GpuMatrix(_cuda, _columns, other.ColumnCount, ret, true);
        }

		public FloatMatrix Data
        {
            get => AsIndexable().Data;

	        set
            {
                Debug.Assert(IsValid);
                var buffer = new float[_rows * _columns];
                _data.CopyToHost(buffer);

                var rowCount = value.Row.Length;
                for (var i = 0; i < rowCount && i < _rows; i++) {
                    var row = value.Row[i];
                    if (row.Data != null) {
                        var data2 = row.Data;
                        var columnCount = data2.Length;
                        for (var j = 0; j < columnCount && j < _columns; j++) {
                            buffer[j * _rows + i] = data2[j];
                        }
                    }
                }

                _data.CopyToDevice(buffer);
            }
        }

        public IMatrix Multiply(IVector vector)
        {
            Debug.Assert(IsValid && vector.IsValid);
            using (var column = vector.ReshapeAsColumnMatrix())
                return Multiply(column);
        }

        public (IMatrix U, IVector S, IMatrix VT) Svd()
        {
            Debug.Assert(IsValid);
            var solver = _cuda.Solver;

            // find the size of the required buffer
            var bufferSize = solver.GesvdBufferSizeFloat(_rows, _columns);
            var mn = System.Math.Min(_rows, _columns);

            // allocate output buffers
            var s = _cuda.Allocate(mn);
            var u = _cuda.Allocate(_rows * _rows);
            var vt = _cuda.Allocate(_columns * _columns);

            // call cusolver to find the SVD
            try {
                var buffer = _cuda.Allocate(bufferSize);
                var rwork = _cuda.Allocate(mn);
                var a = _cuda.Allocate(_rows * _columns);
                try {
                    using (var devInfo = new CudaDeviceVariable<int>(1)) {
                        a.CopyToDevice(_data);
                        solver.Gesvd('A', 'A', _rows, _columns, a.DeviceVariable, _rows, s.DeviceVariable, u.DeviceVariable, _rows, vt.DeviceVariable, _columns, buffer.DeviceVariable, bufferSize, rwork.DeviceVariable, devInfo);
                        return (
                            new GpuMatrix(_cuda, _rows, _rows, u, true),
                            new GpuVector(_cuda, s, true),
                            new GpuMatrix(_cuda, _columns, _columns, vt, true)
                        );
                    }
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

        public IVector ReshapeAsVector()
        {
            Debug.Assert(IsValid);
            return new GpuVector(_cuda, _data, false);
        }

		public I3DTensor ReshapeAs3DTensor(uint rows, uint columns)
		{
			Debug.Assert(IsValid && rows * columns == _rows);
			return new Gpu3DTensor(_cuda, rows, columns, _columns, _data, false);
		}

		public I4DTensor ReshapeAs4DTensor(uint rows, uint columns, uint depth)
        {
	        Debug.Assert(IsValid && rows * columns * depth == _rows);
            return new Gpu4DTensor(_cuda, rows, columns, depth, _columns, _data, false);
        }

	    public float GetAt(uint row, uint column)
	    {
		    return _data.DeviceVariable[column * _rows + row];
	    }

	    public void SetAt(uint row, uint column, float value)
	    {
		    _data.DeviceVariable[column * _rows + row] = value;
	    }

	    public IReadOnlyList<IVector> ColumnVectors()
	    {
		    var ret = new List<IVector>();
		    for (var i = 0; i < ColumnCount; i++)
			    ret.Add(Column(i));
		    return ret;
	    }

	    public IReadOnlyList<IVector> RowVectors()
	    {
		    var ret = new List<IVector>();
		    for (var i = 0; i < RowCount; i++)
			    ret.Add(Row(i));
		    return ret;
	    }
    }
}
