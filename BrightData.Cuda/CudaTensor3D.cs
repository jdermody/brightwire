using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.LinearAlgebra;

namespace BrightData.Cuda
{
    /// <inheritdoc />
    public class CudaTensor3D : BrightTensor3D<CudaLinearAlgebraProvider>
    {
        /// <inheritdoc />
        public CudaTensor3D(ITensorSegment data, uint depth, uint rowCount, uint columnCount, CudaLinearAlgebraProvider lap) : base(data, depth, rowCount, columnCount, lap)
        {
        }
    }
}
