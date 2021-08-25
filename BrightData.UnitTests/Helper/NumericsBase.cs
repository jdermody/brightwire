using BrightData.Numerics;

namespace BrightData.UnitTests.Helper
{


    public class NumericsBase : UnitTestBase
    {
        protected readonly ILinearAlgebraProvider _simple;
        protected readonly ILinearAlgebraProvider _cpu;

        public NumericsBase()
        {
            _simple = _context.LinearAlgebraProvider;
            _cpu = _context.UseNumericsLinearAlgebra();
        }

        public override void Dispose()
        {
            _cpu.Dispose();
            _simple.Dispose();
            base.Dispose();
        }
    }
}
