using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Output
{
    public class TableFold
    {
        public IDataTable Training { get; private set; }
        public IDataTable Validation { get; private set; }

        internal TableFold(IDataTable training, IDataTable validation)
        {
            Training = training;
            Validation = validation;
        }
    }
}
