using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Helper
{
    class GraphAction : IExecutionHistory
    {
        readonly INode _source;
        readonly IReadOnlyList<INode> _parents;
        readonly IGraphData _data;
        IBackpropagation _backpropagation;

        public GraphAction(INode source, IGraphData data, params INode[] parents)
        {
            _parents = parents;
            _source = source;
            _data = data;
        }

        public INode Source => _source;
        public IGraphData Data => _data;
        public IBackpropagation Backpropagation { get => _backpropagation; set => _backpropagation = value; }
        public IReadOnlyList<INode> Parents => _parents;
    }
}
