using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class InputTests
    {
        readonly BrightDataContext _context = new();

        [Fact]
        public async Task SimpleCsv()
        {
            var csv = "123,234,456";
            var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(csv)));

            var table = await _context.ParseCsv(reader, false);
            table.RowCount.Should().Be(1);
        }

        [Fact]
        public async Task CsvWithHeader()
        {
            var csv = @"V1,V2,V3
123,234,456";
            var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(csv)));

            var table = await _context.ParseCsv(reader, true);
            table.RowCount.Should().Be(1);
            table.ColumnMetaData[0].GetName().Should().Be("V1");
            table.ColumnMetaData[1].GetName().Should().Be("V2");
            table.ColumnMetaData[2].GetName().Should().Be("V3");
        }
    }
}
