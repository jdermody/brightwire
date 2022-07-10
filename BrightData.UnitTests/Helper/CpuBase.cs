using BrightData.LinearAlgebra;
using BrightData.MKL;

namespace BrightData.UnitTests.Helper
{
    public class CpuBase : UnitTestBase
    {
        protected readonly LinearAlgebraProvider _cpu;
        protected readonly MklLinearAlgebraProvider _mkl;

        public CpuBase()
        {
            _cpu = _context.LinearAlgebraProvider2;
            _mkl = new MklLinearAlgebraProvider(_context);
        }

        public override void Dispose()
        {
            _mkl.Dispose();
            _cpu.Dispose();
            base.Dispose();
        }
    }
}
