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
            return Lap.CreateVector(new CudaTensorSegment(ptr));
        }

        /// <inheritdoc />
        public override IVector GetRowVector(uint index)
        {
            //return _lap.CreateVector(Row(index));
            var segment = Lap.GetNonContinuousSegment(Segment, index, RowCount, ColumnCount);
            return Lap.CreateVector(segment);
        }
    }
}
