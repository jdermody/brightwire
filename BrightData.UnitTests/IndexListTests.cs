using System.Collections.Generic;
using System.IO;
using BrightData.Helper;
using BrightData.UnitTests.Helper;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class IndexListTests : UnitTestBase
    {
        [Fact]
        public void Merge()
        {
            var first = IndexList.Create(1, 2, 3);
            var second = IndexList.Create(2, 3, 4);
            var merged = IndexList.Merge(new[] {first, second});
            merged.Size.Should().Be(4);
        }

        [Fact]
        public void Inequality()
        {
            var first = IndexList.Create(1, 2, 3);
            var second = IndexList.Create(2, 3, 4);
            first.Equals(second).Should().BeFalse();
        }

        [Fact]
        public void Equality()
        {
            var first = IndexList.Create(1, 2, 3);
            var second = IndexList.Create(1, 2, 3);
            first.Equals(second).Should().BeTrue();
            var set = new HashSet<IndexList> {
                first, second
            };
            set.Count.Should().Be(1);
        }

        [Fact]
        public void Serialisation()
        {
            var first = IndexList.Create(1, 2, 3);
            var data = first.GetData();
            var reader = new BinaryReader(new MemoryStream(data));
            var second = _context.CreateIndexList(reader);
            second.Should().BeEquivalentTo(first);
        }

        [Fact]
        public void JaccardSimilarity()
        {
            var first = IndexList.Create(1, 2, 3);
            var second = IndexList.Create(1, 2, 3);
            var similarity = first.JaccardSimilarity(second);
            similarity.Should().Be(1);
        }

        [Fact]
        public void Serialisation2()
        {
            var first = IndexList.Create(1, 2, 3);
            var data = first.GetData();
            var reader = new BinaryReader(new MemoryStream(data));
            var second = GenericActivator.CreateUninitialized<IndexList>();
            second.Initialize(_context, reader);
            second.Should().BeEquivalentTo(first);
        }

        [Fact]
        public void ToDense()
        {
            var first = IndexList.Create(1, 2, 3);
            var vector = first.AsDense();
            vector.Size.Should().Be(4);

            vector[0].Should().Be(0f);
            vector[3].Should().Be(1f);
        }
    }
}
