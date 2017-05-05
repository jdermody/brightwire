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

        public WireToNode(INode node) { _node = node; }

        public INode SendTo => _node;
        public bool IsPrimary => true;
    }
}
