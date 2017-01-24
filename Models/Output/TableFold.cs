using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Output
{
    /// <summary>
    /// A table that has been folded into training and validation tables
    /// </summary>
    public class TableFold
    {
        /// <summary>
        /// The training data table
        /// </summary>
        public IDataTable Training { get; private set; }

        /// <summary>
        /// The validation data table
        /// </summary>
        public IDataTable Validation { get; private set; }

        internal TableFold(IDataTable training, IDataTable validation)
        {
            Training = training;
            Validation = validation;
        }
    }
}
