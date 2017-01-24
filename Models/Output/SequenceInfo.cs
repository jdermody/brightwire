using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Output
{
    /// <summary>
    /// Meta data about a sequence
    /// </summary>
    public class SequenceInfo
    {
        /// <summary>
        /// The size of the sequence
        /// </summary>
        public int SequenceLength { get; private set; }

        /// <summary>
        /// The number of samples of this sequence size
        /// </summary>
        public int SampleCount { get; private set; }

        internal SequenceInfo(int sequenceLength, int sampleCount)
        {
            SequenceLength = sequenceLength;
            SampleCount = sampleCount;
        }
    }
}
