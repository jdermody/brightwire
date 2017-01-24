using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Output
{
    /// <summary>
    /// A table that has been split into training and test tables
    /// </summary>
    public class TableSplit
    {
        /// <summary>
        /// The training data table
        /// </summary>
        public IDataTable Training { get; private set; }

        /// <summary>
        /// The test data table
        /// </summary>
        public IDataTable Test { get; private set; }

        internal TableSplit(IDataTable training, IDataTable test)
        {
            Training = training;
            Test = test;
        }
    }
}
