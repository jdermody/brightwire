using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Output
{
    public class TableSplit
    {
        public IDataTable Training { get; private set; }
        public IDataTable Test { get; private set; }

        internal TableSplit(IDataTable training, IDataTable test)
        {
            Training = training;
            Test = test;
        }
    }
}
