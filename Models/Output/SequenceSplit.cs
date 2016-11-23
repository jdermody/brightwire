using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Output
{
    public class SequenceSplit<T>
    {
        public IReadOnlyList<T> Training { get; private set; }
        public IReadOnlyList<T> Test { get; private set; }

        internal SequenceSplit(IReadOnlyList<T> training, IReadOnlyList<T> test)
        {
            Training = training;
            Test = test;
        }
    }
}
