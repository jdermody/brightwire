using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class DataTableBuilderTests : UnitTestBase
    {
        [Fact]
        public void BuildSimple()
        {
            var builder = _context.BuildTable();
            builder.AddColumn(ColumnType.Int, "int");
            builder.AddColumn(ColumnType.String, "str");
            builder.AddRow(6, "test");
            builder.AddRow(7, "test 2");
            var table = builder.BuildRowOriented();
            table.ColumnTypes[0].Should().Be(ColumnType.Int);
        }
    }
}
