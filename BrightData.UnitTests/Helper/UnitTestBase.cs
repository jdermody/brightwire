using System;
using BrightData.Helper;
using BrightData.LinearAlgebra;
using FluentAssertions;

namespace BrightData.UnitTests.Helper
{
    public class UnitTestBase : IDisposable
    {
        protected readonly BrightDataContext _context = new(null, 0);

        public IVectorInfo CreateRandomVector(uint size = 32)
        {
            var rand = new Random();
            return _context.CreateVectorInfo(size, _ => FloatMath.Next(rand));
        }

        protected static void AssertSame(params float[] values)
        {
            var first = values[0];
            for(var i = 1; i < values.Length; i++)
                FloatMath.AreApproximatelyEqual(first, values[i]).Should().BeTrue();
        }

        protected static void AssertSameWithMaxDifference(int maxDifference, params float[] values)
        {
            var first = values[0];
            for(var i = 1; i < values.Length; i++)
                FloatMath.AreApproximatelyEqual(first, values[i], maxDifference).Should().BeTrue();
        }

        protected static void AssertSame<T>(params T[] tensors) where T: IHaveSize, IHaveSpan
        {
            var first = tensors[0];
            for(var i = 1; i < tensors.Length; i++)
                FloatMath.AreApproximatelyEqual(first, tensors[i]).Should().BeTrue();
        }

        protected static void AssertSameWithMaxDifference<T>(int maxDifference, params T[] tensors) where T: IHaveSize, IHaveSpan
        {
            var first = tensors[0];
            for(var i = 1; i < tensors.Length; i++)
                FloatMath.AreApproximatelyEqual(first, tensors[i], maxDifference).Should().BeTrue();
        }

        protected static void AssertSameAndThenDispose(params ITensor2[] tensors)
        {
            try {
                AssertSame(tensors);
            }
            finally {
                tensors.DisposeAll();
            }
        }

        public virtual void Dispose()
        {
            _context.Dispose();
        }
    }
}
