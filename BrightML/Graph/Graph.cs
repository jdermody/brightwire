using System;
using System.Collections.Generic;
using System.Text;

namespace BrightML.Graph
{
    class Graph
    {
        readonly List<INode> _inputNodes = new List<INode>();

        public IEnumerable<INode> Input => _inputNodes;
    }
}
