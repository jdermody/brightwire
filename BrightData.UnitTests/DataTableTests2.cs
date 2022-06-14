using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.DataTable2.Builders;
using Xunit;

namespace BrightData.UnitTests
{
    public class DataTableTests2
    {
        readonly BrightDataContext _context = new();

        [Fact]
        public void InMemoryColumnOriented()
        {
            var builder = new DataTableBuilder(_context);
            var buffer = builder.AddColumn<string>("string column");

            buffer.Add("");
        }
    }
}
