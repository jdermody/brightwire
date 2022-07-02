using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.LinearAlegbra2;

namespace BrightData.OpenBlas
{
    internal class OpenBlasMatrix : Matrix2<OpenBlasLinearAlgebraProvider>
    {
        public OpenBlasMatrix(ITensorSegment2 data, uint rows, uint columns, OpenBlasLinearAlgebraProvider computationUnit) : base(data, rows, columns, computationUnit)
        {
        }

        public override IMatrix Create(ITensorSegment2 segment) => new OpenBlasMatrix(segment, RowCount, ColumnCount, _lap);
    }
}
