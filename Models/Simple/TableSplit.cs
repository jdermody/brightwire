using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Simple
{
    public class TableSplit
    {
        public IDataTable Training { get; private set; }
        public IDataTable Test { get; private set; }

        public TableSplit(IDataTable training, IDataTable test)
        {
            Training = training;
            Test = test;
        }
    }
}
