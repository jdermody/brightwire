using System;
using BrightData.Helper;
using BrightData.LinearAlgebra;
using FluentAssertions;

namespace BrightData.UnitTests.Helper
{
    public class UnitTestBase : IDisposable
    {
        protected readonly BrightDataContext _context = new(null, 0);

        public Vector<float> CreateRandomVector(uint size = 32)
        {
            var rand = new Random();
            return _context.CreateVector(size, _ => FloatMath.Next(rand));
        }

        public static void CheckEquivalent<T>(ITensor<T> tensor1, ITensor<T> tensor2) where T: struct
        {
            tensor1.Should().BeEquivalentTo(tensor2, options => options.Excluding(t => t.Segment.AllocationIndex));
        }

        public virtual void Dispose()
        {
            _context.Dispose();
        }
    }
}
