using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.Serialisation;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class SerialisationTests
    {
        [Fact]
        public void TestStructArray()
        {
            using var buffer = new MemoryStream();
            using var writer = new BinaryWriter(buffer, Encoding.UTF8, true);
            var testArray = new uint[] { 1, 2, 3, 4 };
            testArray.WriteTo(writer);
            writer.Flush();

            buffer.Seek(0, SeekOrigin.Begin);
            using var reader = new BinaryReader(buffer, Encoding.UTF8, true);
            var fromReader = reader.ReadStructArray<uint>();
            fromReader.Should().BeEquivalentTo(testArray);
        }
    }
}
