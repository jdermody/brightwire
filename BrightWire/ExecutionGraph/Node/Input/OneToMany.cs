using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BrightWire.ExecutionGraph.Node.Input
{
    /// <summary>
    /// Waits for all children to finish backpropagating before sending the error further backward
    /// </summary>
    sealed class OneToMany : NodeBase
    {
        class Backpropagation : BackpropagationBase<OneToMany>
        {
            readonly Dictionary<INode, IGraphData> _signalTable;

            public Backpropagation(OneToMany source) : base(source)
            {
                _signalTable = _source._children.ToDictionary(n => n, n => (IGraphData)null);
            }

            public override void _Backward(INode fromNode, IGraphData errorSignal, IContext context, INode[] parents)
            {
                Debug.Assert(_source._children.Contains(fromNode));
                _signalTable[fromNode] = errorSignal;
                if(_signalTable.All(s => s.Value != null) && parents?.Any() == true) {
                    var firstSignal = _signalTable[_source._children.First()];
                    var otherSignals = _signalTable
                        .Where(s => s.Value != firstSignal && s.Value.Columns == firstSignal.Columns && s.Value.Rows == firstSignal.Rows)
                        .ToList()
                    ;
                    if(otherSignals.Any()) {
                        var matrix = firstSignal.GetMatrix();
                        foreach (var item in otherSignals)
                            matrix.AddInPlace(item.Value.GetMatrix());
                        firstSignal = firstSignal.ReplaceWith(matrix);
                    }
                    foreach (var parent in parents)
                        context.AddBackward(firstSignal, parent, _source);
                    _source._onBackpropagation?.Invoke(_signalTable);
                }
            }
        }
        readonly INode[] _children;
        readonly Action<IReadOnlyDictionary<INode, IGraphData>> _onBackpropagation;

        public OneToMany(IEnumerable<INode> children, Action<IReadOnlyDictionary<INode, IGraphData>> onBackpropagation, string name = null) : base(name)
        {
            _children = children.ToArray();
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
