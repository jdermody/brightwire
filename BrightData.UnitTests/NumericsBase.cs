using System;
using BrightData.Numerics;

namespace BrightData.UnitTests
{
    public class NumericsBase : IDisposable
    {
        protected readonly IBrightDataContext _context;
        protected readonly ILinearAlgebraProvider _simple;
        protected readonly ILinearAlgebraProvider _cpu;

        public NumericsBase()
        {
            _context = new BrightDataContext();
            _simple = _context.LinearAlgebraProvider;
            _cpu = _context.UseNumericsLinearAlgebra();
        }

        public virtual void Dispose()
        {
            _cpu.Dispose();
            _simple.Dispose();
            _context.Dispose();
        }
    }
}
