using System.Collections.Generic;

namespace BrightWire.ExecutionGraph.Helper
{
    /// <summary>
    /// Records a node execution
    /// </summary>
    class TrainingAction : IExecutionHistory
    {
        readonly INode _source;
        readonly IReadOnlyList<INode> _parents;
        readonly IGraphData _data;
        IBackpropagation _backpropagation;

        public TrainingAction(INode source, IGraphData data, INode parent = null)
        {
            if (parent != null)
                _parents = new[] { parent };
            else
                _parents = new List<INode>();

            _source = source;
            _data = data;
        }

        public TrainingAction(INode source, IGraphData data, IReadOnlyList<INode> parents)
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
