using BrightData.Cuda.CudaToolkit;
using BrightData.LinearAlgebra;

namespace BrightData.Cuda
{
    /// <inheritdoc />
    /// <inheritdoc />
    public unsafe class CudaTensor3D(INumericSegment<float> data, uint depth, uint rowCount, uint columnCount, CudaLinearAlgebraProvider lap)
        : MutableTensor3D<float, CudaLinearAlgebraProvider>(data, depth, rowCount, columnCount, lap)
    {
        /// <summary>
        /// Associated CUDA provider
        /// </summary>
        public CudaProvider Provider = lap.Provider;

        /// <inheritdoc />
        public override IMatrix<float> GetMatrix(uint index)
        {
            var ptr = CudaProvider.OffsetByBlock(Segment.GetDeviceMemoryPtr(), index, MatrixSize);
            return Lap.CreateMatrix(RowCount, ColumnCount, Lap.CreateCudaTensorSegment(ptr));
        }

        /// <inheritdoc />
        public override ITensor3D<float> AddPadding(uint padding)
        {
            var (ret, rows, cols) = Provider.TensorAddPadding(Segment.GetDeviceMemoryPtr(), RowCount, ColumnCount, Depth, 1, padding);
            return Lap.CreateTensor3D(Depth, rows, cols, Lap.CreateCudaTensorSegment(ret));
        }

        /// <inheritdoc />
        public override ITensor3D<float> RemovePadding(uint padding)
        {
            var (ptr, rows, cols) = Provider.TensorRemovePadding(Segment.GetDeviceMemoryPtr(), RowCount, ColumnCount, Depth, 1, padding);
            return Lap.CreateTensor3D(Depth, rows, cols, Lap.CreateCudaTensorSegment(ptr));
        }

        /// <inheritdoc />
        public override IMatrix<float> Im2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            var (ptr, rows, columns, _) = Provider.TensorIm2Col(Segment.GetDeviceMemoryPtr(), RowCount, ColumnCount, Depth, 1, filterWidth, filterHeight, xStride, yStride);
            return Lap.CreateMatrix(rows, columns, Lap.CreateCudaTensorSegment(ptr));
        }

        /// <inheritdoc />
        public override ITensor3D<float> ReverseIm2Col(IMatrix<float> filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            var (ptr, rows, cols, depth, _) = Provider.TensorReverseIm2Col(Segment.GetDeviceMemoryPtr(), filter.Segment.GetDeviceMemoryPtr(), RowCount, ColumnCount, Depth, 1, outputRows, outputColumns, outputDepth, filterWidth, filterHeight, xStride, yStride);
            return Lap.CreateTensor3D(depth, rows, cols, Lap.CreateCudaTensorSegment(ptr));
        }

        /// <inheritdoc />
        public override (ITensor3D<float> Result, ITensor3D<float>? Indices) MaxPool(uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices)
        {
            var (ptr, indices, rows, cols) = Provider.TensorMaxPool(Segment.GetDeviceMemoryPtr(), RowCount, ColumnCount, Depth, 1, filterWidth, filterHeight, xStride, yStride, saveIndices);
            var ret = Lap.CreateTensor3D(Depth, rows, cols, Lap.CreateCudaTensorSegment(ptr));
            var indexTensor = indices is null ? null : Lap.CreateTensor3D(Depth, rows, cols, Lap.CreateCudaTensorSegment(indices));
            return (ret, indexTensor);
        }

        /// <inheritdoc />
        public override ITensor3D<float> ReverseMaxPool(ITensor3D<float> indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            var ptr = Provider.TensorReverseMaxPool(Segment.GetDeviceMemoryPtr(), indices.Segment.GetDeviceMemoryPtr(), RowCount, ColumnCount, Depth, 1, outputRows, outputColumns, filterWidth, filterHeight, xStride, yStride);
            return Lap.CreateTensor3D(Depth, outputRows, outputColumns, Lap.CreateCudaTensorSegment(ptr));
        }

        /// <inheritdoc />
        public override IMatrix<float> AddAllMatrices()
        {
            var matrixSize = MatrixSize;
            var tensorMemory = Segment.GetDeviceMemoryPtr();
            var ret = (CudaTensorSegment)Lap.CreateSegment(matrixSize, true);
            var retMemory = ret.DeviceMemory;
            for (uint i = 0; i < Depth; i++) {
                var ptrToMatrix = tensorMemory.Offset(i * matrixSize, matrixSize);
                Provider.AddInPlace(retMemory, ptrToMatrix, matrixSize, 1f, 1f);
            }
            return Lap.CreateMatrix(RowCount, ColumnCount, ret);
        }

        /// <inheritdoc />
        public override ITensor3D<float> MultiplyEachMatrixBy(IMatrix<float> matrix)
        {
            var ptr = Segment.GetDeviceMemoryPtr();
            uint rowsA = RowCount, columnsARowsB = ColumnCount, columnsB = matrix.ColumnCount;
            float alpha = 1.0f, beta = 0.0f;
            var outputPtr = Provider.Allocate(RowCount * columnsB * Depth);
            var output = Lap.CreateTensor3D(Depth, RowCount, columnsB, Lap.CreateCudaTensorSegment(outputPtr));

            var status = CudaBlasNativeMethods.cublasSgemmStridedBatched(Provider.Blas,
                Operation.NonTranspose,
                Operation.NonTranspose,
                (int)rowsA,
                (int)columnsB,
                (int)columnsARowsB,
                ref alpha,
                ptr.DevicePointer,
                (int)rowsA,
                MatrixSize,
                matrix.Segment.GetDevicePointer(),
                (int)columnsARowsB,
                0,
                ref beta,
                outputPtr.DevicePointer,
                (int)rowsA,
                RowCount * columnsB,
                (int)Depth
            );
            if (status != CuBlasStatus.Success)
                throw new CudaBlasException(status);
            return output;
        }

        /// <inheritdoc />
        public override void AddToEachRow(IVector<float> vector)
        {
            for (uint i = 0, len = Depth; i < len; i++) {
                using var matrix = GetMatrix(i);
                matrix.AddToEachRow(vector.Segment);
            }
        }

        /// <inheritdoc />
        public override void AddToEachColumn(IVector<float> vector)
        {
            for (uint i = 0, len = Depth; i < len; i++) {
                using var matrix = GetMatrix(i);
                matrix.AddToEachColumn(vector.Segment);
            }
        }

        /// <inheritdoc />
        public override ITensor3D<float> TransposeThisAndMultiply(ITensor4D<float> other)
        {
            var ptr = Segment.GetDeviceMemoryPtr();
            var ptr2 = other.Segment.GetDeviceMemoryPtr();
            uint rowsA = RowCount, columnsA = ColumnCount, columnsB = other.Depth, rowsB = other.RowCount * other.ColumnCount, blockSize2 = columnsB * rowsB;
            float alpha = 1.0f, beta = 0.0f;
            var outputPtr = Provider.Allocate(ColumnCount * columnsB * Depth);
            var output = Lap.CreateTensor3D(Depth, columnsB, ColumnCount, Lap.CreateCudaTensorSegment(outputPtr));

            var status = CudaBlasNativeMethods.cublasSgemmStridedBatched(Provider.Blas,
                Operation.Transpose,
                Operation.NonTranspose,
                (int)columnsA,
                (int)columnsB,
                (int)rowsB,
                ref alpha,
                ptr.DevicePointer,
                (int)rowsA,
                MatrixSize,
                ptr2.DevicePointer,
                (int)rowsB,
                blockSize2,
                ref beta,
                outputPtr.DevicePointer,
                (int)columnsA,
                ColumnCount * columnsB,
                (int)Depth
            );
            if (status != CuBlasStatus.Success)
                throw new CudaBlasException(status);

            return output;
        }
    }
}
