using BrightData.UnitTests.Helper;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class MemoryTests : CudaBase
    {
        [Fact]
        public void CudaMemoryLayerTest()
        {
#if DEBUG
            var matrix = _cuda.CreateMatrix(10, 10);
            _cuda.PushScope();
            var matrix2 = _cuda.CreateMatrix(10, 10);
            _cuda.PopScope();
            matrix2.Segment.IsValid.Should().BeFalse();
            matrix.Segment.IsValid.Should().BeTrue();
            matrix.Dispose();
            matrix.Segment.IsValid.Should().BeFalse();
#endif
        }
	}
}
