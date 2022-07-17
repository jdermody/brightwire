using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.LinearAlgebra;

namespace BrightData.Cuda
{
    public class CudaTensor4D : BrightTensor4D<CudaLinearAlgebraProvider>
    {
        public CudaTensor4D(ITensorSegment data, uint count, uint depth, uint rows, uint columns, CudaLinearAlgebraProvider lap) : base(data, count, depth, rows, columns, lap)
        {
        }
    }
}
