using BrightData.UnitTests.Helper;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class DataTypeSpecificationTests : UnitTestBase
    {
        [Fact]
        public void FilterColumnOrientedDataTable()
        {
            var builder = _context.BuildTable();
            builder.AddColumn(BrightDataType.String, "str");
            builder.AddRow("str1");
            builder.AddRow("str2");
            var table = builder.BuildInMemory();

            var typeInfo = table.GetTypeSpecification();
            var stringType = (IDataTypeSpecification<string>)typeInfo.Children![0];
            stringType.AddPredicate(s => s == "str1");

            var badRows = typeInfo.FindNonConformingRows(table);
            badRows.Should().ContainSingle(v => v == 1);
        }

        [Fact]
        public void FilterRowOrientedDataTable()
        {
            var builder = _context.BuildTable();
            builder.AddColumn(BrightDataType.String, "str");
            builder.AddRow("str1");
            builder.AddRow("str2");
            var table = builder.BuildInMemory();

            var typeInfo = table.GetTypeSpecification();
            var stringType = (IDataTypeSpecification<string>)typeInfo.Children![0];
            stringType.AddPredicate(s => s == "str1");

            var badRows = typeInfo.FindNonConformingRows(table);
            badRows.Should().ContainSingle(v => v == 1);
        }

        [Fact]
        public void TestDocument()
        {
            
        }
    }
}
