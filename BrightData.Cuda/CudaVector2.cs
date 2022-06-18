using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.LinearAlegbra2;

namespace BrightData.Cuda
{
    internal class CudaVector2 : Vector2<CudaLinearAlgebraProvider>
    {
        public CudaVector2(ITensorSegment2 data, CudaLinearAlgebraProvider computationUnit) : base(data, computationUnit)
        {
        }
    }
}
