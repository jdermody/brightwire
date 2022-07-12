using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.LinearAlgebra;

namespace BrightData.MKL
{
    public class MklMatrix : BrightMatrix<MklLinearAlgebraProvider>
    {
        public MklMatrix(ITensorSegment data, uint rows, uint columns, MklLinearAlgebraProvider computationUnit) : base(data, rows, columns, computationUnit)
        {
        }

        public override IMatrix Create(ITensorSegment segment) => new MklMatrix(segment, RowCount, ColumnCount, _lap);
    }
}
