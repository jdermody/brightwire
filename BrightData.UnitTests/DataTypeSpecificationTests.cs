using BrightData.UnitTests.Helper;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class DataTypeSpecificationTests : UnitTestBase
    {
        [Fact]
        public void FilterDataTable()
        {
            var builder = _context.CreateTableBuilder();
            builder.CreateColumn(BrightDataType.String, "str");
            builder.AddRow("str1");
            builder.AddRow("str2");
            var table = builder.BuildInMemory();

            var typeInfo = table.GetTypeSpecification();
            var stringType = (IDataTypeSpecification<string>)typeInfo.Children![0];
            stringType.AddPredicate(s => s == "str1");

            var badRows = typeInfo.FindNonConformingRows(table);
            badRows.Should().ContainSingle(v => v == 1);
        }
    }
}
