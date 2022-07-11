using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.LinearAlgebra;

namespace BrightData.Cuda
{
    public class CudaMatrix : Matrix<CudaLinearAlgebraProvider>
    {
        public CudaMatrix(ITensorSegment2 data, uint rows, uint columns, CudaLinearAlgebraProvider computationUnit) : base(data, rows, columns, computationUnit)
        {
        }
    }
}
