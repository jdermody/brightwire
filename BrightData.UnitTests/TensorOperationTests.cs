using System;
using BrightData.Helper;
using BrightData.UnitTests.Fixtures;
using BrightData.UnitTests.Helper;
using Xunit;
using FluentAssertions;
using System.Linq;

namespace BrightData.UnitTests
{
    public class TensorOperationTests : UnitTestBase
    {
        [Fact]
        public void TestAdd() 
        {
            var a = CreateRandomVector();
            var b = CreateRandomVector();
            var result = a.Add(b);
            result.Values.Should().ContainInOrder(a.ReadOnlySegment.Values.Zip(b.ReadOnlySegment.Values, (x, y) => x + y));
        }

    }
}