using BrightData.LinearAlegbra2;
using BrightData.Numerics;

namespace BrightData.UnitTests.Helper
{
    public class NumericsBase : UnitTestBase
    {
        protected readonly LinearAlgebraProvider _lap;

        public NumericsBase()
        {
            _lap = _context.LinearAlgebraProvider2;
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
