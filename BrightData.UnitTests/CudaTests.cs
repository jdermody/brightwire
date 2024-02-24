using BrightData.Cuda;
using BrightData.LinearAlgebra.Segments;
using BrightData.Types;
using BrightData.UnitTests.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class CudaTests : CudaBase
    {
        [Fact]
        public void CopyToWrapperWithStride()
        {
            var empty = _cuda.CreateSegment(24, true);
            var ones = _cuda.CreateSegment(8, _ => 1);
            var wrapper = new MutableTensorSegmentWrapper<float>(empty, 0, 3, 8);
            ones.CopyTo(wrapper);
            empty.ToNewArray()[..6].Should().BeEquivalentTo([1, 0, 0, 1, 0, 0]);
        }
    }
}
