using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData2;

namespace BrightData.Cuda
{
    internal class CudaVector2 : Vector2<CudaComputationUnit>
    {
        public CudaVector2(ITensorSegment2 data, CudaComputationUnit computationUnit) : base(data, computationUnit)
        {
        }
    }
}
