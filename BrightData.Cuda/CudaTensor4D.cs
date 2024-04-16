using BrightData.LinearAlgebra;

namespace BrightData.Cuda
{
    /// <inheritdoc />
    /// <inheritdoc />
    public unsafe class CudaTensor4D(INumericSegment<float> data, uint count, uint depth, uint rows, uint columns, CudaLinearAlgebraProvider lap)
        : MutableTensor4D<float, CudaLinearAlgebraProvider>(data, count, depth, rows, columns, lap)
    {
        /// <summary>
        /// Associated CUDA provider
        /// </summary>
        public CudaProvider Provider = lap.Provider;

        /// <inheritdoc />
        public override ITensor3D<float> GetTensor(uint index)
        {
            var ptr = CudaProvider.OffsetByBlock(Segment.GetDeviceMemoryPtr(), index, TensorSize);
            return new CudaTensor3D(Lap.CreateCudaTensorSegment(ptr), Depth, RowCount, ColumnCount, Lap);
        }

        /// <inheritdoc />
        public override ITensor4D<float> ReverseIm2Col(IMatrix<float> filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            var (ptr, rows, cols, depth, count) = Provider.TensorReverseIm2Col(Segment.GetDeviceMemoryPtr(), filter.Segment.GetDeviceMemoryPtr(), RowCount, ColumnCount, Depth, Count, outputRows, outputColumns, outputDepth, filterWidth, filterHeight, xStride, yStride);
            return Lap.CreateTensor4D(count, depth, rows, cols, Lap.CreateCudaTensorSegment(ptr));
        }

        /// <inheritdoc />
        public override ITensor3D<float> Im2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            var (ptr, rows, columns, depth) = Provider.TensorIm2Col(Segment.GetDeviceMemoryPtr(), RowCount, ColumnCount, Depth, Count, filterWidth, filterHeight, xStride, yStride);
            return Lap.CreateTensor3D(depth, rows, columns, Lap.CreateCudaTensorSegment(ptr));
        }

        /// <inheritdoc />
        public override ITensor4D<float> ReverseMaxPool(ITensor4D<float> indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            var ptr = Provider.TensorReverseMaxPool(Segment.GetDeviceMemoryPtr(), indices.Segment.GetDeviceMemoryPtr(), RowCount, ColumnCount, Depth, Count, outputRows, outputColumns, filterWidth, filterHeight, xStride, yStride);
            return Lap.CreateTensor4D(Count, Depth, outputRows, outputColumns, Lap.CreateCudaTensorSegment(ptr));
        }

        /// <inheritdoc />
        public override (ITensor4D<float> Result, ITensor4D<float>? Indices) MaxPool(uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices)
        {
            var (ptr, indices, rows, cols) = Provider.TensorMaxPool(Segment.GetDeviceMemoryPtr(), RowCount, ColumnCount, Depth, Count, filterWidth, filterHeight, xStride, yStride, saveIndices);
            var ret = Lap.CreateTensor4D(Count, Depth, rows, cols, Lap.CreateCudaTensorSegment(ptr));
            var indexTensor = indices is null 
                ? null 
                : Lap.CreateTensor4D(Count, Depth, rows, cols, Lap.CreateCudaTensorSegment(indices));
            return (ret, indexTensor);
        }

        /// <inheritdoc />
        public override ITensor4D<float> RemovePadding(uint padding)
        {
            var (ptr, rows, cols) = Provider.TensorRemovePadding(Segment.GetDeviceMemoryPtr(), RowCount, ColumnCount, Depth, Count, padding);
            return Lap.CreateTensor4D(Count, Depth, rows, cols, Lap.CreateCudaTensorSegment(ptr));
        }

        /// <inheritdoc />
        public override ITensor4D<float> AddPadding(uint padding)
        {
            var (ret, rows, cols) = Provider.TensorAddPadding(Segment.GetDeviceMemoryPtr(), RowCount, ColumnCount, Depth, Count, padding);
            return Lap.CreateTensor4D(Count, Depth, rows, cols, Lap.CreateCudaTensorSegment(ret));
        }

        /// <inheritdoc />
        public override IVector<float> ColumnSums()
        {
            uint matrixSize = MatrixSize, tensorSize = TensorSize, depth = Depth, count = Count;
            var ret = (CudaTensorSegment)Lap.CreateSegment(depth, true);
            var tensorMemory = Segment.GetDeviceMemoryPtr();
            var retMemory = ret.DeviceMemory;
            using var singleBlock = Provider.Allocate(count * matrixSize, null, true);

            for (uint i = 0; i < count; i++) {
                var tensorPtr = tensorMemory.Offset(i * tensorSize, tensorSize);
                var retPtr = singleBlock.Offset(i * matrixSize, matrixSize);
                using var columnSums = Provider.SumColumns(tensorPtr, matrixSize, depth, retPtr);
                Provider.AddInPlace(retMemory, columnSums, depth, 1f, 1f);
            }
            return Lap.CreateVector(ret);
        }
    }
}
