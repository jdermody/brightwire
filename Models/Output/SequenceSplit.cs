using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Output
{
    /// <summary>
    /// A sequence that has been split into training and test components
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SequenceSplit<T>
    {
        /// <summary>
        /// The training data components
        /// </summary>
        public IReadOnlyList<T> Training { get; private set; }

        /// <summary>
        /// The test data components
        /// </summary>
        public IReadOnlyList<T> Test { get; private set; }

        internal SequenceSplit(IReadOnlyList<T> training, IReadOnlyList<T> test)
        {
            Training = training;
            Test = test;
        }
    }
}
