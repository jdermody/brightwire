using System.Collections.Generic;
using System.Diagnostics;
using BrightData.Cuda.CudaToolkit.Types;
using BrightData.Cuda.CudaToolkit;
using BrightData.LinearAlgebra;
using System.Linq;

namespace BrightData.Cuda
{
    /// <inheritdoc />
    /// <inheritdoc />
    public unsafe class CudaMatrix(INumericSegment<float> data, uint rows, uint columns, CudaLinearAlgebraProvider lap) 
        : MutableMatrix<CudaLinearAlgebraProvider>(data, rows, columns, lap)
    {
        /// <summary>
        /// Associated CUDA provider
        /// </summary>
        public CudaProvider Provider = lap.Provider;

        /// <inheritdoc />
        public override IVector GetColumnVector(uint index)
        {
            var segment = (CudaTensorSegment)Segment;
            var ptr = segment.DeviceMemory.Offset(index * RowCount, RowCount);
            return Lap.CreateVector(new CudaTensorSegment(ptr, Provider));
        }

        /// <inheritdoc />
        public override IVector GetRowVector(uint index)
        {
            //return _lap.CreateVector(Row(index));
            var segment = Lap.GetNonContinuousSegment(Segment, index, RowCount, ColumnCount);
            return Lap.CreateVector(segment);
        }

        /// <inheritdoc />
        public override IMatrix Transpose()
        {
            var ret = Provider.Allocate(RowCount * ColumnCount);
            float alpha = 1.0f, beta = 0.0f;
            CudaBlasNativeMethods.cublasSgeam(Provider.Blas,
                Operation.Transpose,
                Operation.NonTranspose,
                (int)ColumnCount,
                (int)RowCount,
                ref alpha,
                Segment.GetDevicePointer(),
                (int)RowCount,
                ref beta,
                new CuDevicePtr(0),
                (int)ColumnCount,
                ret.DevicePointer,
                (int)ColumnCount
            );
            return Lap.CreateMatrix(ColumnCount, RowCount, new CudaTensorSegment(ret, Provider));
        }

        /// <inheritdoc />
        public override IVector Multiply(IVector vector)
        {
            var (ptr, stride) = vector.Segment.GetDeviceMemory();
            var matrixPtr = Segment.GetDeviceMemoryPtr();
            var size = (int)ptr.Size;
            float alpha = 1.0f, beta = 0f;

            var result = Provider.Allocate((uint)size);
            CudaBlasNativeMethods.cublasSgemv_v2(Provider.Blas,
                Operation.NonTranspose,
                size,
                size,
                ref alpha,
                matrixPtr.DevicePointer,
                size,
                ptr.DevicePointer,
                (int)stride,
                ref beta,
                result.DevicePointer,
                1
            ).CheckResult();
            return new CudaVector(Lap.CreateCudaTensorSegment(result), Lap);
        }

        /// <inheritdoc />
        public override INumericSegment<float>[] SoftmaxDerivativePerRow(IReadOnlyNumericSegment<float>[] rows)
        {
            uint size = rows[0].Size, rowCount = RowCount;
            var derivatives = Lap.MultiSoftmaxDerivative(rows);
            using var singleBlock = Provider.Allocate(size * rowCount);
            var ret = new INumericSegment<float>[rowCount];

            for (uint i = 0; i < rowCount; i++) {
                using var derivative = derivatives[i];
                var derivativePtr = derivative.Segment.GetDeviceMemoryPtr();
                var (ptr, stride) = GetRow(i).GetDeviceMemory();

                float alpha = 1.0f, beta = 0f;
                var result = singleBlock.Offset(i * size, size);
                CudaBlasNativeMethods.cublasSgemv_v2(Provider.Blas,
                    Operation.NonTranspose,
                    (int)size,
                    (int)size,
                    ref alpha,
                    derivativePtr.DevicePointer,
                    (int)size,
                    ptr.DevicePointer,
                    (int)stride,
                    ref beta,
                    result.DevicePointer,
                    1
                ).CheckResult();
                ret[i] = Lap.CreateCudaTensorSegment(result);
                result.AddRef();
            }

            return ret;
        }

        /// <inheritdoc />
        public override void AddToEachColumn(IReadOnlyNumericSegment<float> segment)
        {
            Provider.AddToEachColumn(Segment.GetDeviceMemoryPtr(), segment.GetDeviceMemoryPtr(), RowCount, ColumnCount);
        }

        /// <inheritdoc />
        public override void AddToEachRow(IReadOnlyNumericSegment<float> segment)
        {
            Provider.AddToEachRow(Segment.GetDeviceMemoryPtr(), segment.GetDeviceMemoryPtr(), RowCount, ColumnCount);
        }

        /// <inheritdoc />
        public override void MultiplyEachColumnWith(IReadOnlyNumericSegment<float> segment)
        {
            Provider.MultiplyByEachColumn(Segment.GetDeviceMemoryPtr(), segment.GetDeviceMemoryPtr(), RowCount, ColumnCount);
        }

        /// <inheritdoc />
        public override void MultiplyEachRowWith(IReadOnlyNumericSegment<float> segment)
        {
            Provider.MultiplyByEachRow(Segment.GetDeviceMemoryPtr(), segment.GetDeviceMemoryPtr(), RowCount, ColumnCount);
        }

        /// <inheritdoc />
        public override IMatrix GetNewMatrixFromColumns(IEnumerable<uint> columnIndices)
        {
            uint offset = 0;
            var indices = columnIndices.ToList();
            var ret = Provider.Allocate(RowCount * (uint)indices.Count);
            foreach (var item in indices) {
                ret.DeviceVariable.CopyToDevice(Segment.GetDeviceVariable(), item * RowCount * CudaProvider.FloatSize, offset * CudaProvider.FloatSize, RowCount * CudaProvider.FloatSize);
                offset += RowCount;
            }
            return Lap.CreateMatrix(RowCount, (uint)indices.Count, Lap.CreateCudaTensorSegment(ret));
        }

        /// <inheritdoc />
        public override IMatrix GetNewMatrixFromRows(IEnumerable<uint> rowIndices)
        {
            int offset = 0;
            var indices = rowIndices.ToList();
            var ret = Provider.Allocate(ColumnCount * (uint)indices.Count);
            foreach (var item in indices) {
                CudaBlasNativeMethods.cublasScopy_v2(Provider.Blas,
                    n: (int)ColumnCount,
                    x: Segment.GetDevicePointer() + (item * CudaProvider.FloatSize),
                    incx: (int)RowCount,
                    y: ret.DevicePointer + (offset * CudaProvider.FloatSize),
                    incy: indices.Count
                );
                offset += 1;
            }
            return Lap.CreateMatrix((uint)indices.Count, ColumnCount, Lap.CreateCudaTensorSegment(ret));
        }

        /// <inheritdoc />
        public override IMatrix Multiply(IMatrix other)
        {
            var ret = Provider.Allocate(RowCount * other.ColumnCount);
            int rowsA = (int)RowCount, columnsARowsB = (int)ColumnCount, columnsB = (int)other.ColumnCount;

            float alpha = 1.0f, beta = 0.0f;
            CudaBlasNativeMethods.cublasSgemm_v2(Provider.Blas,
                Operation.NonTranspose,
                Operation.NonTranspose,
                rowsA,
                columnsB,
                columnsARowsB,
                ref alpha,
                Segment.GetDevicePointer(),
                rowsA,
                other.Segment.GetDevicePointer(),
                columnsARowsB,
                ref beta,
                ret.DevicePointer,
                rowsA
            );
            return Lap.CreateMatrix(RowCount, other.ColumnCount, Lap.CreateCudaTensorSegment(ret));
        }

        /// <inheritdoc />
        public override IMatrix TransposeAndMultiply(IMatrix other)
        {
            var ret = Provider.Allocate(RowCount * other.RowCount);
            int rowsA = (int)RowCount, columnsARowsB = (int)ColumnCount, rowsB = (int)other.RowCount;

            float alpha = 1.0f, beta = 0.0f;
            CudaBlasNativeMethods.cublasSgemm_v2(Provider.Blas,
                Operation.NonTranspose,
                Operation.Transpose,
                rowsA,
                rowsB,
                columnsARowsB,
                ref alpha,
                Segment.GetDevicePointer(),
                rowsA,
                other.Segment.GetDevicePointer(),
                rowsB,
                ref beta,
                ret.DevicePointer,
                rowsA
            );
            return Lap.CreateMatrix(RowCount, other.RowCount, Lap.CreateCudaTensorSegment(ret));
        }

        /// <inheritdoc />
        public override IMatrix TransposeThisAndMultiply(IMatrix other)
        {
            var ret = Provider.Allocate(ColumnCount * other.ColumnCount);
            int rowsA = (int)RowCount, columnsA = (int)ColumnCount, columnsB = (int)other.ColumnCount, rowsB = (int)other.RowCount;

            float alpha = 1.0f, beta = 0.0f;
            CudaBlasNativeMethods.cublasSgemm_v2(Provider.Blas,
                Operation.Transpose,
                Operation.NonTranspose,
                columnsA,
                columnsB,
                rowsB,
                ref alpha,
                Segment.GetDevicePointer(),
                rowsA,
                other.Segment.GetDevicePointer(),
                rowsB,
                ref beta,
                ret.DevicePointer,
                columnsA
            );
            return Lap.CreateMatrix(ColumnCount, other.ColumnCount, Lap.CreateCudaTensorSegment(ret));
        }

        /// <inheritdoc />
        public override IVector GetDiagonal()
        {
            var ret = Provider.Diagonal(Segment.GetDeviceMemoryPtr(), RowCount, ColumnCount);
            return Lap.CreateVector(Lap.CreateCudaTensorSegment(ret));
        }

        /// <inheritdoc />
        public override IVector RowSums()
        {
            var ret = Provider.SumRows(Segment.GetDeviceMemoryPtr(), RowCount, ColumnCount, null);
            return Lap.CreateVector(Lap.CreateCudaTensorSegment(ret));
        }

        /// <inheritdoc />
        public override IVector ColumnSums()
        {
            var ret = Provider.SumColumns(Segment.GetDeviceMemoryPtr(), RowCount, ColumnCount, null);
            return Lap.CreateVector(Lap.CreateCudaTensorSegment(ret));
        }

        /// <inheritdoc />
        public override (IMatrix U, IVector S, IMatrix VT) Svd()
        {
            var solver = Provider.Solver;
            var rows = RowCount;
            var columns = ColumnCount;

            // find the size of the required buffer
            var bufferSize = 0;
            CudaSolveNativeMethods.Dense.cusolverDnSgesvd_bufferSize(solver, (int)rows, (int)columns, ref bufferSize);
            var mn = System.Math.Min(rows, columns);

            // allocate output buffers
            var s = Provider.Allocate(mn);
            var u = Provider.Allocate(rows * rows);
            var vt = Provider.Allocate(columns * columns);

            // call cusolver to find the SVD
            try {
                var buffer = Provider.Allocate((uint)bufferSize);
                var rwork = Provider.Allocate(mn);
                var a = Provider.Allocate(rows * columns);
                try {
                    using var devInfo = new CudaDeviceVariable<int>(1);
                    a.CopyToDevice(Segment.GetDeviceMemoryPtr());
                    CudaSolveNativeMethods.Dense.cusolverDnSgesvd(solver,
                        'A',
                        'A',
                        (int)rows,
                        (int)columns,
                        a.DevicePointer,
                        (int)rows,
                        s.DevicePointer,
                        u.DevicePointer,
                        (int)rows,
                        vt.DevicePointer,
                        (int)columns,
                        buffer.DevicePointer,
                        bufferSize,
                        rwork.DevicePointer,
                        devInfo.DevicePointer
                    );
                    return (
                        Lap.CreateMatrix(rows, rows, Lap.CreateCudaTensorSegment(u)),
                        Lap.CreateVector(Lap.CreateCudaTensorSegment(s)),
                        Lap.CreateMatrix(columns, columns, Lap.CreateCudaTensorSegment(vt))
                    );
                }
                finally {
                    buffer.Release();
                    rwork.Release();
                    a.Release();
                }
            }
            catch {
                s.Release();
                u.Release();
                vt.Release();
                throw;
            }
        }

        /// <inheritdoc />
        public override (IMatrix Left, IMatrix Right) SplitAtColumn(uint columnIndex)
        {
            var size = ColumnCount - columnIndex;
            var ret1 = Provider.Allocate(RowCount * columnIndex);
            var ret2 = Provider.Allocate(RowCount * size);
            Provider.SplitRows(Segment.GetDeviceMemoryPtr(), ret1, ret2, RowCount, ColumnCount, columnIndex);
            return (Lap.CreateMatrix(RowCount, columnIndex, Lap.CreateCudaTensorSegment(ret1)), Lap.CreateMatrix(RowCount, size, Lap.CreateCudaTensorSegment(ret2)));
        }

        /// <inheritdoc />
        public override (IMatrix Top, IMatrix Bottom) SplitAtRow(uint rowIndex)
        {
            var size = RowCount - rowIndex;
            var ret1 = Provider.Allocate(rowIndex * ColumnCount);
            var ret2 = Provider.Allocate(size * ColumnCount);
            Provider.SplitColumns(Segment.GetDeviceMemoryPtr(), ret1, ret2, RowCount, ColumnCount, rowIndex);
            return (Lap.CreateMatrix(rowIndex, ColumnCount, Lap.CreateCudaTensorSegment(ret1)), Lap.CreateMatrix(size, ColumnCount, Lap.CreateCudaTensorSegment(ret2)));
        }

        /// <inheritdoc />
        public override IMatrix ConcatBelow(IMatrix bottom)
        {
            Debug.Assert(ColumnCount == bottom.ColumnCount);
            var size = RowCount + bottom.RowCount;
            var ret = Provider.Allocate(size * ColumnCount);
            Provider.ConcatColumns(Segment.GetDeviceMemoryPtr(), bottom.Segment.GetDeviceMemoryPtr(), ret, size, ColumnCount, RowCount, bottom.RowCount);
            return Lap.CreateMatrix(size, ColumnCount, Lap.CreateCudaTensorSegment(ret));
        }

        /// <inheritdoc />
        public override IMatrix ConcatRight(IMatrix right)
        {
            Debug.Assert(RowCount == right.RowCount);
            var size = ColumnCount + right.ColumnCount;
            var ret = Provider.Allocate(RowCount * size);
            Provider.ConcatRows(Segment.GetDeviceMemoryPtr(), right.Segment.GetDeviceMemoryPtr(), ret, RowCount, size, ColumnCount);
            return Lap.CreateMatrix(RowCount, size, Lap.CreateCudaTensorSegment(ret));
        }
    }
}
