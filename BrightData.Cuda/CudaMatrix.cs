using BrightData.LinearAlgebra;
using BrightData.LinearAlgebra.Segments;

namespace BrightData.Cuda
{
    /// <inheritdoc />
    public class CudaMatrix : BrightMatrix<CudaLinearAlgebraProvider>
    {
        /// <inheritdoc />
        public CudaMatrix(ITensorSegment data, uint rows, uint columns, CudaLinearAlgebraProvider lap) : base(data, rows, columns, lap)
        {
        }

        /// <inheritdoc />
        public override IReadOnlyVector[] AllColumnsAsReadOnly(bool makeCopy)
        {
            var segment = new ArrayBasedTensorSegment(Segment.ToNewArray());
            var ret = new IReadOnlyVector[ColumnCount];
            for (uint i = 0; i < ColumnCount; i++)
                ret[i] = Column(i, segment).ToReadOnlyVector();
            return ret;
        }

        /// <inheritdoc />
        public override IReadOnlyVector[] AllRowsAsReadOnly(bool makeCopy)
        {
            var segment = new ArrayBasedTensorSegment(Segment.ToNewArray());
            var ret = new IReadOnlyVector[RowCount];
            for (uint i = 0; i < RowCount; i++)
                ret[i] = Row(i, segment).ToReadOnlyVector();
            return ret;
        }

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
