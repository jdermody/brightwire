using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Output
{
    public class RowClassification
    {
        public IRow Row { get; private set; }
        public string Classification { get; private set; }

        internal RowClassification(IRow row, string classification)
        {
            Row = row;
            Classification = classification;
        }
    }
}
