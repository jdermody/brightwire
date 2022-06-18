using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.LinearAlegbra2;

namespace BrightData.MKL
{
    public class MklMatrix : Matrix2<MklLinearAlgebraProvider>
    {
        public MklMatrix(ITensorSegment2 data, uint rows, uint columns, MklLinearAlgebraProvider computationUnit) : base(data, rows, columns, computationUnit)
        {
        }

        public override IMatrix Create(ITensorSegment2 segment) => new MklMatrix(segment, RowCount, ColumnCount, _lap);
    }
}
