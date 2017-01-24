using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Output
{
    /// <summary>
    /// A converted data table in which all rows whose target value is of type Classification are true (or false if the row is not of type Classification)
    /// </summary>
    public class BinaryTableClassification
    {
        /// <summary>
        /// The classification
        /// </summary>
        public string Classification { get; private set; }

        /// <summary>
        /// The converted table
        /// </summary>
        public IDataTable Table { get; private set; }

        internal BinaryTableClassification(string classification, IDataTable dataTable)
        {
            Classification = classification;
            Table = dataTable;
        }
    }
}
