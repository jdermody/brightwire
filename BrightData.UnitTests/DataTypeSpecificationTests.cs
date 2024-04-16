using System.Threading.Tasks;
using BrightData.UnitTests.Helper;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class DataTypeSpecificationTests : UnitTestBase
    {
        [Fact]
        public async Task FilterDataTable()
        {
            var builder = _context.CreateTableBuilder();
            builder.CreateColumn(BrightDataType.String, "str");
            builder.AddRow("str1");
            builder.AddRow("str2");
            var table = await builder.BuildInMemory();

            var typeInfo = table.GetTypeSpecification();
            var stringType = (IDataTypeSpecification<string>)typeInfo.Children![0];
            stringType.AddPredicate(s => s == "str1");

            var badRows = await typeInfo.FindNonConformingRows(table);
            badRows.Should().ContainSingle(v => v == 1);
        }
    }
}
