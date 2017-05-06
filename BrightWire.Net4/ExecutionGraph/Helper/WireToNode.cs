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
        readonly bool _isPrimary;

        public WireToNode(INode node, bool isPrimary = true) { _node = node; _isPrimary = isPrimary; }

        public INode SendTo => _node;
        public bool IsPrimary => _isPrimary;
    }
}
