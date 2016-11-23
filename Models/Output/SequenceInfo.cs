using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Output
{
    public class SequenceInfo
    {
        public int SequenceLength { get; private set; }
        public int SampleCount { get; private set; }

        internal SequenceInfo(int sequenceLength, int sampleCount)
        {
            SequenceLength = sequenceLength;
            SampleCount = sampleCount;
        }
    }
}
