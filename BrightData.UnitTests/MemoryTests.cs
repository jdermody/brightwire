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
            var matrix = _cuda.CreateZeroMatrix(10, 10);
            _cuda.PushLayer();
            var matrix2 = _cuda.CreateZeroMatrix(10, 10);
            _cuda.PopLayer();
            matrix2.IsValid.Should().BeFalse();
            matrix.IsValid.Should().BeTrue();
            matrix.Dispose();
            matrix.IsValid.Should().BeFalse();
#endif
        }
	}
}
