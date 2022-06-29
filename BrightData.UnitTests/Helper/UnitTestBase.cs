using System;
using BrightData.Helper;
using BrightData.LinearAlgebra;
using FluentAssertions;

namespace BrightData.UnitTests.Helper
{
    public class UnitTestBase : IDisposable
    {
        protected readonly BrightDataContext _context = new(null, 0);

        public IVector CreateRandomVector(uint size = 32)
        {
            var rand = new Random();
            return _context.CreateVector(size, _ => FloatMath.Next(rand));
        }

        public virtual void Dispose()
        {
            _context.Dispose();
        }
    }
}
