using BrightData.Cuda.CudaToolkit.Types;
using BrightData.Cuda.CudaToolkit;
using BrightData.LinearAlgebra;

namespace BrightData.Cuda
{
    /// <inheritdoc />
    /// <inheritdoc />
    public class CudaMatrix(INumericSegment<float> data, uint rows, uint columns, CudaLinearAlgebraProvider lap) 
        : MutableMatrix<CudaLinearAlgebraProvider>(data, rows, columns, lap)
    {

        /// <inheritdoc />
        public override IVector GetColumnVector(uint index)
        {
            var segment = (CudaTensorSegment)Segment;
            var ptr = segment.DeviceMemory.Offset(index * RowCount, RowCount);
            return Lap.CreateVector(new CudaTensorSegment(ptr, lap.Provider));
        }

        /// <inheritdoc />
        public override IVector GetRowVector(uint index)
        {
            //return _lap.CreateVector(Row(index));
            var segment = Lap.GetNonContinuousSegment(Segment, index, RowCount, ColumnCount);
            return Lap.CreateVector(segment);
        }

        /// <inheritdoc />
        public override unsafe IMatrix Transpose()
        {
            var provider = lap.Provider;
            var ret = provider.Allocate(RowCount * ColumnCount);
            float alpha = 1.0f, beta = 0.0f;
            CudaBlasNativeMethods.cublasSgeam(provider.Blas,
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
            return lap.CreateMatrix(ColumnCount, RowCount, new CudaTensorSegment(ret, lap.Provider));
        }
    }
}
