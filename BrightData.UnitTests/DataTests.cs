using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class DataTests
    {
        [Fact]
        public void FindContiguousRanges()
        {
            var items = new uint[] { 1, 2, 3, 6, 6, 7, 9, 10 };
            var ranges = items.FindDistinctContiguousRanges().ToArray();
            ranges.Length.Should().Be(3);

            ranges[0].First.Should().Be(1);
            ranges[0].Last.Should().Be(3);

            ranges[1].First.Should().Be(6);
            ranges[1].Last.Should().Be(7);

            ranges[2].First.Should().Be(9);
            ranges[2].Last.Should().Be(10);
        }
    }
}
