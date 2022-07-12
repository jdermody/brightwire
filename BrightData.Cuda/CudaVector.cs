using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.LinearAlgebra;

namespace BrightData.Cuda
{
    internal class CudaVector : BrightVector<CudaLinearAlgebraProvider>, IHaveDeviceMemory
    {
        public CudaVector(ITensorSegment data, CudaLinearAlgebraProvider computationUnit) : base(data, computationUnit)
        {
        }

        public IDeviceMemoryPtr Memory => CudaLinearAlgebraProvider.GetDeviceMemoryPtr(Segment);
    }
}
