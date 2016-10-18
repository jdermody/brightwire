using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Simple
{
    public class SequenceInfo
    {
        public int SequenceLength { get; private set; }
        public int SampleCount { get; private set; }

        public SequenceInfo(int sequenceLength, int sampleCount)
        {
            SequenceLength = sequenceLength;
            SampleCount = sampleCount;
        }
    }
}
