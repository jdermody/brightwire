using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Simple
{
    public class TableFold
    {
        public IDataTable Training { get; private set; }
        public IDataTable Validation { get; private set; }

        public TableFold(IDataTable training, IDataTable validation)
        {
            Training = training;
            Validation = validation;
        }
    }
}
