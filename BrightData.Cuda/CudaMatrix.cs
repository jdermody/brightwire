using BrightData.LinearAlgebra;

namespace BrightData.Cuda
{
    public class CudaMatrix : BrightMatrix<CudaLinearAlgebraProvider>
    {
        public CudaMatrix(ITensorSegment data, uint rows, uint columns, CudaLinearAlgebraProvider lap) : base(data, rows, columns, lap)
        {
        }

        public override IReadOnlyVector[] AllColumns(bool makeCopy)
        {
            var segment = new ArrayBasedTensorSegment(Segment.ToNewArray());
            var ret = new IReadOnlyVector[ColumnCount];
            for (uint i = 0; i < ColumnCount; i++)
                ret[i] = Column(i, segment).ToVectorInfo();
            return ret;
        }

        public override IReadOnlyVector[] AllRows(bool makeCopy)
        {
            var segment = new ArrayBasedTensorSegment(Segment.ToNewArray());
            var ret = new IReadOnlyVector[RowCount];
            for (uint i = 0; i < RowCount; i++)
                ret[i] = Row(i, segment).ToVectorInfo();
            return ret;
        }

        public override IVector GetColumnVector(uint index)
        {
            var segment = (CudaTensorSegment)Segment;
            var ptr = segment.DeviceMemory.Offset(index * RowCount, RowCount);
            return _lap.CreateVector(new CudaTensorSegment(ptr));
        }

        public override IVector GetRowVector(uint index)
        {
            var segment = _lap.GetNonContinuousSegment(Segment, index, RowCount, ColumnCount);
            return _lap.CreateVector(segment);
        }
    }
}
