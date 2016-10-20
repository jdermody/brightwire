using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Simple
{
    public class BinaryClassification
    {
        public string Classification { get; private set; }
        public IDataTable Table { get; private set; }

        public BinaryClassification(string classification, IDataTable dataTable)
        {
            Classification = classification;
            Table = dataTable;
        }
    }
}
