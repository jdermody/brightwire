using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Output
{
    /// <summary>
    /// A classified data table row
    /// </summary>
    public class RowClassification
    {
        /// <summary>
        /// The row that was classified
        /// </summary>
        public IRow Row { get; private set; }

        /// <summary>
        /// The associated classification
        /// </summary>
        public string Classification { get; private set; }

        internal RowClassification(IRow row, string classification)
        {
            Row = row;
            Classification = classification;
        }
    }
}
