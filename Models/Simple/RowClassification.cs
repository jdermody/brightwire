using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Simple
{
    public class RowClassification
    {
        public IRow Row { get; private set; }
        public string Classification { get; private set; }

        public RowClassification(IRow row, string classification)
        {
            Row = row;
            Classification = classification;
        }
    }
}
