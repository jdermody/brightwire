using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Helper
{
    class WireToNode : IWire
    {
        readonly INode _node;
        readonly int _channel;

        public WireToNode(INode node, int channel = 0) { _node = node; _channel = channel; }

        public INode SendTo => _node;
        public int Channel => _channel;
    }
}
