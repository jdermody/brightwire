using System;
using System.IO;
using BrightData.Cuda;

namespace BrightData.UnitTests.Helper
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
