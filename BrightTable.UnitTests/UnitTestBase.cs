using System;
using System.Collections.Generic;
using System.Text;
using BrightData;

namespace BrightTable.UnitTests
{
    public class UnitTestBase
    {
        protected readonly BrightDataContext _context = new BrightDataContext(0);
    }
}
