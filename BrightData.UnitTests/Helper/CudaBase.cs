using System;
using System.IO;
using BrightData.Cuda;

namespace BrightData.UnitTests.Helper
{
    public class CudaBase : NumericsBase
    {
        readonly ILinearAlgebraProvider              _cudaProvider;
        protected readonly CudaLinearAlgebraProvider _cuda;

        public CudaBase()
        {
            _cudaProvider = _context.UseCudaLinearAlgebra(Path.Combine(Environment.CurrentDirectory, "cuda", "brightwire.ptx"));
            _cuda = new CudaLinearAlgebraProvider(_context, (CudaProvider)_cudaProvider);
        }

        public override void Dispose()
        {
            _cuda.Dispose();
            _cudaProvider.Dispose();
            base.Dispose();
        }
    }
}
