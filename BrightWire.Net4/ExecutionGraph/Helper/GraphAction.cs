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
        readonly IGraphData _data;
        IBackpropagation _backpropagation;

        public GraphAction(INode source, IGraphData data)
        {
            _source = source;
            _data = data;
        }

        public INode Source => _source;
        public IGraphData Data => _data;
        public IBackpropagation Backpropagation { get => _backpropagation; set => _backpropagation = value; }
    }
}
