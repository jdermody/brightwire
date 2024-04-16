using System;
using System.Linq;
using BrightData.Helper;
using FluentAssertions;

namespace BrightData.UnitTests.Helper
{
    public class UnitTestBase : IDisposable
    {
        protected readonly BrightDataContext _context = new(null, 0);
        protected readonly Random _random = new(0);

        public IReadOnlyVector<float> CreateRandomVector(uint size = 32)
        {
            var rand = new Random();
            return _context.CreateReadOnlyVector(size, _ => Math<float>.Next(rand));
        }

        protected ReadOnlySpan<float> CreateFloatSpan(uint size = 32) => size.AsRange().Select(_ => _random.NextSingle()).ToArray();

        protected static void AssertSame(params float[] values)
        {
            var first = values[0];
            for(var i = 1; i < values.Length; i++)
                Math<float>.AreApproximatelyEqual(first, values[i]).Should().BeTrue();
        }

        protected static void AssertSameWithMaxDifference(int maxDifference, params float[] values)
        {
            var first = values[0];
            for(var i = 1; i < values.Length; i++)
                Math<float>.AreApproximatelyEqual(first, values[i], maxDifference).Should().BeTrue();
        }

        protected static void AssertSame<T>(params T[] tensors) where T: IHaveReadOnlyTensorSegment<float>
        {
            var first = tensors[0];
            for(var i = 1; i < tensors.Length; i++)
                Math<float>.AreApproximatelyEqual(first, tensors[i]).Should().BeTrue();
        }
        protected static void AssertSame(params INumericSegment<float>[] tensors)
        {
            var first = tensors[0];
            for(var i = 1; i < tensors.Length; i++)
                Math<float>.AreApproximatelyEqual(first, tensors[i]).Should().BeTrue();
        }

        protected static void AssertSame(params float[][] tensors)
        {
            var first = tensors[0];
            for(var i = 1; i < tensors.Length; i++)
                Math<float>.AreApproximatelyEqual(first, tensors[i]).Should().BeTrue();
        }

        protected static void AssertSameWithMaxDifference<T>(int maxDifference, params T[] tensors) where T: IHaveReadOnlyTensorSegment<float>
        {
            var first = tensors[0];
            for(var i = 1; i < tensors.Length; i++)
                Math<float>.AreApproximatelyEqual(first, tensors[i], maxDifference).Should().BeTrue();
        }

        protected static void AssertSameAndThenDispose(params ITensor<float>[] tensors)
        {
            try {
                AssertSame(tensors);
            }
            finally {
                tensors.DisposeAll();
            }
        }
        protected static void AssertSameAndThenDispose(params INumericSegment<float>[] tensors)
        {
            try {
                AssertSame(tensors);
            }
            finally {
                tensors.DisposeAll();
            }
        }
        protected static void AssertSameAndThenDispose(params ITensor<float>[][] tensors)
        {
            try {
                var first = tensors[0];
                var size = first.Length;
                for (var i = 1; i < tensors.Length; i++) {
                    for(uint j = 0; j < size; j++)
                        Math<float>.AreApproximatelyEqual(first[j], tensors[i][j]).Should().BeTrue();
                }
            }
            finally {
                foreach(var item in tensors)
                    item.DisposeAll();
            }
        }
        protected static void AssertSameAndThenDispose(int maxDifference, params INumericSegment<float>[][] tensors)
        {
            try {
                var first = tensors[0];
                var size = first.Length;
                for (var i = 1; i < tensors.Length; i++) {
                    for (uint j = 0; j < size; j++) {
                        var v1 = first[j];
                        var v2 = tensors[i][j];
                        Math<float>.AreApproximatelyEqual(v1, v2, maxDifference).Should().BeTrue();
                    }
                }
            }
            finally {
                foreach(var item in tensors)
                    item.DisposeAll();
            }
        }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
            _context.Dispose();
        }
    }
}
