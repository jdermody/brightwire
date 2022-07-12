using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.LinearAlgebra;

namespace BrightData.Cuda
{
    public class CudaMatrix : BrightMatrix<CudaLinearAlgebraProvider>
    {
        public CudaMatrix(ITensorSegment data, uint rows, uint columns, CudaLinearAlgebraProvider lap) : base(data, rows, columns, lap)
        {
        }

        public override IVectorInfo[] AllColumns()
        {
            var segment = new ArrayBasedTensorSegment(Segment.ToNewArray());
            var ret = new IVectorInfo[ColumnCount];
            for (uint i = 0; i < ColumnCount; i++)
                ret[i] = Column(i, segment).ToVectorInfo();
            return ret;
        }

        public override IVectorInfo[] AllRows()
        {
            var segment = new ArrayBasedTensorSegment(Segment.ToNewArray());
            var ret = new IVectorInfo[RowCount];
            for (uint i = 0; i < RowCount; i++)
                ret[i] = Row(i, segment).ToVectorInfo();
            return ret;
        }

        public override IVector GetColumnVector(uint index)
        {
            //var segment = _lap.GetNonContinuousSegment(Segment, index, RowCount, ColumnCount);
            //return _lap.CreateVector(segment);
            return base.GetColumnVector(index);
        }

        public override IVector GetRowVector(uint index)
        {
            var segment = _lap.GetNonContinuousSegment(Segment, index, RowCount, ColumnCount);
            return _lap.CreateVector(segment);
        }

        public override IVectorInfo GetColumn(uint columnIndex)
        {
            return base.GetColumn(columnIndex);
        }

        public override IVectorInfo GetRow(uint rowIndex)
        {
            return base.GetRow(rowIndex);
        }
    }
}
