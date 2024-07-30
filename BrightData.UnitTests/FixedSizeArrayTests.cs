using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.Types;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class FixedSizeArrayTests
    {
        [Fact]
        public void TestAscending()
        {
            var array = new FixedSizeSortedAscending2Array<uint, float>();
            array.TryAdd(1, 0.5f).Should().BeTrue();
            array.TryAdd(1, 0.5f).Should().BeFalse();
            array.TryAdd(2, 0.3f).Should().BeTrue();
            array.TryAdd(3, 0.8f).Should().BeFalse();
            array.MinValue.Should().Be(2);
            array.MaxValue.Should().Be(1);
            array.RemoveAt(0);
            array.Size.Should().Be(1);
            array.MinValue.Should().Be(1);
        }

        [Fact]
        public void TestDescending()
        {
            var array = new FixedSizeSortedDescending2Array<uint, float>();
            array.TryAdd(1, 0.5f).Should().BeTrue();
            array.TryAdd(1, 0.5f).Should().BeFalse();
            array.TryAdd(2, 0.8f).Should().BeTrue();
            array.TryAdd(3, 0.3f).Should().BeFalse();
            array.MinValue.Should().Be(1);
            array.MaxValue.Should().Be(2);
            array.RemoveAt(0);
            array.Size.Should().Be(1);
            array.MaxValue.Should().Be(1);
        }
    }
}
