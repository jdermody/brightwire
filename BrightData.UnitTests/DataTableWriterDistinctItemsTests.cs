using System.IO;
using System.Threading.Tasks;
using BrightData.Buffer.Composite;
using BrightData.DataTable;
using BrightData.Types;
using AwesomeAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    /// <summary>
    /// Regression tests for ColumnOrientedDataTableWriter.Convert, which drops the encoded
    /// index data (never calls stream.Write) when the buffer reports IHaveDistinctItemCount.
    /// </summary>
    public class DataTableWriterDistinctItemsTests
    {
        readonly BrightDataContext _context = new();

        [Fact]
        public async Task RoundTrip_StringColumnWithDistinctItems_FollowedByIntColumn()
        {
            // a string buffer with distinct items tracking enabled routes the writer's
            // Convert method through the IHaveDistinctItemCount branch
            var strings = new StringCompositeBuffer(maxDistinctItems: 64);
            strings.Append("apple");
            strings.Append("banana");
            strings.Append("apple");
            strings.Append("cherry");
            strings.Append("banana");
            strings.MetaData.SetType(BrightDataType.String);

            var ints = new UnmanagedCompositeBuffer<int>();
            ints.Append(1);
            ints.Append(2);
            ints.Append(3);
            ints.Append(4);
            ints.Append(5);
            ints.MetaData.SetType(BrightDataType.Int);

            var writer = new ColumnOrientedDataTableWriter();
            using var stream = new MemoryStream();
            await writer.Write(new MetaData(), [strings, ints], stream);

            stream.Seek(0, SeekOrigin.Begin);
            using var table = await _context.LoadTableFromStream(stream);

            var stringColumn = await table.GetColumn<string>(0).ToArray();
            stringColumn.Should().ContainInOrder("apple", "banana", "apple", "cherry", "banana");

            var intColumn = await table.GetColumn<int>(1).ToArray();
            intColumn.Should().ContainInOrder(1, 2, 3, 4, 5);
        }

        [Fact]
        public async Task RoundTrip_BuilderStringColumnWithMaxDistinctItems()
        {
            // builder-wide distinct tracking enables the writer's dedup branch through the normal builder flow
            var builder = _context.CreateTableBuilder(maxDistinctItems: 64);
            var stringColumn = builder.CreateColumn<string>("string column");
            stringColumn.Append("apple");
            stringColumn.Append("banana");
            stringColumn.Append("apple");
            stringColumn.Append("cherry");
            stringColumn.Append("banana");

            var intColumn = builder.CreateColumn<int>("int column");
            intColumn.Append(1);
            intColumn.Append(2);
            intColumn.Append(3);
            intColumn.Append(4);
            intColumn.Append(5);

            using var stream = new MemoryStream();
            await builder.WriteTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            using var table = await _context.LoadTableFromStream(stream);

            var stringColumnData = await table.GetColumn<string>(0).ToArray();
            stringColumnData.Should().ContainInOrder("apple", "banana", "apple", "cherry", "banana");

            var intColumnData = await table.GetColumn<int>(1).ToArray();
            intColumnData.Should().ContainInOrder(1, 2, 3, 4, 5);
        }

        [Fact]
        public async Task RoundTrip_SingleStringColumnWithDistinctItems()
        {
            var strings = new StringCompositeBuffer(maxDistinctItems: 64);
            strings.Append("apple");
            strings.Append("banana");
            strings.Append("apple");
            strings.Append("cherry");
            strings.Append("banana");
            strings.MetaData.SetType(BrightDataType.String);

            var writer = new ColumnOrientedDataTableWriter();
            using var stream = new MemoryStream();
            await writer.Write(new MetaData(), [strings], stream);

            stream.Seek(0, SeekOrigin.Begin);
            using var table = await _context.LoadTableFromStream(stream);

            var stringColumn = await table.GetColumn<string>(0).ToArray();
            stringColumn.Should().ContainInOrder("apple", "banana", "apple", "cherry", "banana");
        }
    }
}
