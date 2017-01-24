using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Output
{
    /// <summary>
    /// Bidirectional neural network initial memory
    /// </summary>
    public class BidirectionalMemory
    {
        /// <summary>
        /// The initial forward memory
        /// </summary>
        public float[] ForwardMemory { get; private set; }

        /// <summary>
        /// The initial backward memory
        /// </summary>
        public float[] BackwardMemory { get; private set; }

        internal BidirectionalMemory(float[] forward, float[] backward)
        {
            ForwardMemory = forward;
            BackwardMemory = backward;
        }
    }
}
