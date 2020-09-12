using System;
using BrightTable.Builders;
using Xunit;
using FluentAssertions;

namespace BrightTable.UnitTests
{
    public class BuilderTests : UnitTestBase
    {
        [Fact]
        public void BuildSimple()
        {
            var builder = _context.BuildTable();
            builder.AddColumn(ColumnType.Int, "int");
            builder.AddColumn(ColumnType.String, "str");
            builder.AddRow(6, "test");
            builder.AddRow(7, "test 2");
            var table = builder.Build();
            table.ColumnTypes[0].Should().Be(ColumnType.Int);
        }
    }
}
