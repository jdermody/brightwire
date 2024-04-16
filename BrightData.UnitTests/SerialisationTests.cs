using System.IO;
using System.Linq;
using System.Text;
using BrightData.Helper;
using BrightData.UnitTests.Helper;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class SerialisationTests : UnitTestBase
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

        [Fact]
        public void SimpleSerialisation()
        {
            using var buffer = new MemoryStream();
            using var writer = new BinaryWriter(buffer, Encoding.UTF8, true);
            using var reader = new BinaryReader(buffer, Encoding.UTF8, true);

            var str = "this is a test";
            int integer = -123;
            uint unsignedInteger = 123;
            double dbl = 123d;
            float flt = 123f;
            str.WriteTo(writer);
            integer.WriteTo(writer);
            unsignedInteger.WriteTo(writer);
            dbl.WriteTo(writer);
            flt.WriteTo(writer);

            writer.Flush();
            buffer.Seek(0, SeekOrigin.Begin);

            reader.ReadString().Should().Be(str);
            reader.ReadInt32().Should().Be(integer);
            reader.ReadUInt32().Should().Be(unsignedInteger);
            reader.ReadDouble().Should().Be(dbl);
            reader.ReadSingle().Should().Be(flt);
        }

        [Fact]
        public void NullableSerialisation()
        {
            using var buffer = new MemoryStream();
            using var writer = new BinaryWriter(buffer, Encoding.UTF8, true);
            using var reader = new BinaryReader(buffer, Encoding.UTF8, true);

            int? val = null;
            int? val2 = 123;
            string? str = null;

            val.WriteTo(writer, writer.Write);
            val2.WriteTo(writer, writer.Write);
            str.WriteTo(writer);

            writer.Flush();
            buffer.Seek(0, SeekOrigin.Begin);

            reader.ReadNullable(reader.ReadInt32).Should().Be(val);
            reader.ReadNullable(reader.ReadInt32).Should().Be(val2);
            reader.ReadString().Should().Be(string.Empty);
        }

        [Fact]
        public void ArrayStreamSerialisation()
        {
            using var buffer = new MemoryStream();
            var array = Enumerable.Range(0, 20).ToArray();
            array.WriteTo(buffer);
            buffer.Seek(0, SeekOrigin.Begin);
            var array2 = buffer.Enumerate<int>((uint)array.Length).ToArray();
            array2.Should().BeEquivalentTo(array);
        }

        [Fact]
        public void ArraySerialisation()
        {
            using var buffer = new MemoryStream();
            using var writer = new BinaryWriter(buffer, Encoding.UTF8, true);
            using var reader = new BinaryReader(buffer, Encoding.UTF8, true);
            var array = Enumerable.Range(0, 20).ToArray();

            array.WriteTo(writer);
            writer.Flush();
            buffer.Seek(0, SeekOrigin.Begin);

            reader.ReadStructArray<int>().Should().BeEquivalentTo(array);
        }

        [Fact]
        public void StringArraySerialisation()
        {
            using var buffer = new MemoryStream();
            using var writer = new BinaryWriter(buffer, Encoding.UTF8, true);
            using var reader = new BinaryReader(buffer, Encoding.UTF8, true);
            var stringArray = new[] {
                "test",
                "another test"
            };

            stringArray.WriteTo(writer);
            writer.Flush();
            buffer.Seek(0, SeekOrigin.Begin);

            reader.ReadStringArray().Should().BeEquivalentTo(stringArray);
        }

        [Fact]
        public void TensorSerialisation()
        {
            using var buffer = new MemoryStream();
            using var writer = new BinaryWriter(buffer, Encoding.UTF8, true);
            using var reader = new BinaryReader(buffer, Encoding.UTF8, true);

            using var vector = _context.LinearAlgebraProvider.CreateVector(1f, 2f, 3f);
            using var matrix = _context.LinearAlgebraProvider.CreateMatrix(2, 2, (i, j) => i + j);
            vector.WriteTo(writer);
            matrix.WriteTo(writer);
            writer.Flush();
            buffer.Seek(0, SeekOrigin.Begin);

            _context.Create<IVector<float>>(reader).ToArray().Should().BeEquivalentTo(vector.ToArray());
            _context.Create<IMatrix<float>>(reader).ToArray().Should().BeEquivalentTo(matrix.ToArray());
        }

        [Fact]
        public void ReadOnlyTensorSerialisation()
        {
            using var buffer = new MemoryStream();
            using var writer = new BinaryWriter(buffer, Encoding.UTF8, true);
            using var reader = new BinaryReader(buffer, Encoding.UTF8, true);

            var vector = _context.CreateReadOnlyVector(1f, 2f, 3f);
            var matrix = _context.CreateReadOnlyMatrix(2, 2, (i, j) => i + j);
            vector.WriteTo(writer);
            matrix.WriteTo(writer);
            writer.Flush();
            buffer.Seek(0, SeekOrigin.Begin);

            _context.Create<IVector<float>>(reader).ToArray().Should().BeEquivalentTo(vector.ToArray());
            _context.Create<IMatrix<float>>(reader).ToArray().Should().BeEquivalentTo(matrix.ToArray());
        }

        [Fact]
        public void ArrayOfArraySerialisation()
        {
            using var buffer = new MemoryStream();
            using var writer = new BinaryWriter(buffer, Encoding.UTF8, true);
            using var reader = new BinaryReader(buffer, Encoding.UTF8, true);
            var array = new[] {
                [1, 2, 3],
                new[] { 4, 5, 6 },
            };

            array.WriteTo(writer);
            writer.Flush();
            buffer.Seek(0, SeekOrigin.Begin);

            var array2 = reader.ReadArrayOfArrays<int>();
            array2.Should().BeEquivalentTo(array);
        }
    }
}
