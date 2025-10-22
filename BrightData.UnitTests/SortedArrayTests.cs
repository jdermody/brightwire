using BrightData.Types;
using FluentAssertions;
using Xunit;
using BrightData.Types.Graph.Helper;

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
    }
}
