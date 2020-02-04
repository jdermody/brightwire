using System;
using System.Collections.Generic;
using System.Text;

namespace BrightML.Graph
{
    public class Wire
    {
        public Wire(INode destination, uint channel)
        {
            Destination = destination;
            Channel = channel;
        }

        public INode Destination { get; }
        public uint Channel { get; }
    }
}
