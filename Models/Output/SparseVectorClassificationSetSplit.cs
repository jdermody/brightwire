using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BrightWire.Models.Input;

namespace BrightWire.Models.Output
{
    public class SparseVectorClassificationSetSplit
    {
        public SparseVectorClassificationSet Training { get; private set; }
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
