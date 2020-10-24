using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BrightData.Cuda;
using BrightData.Numerics;

namespace BrightData.UnitTests
{
    public class CudaBase : NumericsBase
    {
        protected readonly ILinearAlgebraProvider _cuda;

        public CudaBase()
        {
            _cuda = _context.UseCudaLinearAlgebra(Path.Combine(Environment.CurrentDirectory, "cuda", "brightwire.ptx"));
        }

        public override void Dispose()
        {
            _cuda.Dispose();
            base.Dispose();
        }
    }
}
