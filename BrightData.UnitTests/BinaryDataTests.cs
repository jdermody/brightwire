using BrightData.UnitTests.Helper;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BrightData.Serialisation;
using Xunit;

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
    }
}
