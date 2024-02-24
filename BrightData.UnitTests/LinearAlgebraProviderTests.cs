using BrightData.UnitTests.Helper;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class LinearAlgebraProviderTests : UnitTestBase
    {
        [Fact]
        public void TestMapParallel()
        {
            using var input = _context.LinearAlgebraProvider.CreateSegment(1, 2, 3);
            using var output = input.MapParallel(x => x * 2);
            output[0].Should().Be(2);
            output[1].Should().Be(4);
            output[2].Should().Be(6);
        }

        [Fact]
        public void TestMapParallel2()
        {
            using var input = _context.LinearAlgebraProvider.CreateSegment(1, 2, 3);
            using var output = input.MapParallel((i, x) => x * i);
            output[0].Should().Be(0);
            output[1].Should().Be(2);
            output[2].Should().Be(6);
        }

        [Fact]
        public void TestMapParallelInPlace()
        {
            using var input = _context.LinearAlgebraProvider.CreateSegment(1, 2, 3);
            input.MapParallelInPlace(x => x * 2);
            input[0].Should().Be(2);
            input[1].Should().Be(4);
            input[2].Should().Be(6);
        }

        [Fact]
        public void TestMapParallelInPlace2()
        {
            using var input = _context.LinearAlgebraProvider.CreateSegment(1, 2, 3);
            input.MapParallelInPlace((i, x) => x * i);
            input[0].Should().Be(0);
            input[1].Should().Be(2);
            input[2].Should().Be(6);
        }
    }
}
