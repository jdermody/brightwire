using BrightData.UnitTests.Helper;
using System;
using System.IO;
using System.Threading.Tasks;
using BrightData.Parquet;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class ParquetTests : UnitTestBase
    {
        [Fact]
        public async Task WriteAndReadParquet()
        {
            var builder = _context.CreateTableBuilder();
            builder.CreateColumn(BrightDataType.Boolean, "boolean");
            builder.CreateColumn(BrightDataType.SByte, "byte");
            builder.CreateColumn(BrightDataType.Date, "date");
            builder.CreateColumn(BrightDataType.Double, "double");
            builder.CreateColumn(BrightDataType.Float, "float");
            builder.CreateColumn(BrightDataType.Int, "int");
            builder.CreateColumn(BrightDataType.Long, "long");
            builder.CreateColumn(BrightDataType.String, "string");

            var now = DateTime.UtcNow;
            builder.AddRow(true, (sbyte)100, now, 1.0 / 3, 0.5f, int.MaxValue, long.MaxValue, "test");
            var dataTable = await builder.BuildInMemory();
            dataTable.MetaData.Set("Testing", "Just a test");

            using var memoryStream = new MemoryStream();
            await dataTable.WriteAsParquet(memoryStream);

            var dataTable2 = await _context.CreateTableFromParquet(memoryStream, null);
            dataTable2.MetaData.Get("Testing", string.Empty).Should().Be("Just a test");
            dataTable2.RowCount.Should().Be(dataTable.RowCount);
            dataTable2.ColumnCount.Should().Be(dataTable.ColumnCount);
            for (var i = 0; i < dataTable.ColumnCount; i++)
                dataTable2.ColumnTypes[i].Should().Be(dataTable.ColumnTypes[i]);

            var row1 = dataTable[0];
            var row2 = dataTable2[0];

            row1.Get<bool>(0).Should().Be(row2.Get<bool>(0));
            row1.Get<sbyte>(1).Should().Be(row2.Get<sbyte>(1));
            row1.Get<DateTime>(2).Should().Be(row2.Get<DateTime>(2));
            row1.Get<double>(3).Should().Be(row2.Get<double>(3));
            row1.Get<float>(4).Should().Be(row2.Get<float>(4));
            row1.Get<int>(4).Should().Be(row2.Get<int>(4));
            row1.Get<long>(4).Should().Be(row2.Get<long>(4));
            row1.Get<string>(4).Should().Be(row2.Get<string>(4));
        }
    }
}
