using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BrightWire.Models.Input;

namespace BrightWire.Models.Output
{
    /// <summary>
    /// A sparse vector classification set that has been split into training and test components
    /// </summary>
    public class SparseVectorClassificationSetSplit
    {
        /// <summary>
        /// The training data split
        /// </summary>
        public SparseVectorClassificationSet Training { get; private set; }

        /// <summary>
        /// The test data split
        /// </summary>
        public SparseVectorClassificationSet Test { get; private set; }

        internal SparseVectorClassificationSetSplit(SequenceSplit<SparseVectorClassification> split)
        {
            Training = new SparseVectorClassificationSet {
                Classification = split.Training.ToArray()
            };
            Test = new SparseVectorClassificationSet {
                Classification = split.Test.ToArray()
            };
        }
    }
}
