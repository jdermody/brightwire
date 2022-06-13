using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.LinearAlegbra2;

namespace BrightData.Cuda
{
    public class CudaMatrix2 : Matrix2<CudaComputationUnit>
    {
        public CudaMatrix2(ITensorSegment2 data, uint rows, uint columns, CudaComputationUnit computationUnit) : base(data, rows, columns, computationUnit)
        {
        }
    }
}
