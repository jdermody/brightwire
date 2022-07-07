using BrightData.LinearAlegbra2;
using BrightData.MKL;

namespace BrightData.UnitTests.Helper
{
    public class NumericsBase : UnitTestBase
    {
        protected readonly LinearAlgebraProvider _cpu;
        protected readonly MklLinearAlgebraProvider _mkl;

        public NumericsBase()
        {
            _cpu = _context.LinearAlgebraProvider2;
            _mkl = new MklLinearAlgebraProvider(_context);
        }

        public override void Dispose()
        {
            _mkl.Dispose();
            base.Dispose();
        }
    }
}
