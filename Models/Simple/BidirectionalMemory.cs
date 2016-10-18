using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Simple
{
    public class BidirectionalMemory
    {
        public float[] ForwardMemory { get; private set; }
        public float[] BackwardMemory { get; private set; }

        public BidirectionalMemory(float[] forward, float[] backward)
        {
            ForwardMemory = forward;
            BackwardMemory = backward;
        }
    }
}
