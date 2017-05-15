using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Input
{
    class OneToMany : NodeBase
    {
        class Backpropagation : BackpropagationBase<OneToMany>
        {
            readonly Dictionary<INode, IGraphData> _signalTable;

            public Backpropagation(OneToMany source) : base(source)
            {
                _signalTable = _source._children.ToDictionary(n => n, n => (IGraphData)null);
            }

            public override void _Backward(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                Debug.Assert(_source._children.Contains(fromNode));
                _signalTable[fromNode] = errorSignal;
                if(_signalTable.All(s => s.Value != null) && parents?.Any() == true) {
                    var firstSignal = _signalTable[_source._children.First()];
                    foreach (var parent in parents)
                        context.AddBackward(firstSignal, parent, _source);
                    _source._onBackpropagation(_signalTable);
                }
            }
        }
        readonly IReadOnlyList<INode> _children;
        readonly Action<IReadOnlyDictionary<INode, IGraphData>> _onBackpropagation;

        public OneToMany(IEnumerable<INode> children, Action<IReadOnlyDictionary<INode, IGraphData>> onBackpropagation, string name = null) : base(name)
        {
            _children = children.ToList();
            _onBackpropagation = onBackpropagation;
            foreach (var child in _children)
                Output.Add(new WireToNode(child));
        }

        public override void ExecuteForward(IContext context)
        {
            _AddNextGraphAction(context, context.Data, () => new Backpropagation(this));
        }
    }
}
