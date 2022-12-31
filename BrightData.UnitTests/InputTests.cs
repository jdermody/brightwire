using System.IO;
using System.Text;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class InputTests
    {
        readonly BrightDataContext _context;

        public InputTests()
        {
            _context = new BrightDataContext();
        }

        [Fact]
        public void SimpleCsv()
        {
            var csv = @"123,234,456";
            var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(csv)));

            var table = _context.ParseCsvIntoMemory(reader, false);
            table.RowCount.Should().Be(1);
        }

        [Fact]
        public void CsvWithHeader()
        {
            var csv = @"V1,V2,V3
123,234,456";
            var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(csv)));

            var table = _context.ParseCsvIntoMemory(reader, true);
            table.RowCount.Should().Be(1);
            table.ColumnMetaData[0].GetName().Should().Be("V1");
            table.ColumnMetaData[1].GetName().Should().Be("V2");
            table.ColumnMetaData[2].GetName().Should().Be("V3");
        }
    }
}
