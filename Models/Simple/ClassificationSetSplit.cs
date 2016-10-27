using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace BrightWire.Models.Simple
{
    public class ClassificationSetSplit
    {
        public WeightedClassificationSet Training { get; private set; }
        public WeightedClassificationSet Test { get; private set; }

        public ClassificationSetSplit(SequenceSplit<WeightedClassificationSet.Classification> split)
        {
            Training = new WeightedClassificationSet {
                Classifications = split.Training.ToArray()
            };
            Test = new WeightedClassificationSet {
                Classifications = split.Test.ToArray()
            };
        }
    }
}
