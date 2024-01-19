using BrightData.LinearAlgebra;

namespace BrightData.Cuda
{
    /// <inheritdoc />
    public class CudaMatrix : BrightMatrix<CudaLinearAlgebraProvider>
    {
        /// <inheritdoc />
        public CudaMatrix(INumericSegment<float> data, uint rows, uint columns, CudaLinearAlgebraProvider lap) : base(data, rows, columns, lap)
        {
        }

        /// <inheritdoc />
        public override IReadOnlyVector[] AllColumnsAsReadOnly(bool makeCopy) => base.AllColumnsAsReadOnly(true);

        /// <inheritdoc />
        public override IReadOnlyVector[] AllRowsAsReadOnly(bool makeCopy) => base.AllRowsAsReadOnly(true);

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
