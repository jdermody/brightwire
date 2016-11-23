using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Output
{
    public class BidirectionalMemory
    {
        public float[] ForwardMemory { get; private set; }
        public float[] BackwardMemory { get; private set; }

        internal BidirectionalMemory(float[] forward, float[] backward)
        {
            ForwardMemory = forward;
            BackwardMemory = backward;
        }
    }
}
