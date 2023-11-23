using System.Collections.Generic;
using BrightData.UnitTests.Helper;
using FluentAssertions;
using System.IO;
using System.Runtime.CompilerServices;
using Xunit;
using System;
using BrightData.Helper;
using BrightData.Types;

namespace BrightData.UnitTests
{
    public class BinaryDataTests : UnitTestBase
    {
        [Fact]
        public void TestSize()
        {
            Unsafe.SizeOf<BinaryData>().Should().Be(Unsafe.SizeOf<byte[]>());
        }

        [Fact]
        public void Serialisation()
        {
            var first = new BinaryData(new byte[] { 1, 2, 3, 4 });
            var data = first.GetData();
            var reader = new BinaryReader(new MemoryStream(data));
            var second = _context.Create<BinaryData>(reader);
            second.Should().BeEquivalentTo(first);
        }

        [Fact]
        public void Equality()
        {
            var first = new BinaryData(new byte[] { 1, 2, 3, 4 });
            var second = new BinaryData(new byte[] { 1, 2, 3, 4 });
            first.Should().BeEquivalentTo(second);
            first.ToString().Should().Be(second.ToString());
            var set = new HashSet<BinaryData> {
                first, second
            };
            set.Count.Should().Be(1);
        }
    }
}
