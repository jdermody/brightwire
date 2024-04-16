using System.Threading.Tasks;
using BrightData.UnitTests.Helper;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class DataTableBuilderTests : UnitTestBase
    {
        [Fact]
        public async Task BuildSimple()
        {
            var builder = _context.CreateTableBuilder();
            builder.CreateColumn(BrightDataType.Int, "int");
            builder.CreateColumn(BrightDataType.String, "str");
            builder.AddRow(6, "test");
            builder.AddRow(7, "test 2");
            var table = await builder.BuildInMemory();
            table.ColumnTypes[0].Should().Be(BrightDataType.Int);
        }
    }
}
