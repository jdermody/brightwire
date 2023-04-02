using BrightData.LinearAlgebra;
using BrightData.MKL;
using System;

namespace BrightData.UnitTests.Helper
{
    public class CpuBase : UnitTestBase
    {
        protected readonly LinearAlgebraProvider _cpu;
        protected readonly MklLinearAlgebraProvider _mkl;

        public CpuBase()
        {
            _cpu = _context.LinearAlgebraProvider;
            _mkl = new MklLinearAlgebraProvider(_context);
        }

        public override void Dispose()
        {
            GC.SuppressFinalize(this);
            _mkl.Dispose();
            _cpu.Dispose();
            base.Dispose();
        }
    }
}
