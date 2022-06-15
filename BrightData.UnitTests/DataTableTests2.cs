using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.DataTable2;
using BrightData.DataTable2.Builders;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class DataTableTests2
    {
        readonly BrightDataContext _context = new();

        [Fact]
        public void InMemoryColumnOriented()
        {
            using var builder = new DataTableBuilder(_context);
            var stringColumnBuilder = builder.AddColumn<string>("string column");
            stringColumnBuilder.Add("a row");
            stringColumnBuilder.Add("another row");

            var intColumnBuilder = builder.AddColumn<int>("int column");
            intColumnBuilder.Add(123);
            intColumnBuilder.Add(234);

            using var stream = new MemoryStream();
            builder.WriteTo(stream);

            stream.Seek(0, SeekOrigin.Begin);
            var dataTable = new BrightDataTable(_context, stream);

            using var stringReader = dataTable.ReadColumn<string>(0);
            stringReader.EnumerateTyped().Count().Should().Be(2);
            stringReader.EnumerateTyped().Count().Should().Be(2);

            using var intReader = dataTable.ReadColumn<int>(1);
            intReader.EnumerateTyped().Count().Should().Be(2);
            intReader.EnumerateTyped().Count().Should().Be(2);
        }
    }
}
