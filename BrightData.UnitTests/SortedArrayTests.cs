using BrightData.Types.Graph;
using BrightData.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class SortedArrayTests
    {
        [Fact]
        public void TestIndexedSortedArray()
        {
            var array = new IndexedSortedArray<GraphNodeIndex>(4);
            for (var i = 0U; i < 4; i++)
                array.Add(new(4-i));
            array.Size.Should().Be(4);
            array[0].Index.Should().Be(1);
            array[3].Index.Should().Be(4);
            array.TryFind(1, out var index).Should().BeTrue();
            index!.Value.Index.Should().Be(1);
        }

        [Fact]
        public void TestSortedArray()
        {
            var array = new SortedArray<uint, float>(4);
            for (var i = 0U; i < 4; i++)
                array.Add(i, 4-i);
            array.Size.Should().Be(4);
            array[0].Weight.Should().Be(1);
            array[3].Weight.Should().Be(4);
            array.TryFind(1, out var value).Should().BeTrue();
            value.Should().Be(3);
        }
    }
}
