using System.IO;
using System.Threading.Tasks;
using BrightData.Helper;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class CsvParserTests
    {
        [Fact]
        public async Task Parse_WhenValidCsvString_ReturnsExpectedResult()
        {
            // Arrange
            var csvParser = new CsvParser(true, ',');
            var csvString = "Name,Age\nJohn,30\nAlice,25";

            // Act
            var result = csvParser.Parse(csvString);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);

            var firstColumn = result![0];
            firstColumn.MetaData.GetName().Should().Be("Name");
            var firstColumnData = await firstColumn.ToArray();
            firstColumnData.Should().NotBeNull();
            firstColumnData.Should().HaveCount(2);
            firstColumnData[0].Should().Be("John");
            firstColumnData[1].Should().Be("Alice");

            var secondColumn = result[1];
            secondColumn.MetaData.GetName().Should().Be("Age");
            var secondColumnData = await secondColumn.ToArray();
            secondColumnData.Should().NotBeNull();
            secondColumnData.Should().HaveCount(2);
            secondColumnData[0].Should().Be("30");
            secondColumnData[1].Should().Be("25");
        }

        [Fact]
        public async Task ParseAsync_WhenValidCsvStream_ReturnsExpectedResult()
        {
            // Arrange
            var csvParser = new CsvParser(false, ',');
            var csvStream = new MemoryStream();
            var writer = new StreamWriter(csvStream);
            await writer.WriteAsync("Name,Age\nJohn,30\nAlice,25");
            await writer.FlushAsync();
            csvStream.Position = 0;

            // Act
            var result = await csvParser.Parse(new StreamReader(csvStream));

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);

            var firstColumn = result![0];
            var firstColumnData = await firstColumn.ToArray();
            firstColumnData.Should().NotBeNull();
            firstColumnData.Should().HaveCount(3);
            firstColumnData[0].Should().Be("Name");
            firstColumnData[1].Should().Be("John");
            firstColumnData[2].Should().Be("Alice");

            var secondColumn = result[1];
            var secondColumnData = await secondColumn.ToArray();
            secondColumnData.Should().NotBeNull();
            secondColumnData.Should().HaveCount(3);
            secondColumnData[0].Should().Be("Age");
            secondColumnData[1].Should().Be("30");
            secondColumnData[2].Should().Be("25");
        }
    }
}
