using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BrightData.Helper;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class IndexListTests : UnitTestBase
    {
        [Fact]
        public void Merge()
        {
            var first = _context.CreateIndexList(1, 2, 3);
            var second = _context.CreateIndexList(2, 3, 4);
            var merged = IndexList.Merge(new[] {first, second});
            merged.Count.Should().Be(4);
        }

        [Fact]
        public void Inequality()
        {
            var first = _context.CreateIndexList(1, 2, 3);
            var second = _context.CreateIndexList(2, 3, 4);
            first.Equals(second).Should().BeFalse();
        }

        [Fact]
        public void Equality()
        {
            var first = _context.CreateIndexList(1, 2, 3);
            var second = _context.CreateIndexList(1, 2, 3);
            first.Equals(second).Should().BeTrue();
        }

        [Fact]
        public void Serialisation()
        {
            var first = _context.CreateIndexList(1, 2, 3);
            var data = first.GetData();
            var reader = new BinaryReader(new MemoryStream(data));
            var second = _context.CreateIndexList(reader);
            second.Should().BeEquivalentTo(first);
        }

        [Fact]
        public void JaccardSimilarity()
        {
            var first = _context.CreateIndexList(1, 2, 3);
            var second = _context.CreateIndexList(1, 2, 3);
            var similarity = first.JaccardSimilarity(second);
            similarity.Should().Be(1);
        }

        [Fact]
        public void Serialisation2()
        {
            var first = _context.CreateIndexList(1, 2, 3);
            var data = first.GetData();
            var reader = new BinaryReader(new MemoryStream(data));
            var second = GenericActivator.CreateUninitialized<IndexList>();
            second.Initialize(_context, reader);
            second.Should().BeEquivalentTo(first);
        }

        [Fact]
        public void ToDense()
        {
            var first = _context.CreateIndexList(1, 2, 3);
            var vector = first.AsDense();
            vector.Size.Should().Be(4);

            vector[0].Should().Be(0f);
            vector[3].Should().Be(1f);
        }
    }
}
