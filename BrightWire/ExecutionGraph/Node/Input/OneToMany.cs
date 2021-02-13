using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.ExecutionGraph.Node.Input
{
    /// <summary>
    /// Waits for all children to finish backpropagating before sending the error further backward
    /// </summary>
    internal sealed class OneToMany : NodeBase
    {
        //class Backpropagation : BackpropagationBase<OneToMany>
        //{
        //    //readonly Dictionary<INode, IGraphData> _signalTable;

        //    public Backpropagation(OneToMany source) : base(source)
        //    {
        //        //_signalTable = _source._children.ToDictionary(n => n, n => (IGraphData)NullGraphData.Instance);
        //    }

        //    //public override void BackwardInternal(INode? fromNode, IGraphData errorSignal, IGraphSequenceContext context, INode[] parents)
        //    //{
        //    //    if (fromNode == null || !_source._children.Contains(fromNode))
        //    //        throw new Exception("Unknown from node");

        //    //    _signalTable[fromNode] = errorSignal;
        //    //    if(_signalTable.All(s => s.Value.HasValue) && parents.Any()) {
        //    //        var firstSignal = _signalTable[_source._children.First()];
        //    //        var otherSignals = _signalTable
        //    //            .Where(s => s.Value != firstSignal && s.Value.Columns == firstSignal.Columns && s.Value.Rows == firstSignal.Rows)
        //    //            .ToList()
        //    //        ;
        //    //        if(otherSignals.Any()) {
        //    //            var matrix = firstSignal.GetMatrix();
        //    //            foreach (var item in otherSignals)
        //    //                matrix.AddInPlace(item.Value.GetMatrix());
        //    //            firstSignal = firstSignal.ReplaceWith(matrix);
        //    //        }
        //    //        foreach (var parent in parents)
        //    //            context.AddBackward(firstSignal, parent, _source);
        //    //        _source._onBackpropagation?.Invoke(_signalTable);
        //    //    }
        //    //}

        //    public override IEnumerable<(IGraphData Signal, INode ToNode)> Backward(IGraphData errorSignal, IGraphSequenceContext context, INode[] parents)
        //    {
        //        return ErrorTo(errorSignal, parents);
        //    }
        //}
        //readonly INode[] _children;
        //readonly Action<IReadOnlyDictionary<INode, IGraphData>>? _onBackpropagation;

        public OneToMany(IEnumerable<INode> children, Action<IReadOnlyDictionary<INode, IGraphData>>? onBackpropagation = null, string? name = null) : base(name)
        {
            //_children = children.ToArray();
            //_onBackpropagation = onBackpropagation;
            foreach (var child in children)
                Output.Add(new WireToNode(child));
        }

        public override void ExecuteForward(IGraphSequenceContext context)
        {
            AddNextGraphAction(context, context.Data, null/*, () => new Backpropagation(this)*/);
        }

        public override (INode FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) Forward(IGraphData signal, uint channel, IGraphSequenceContext context, INode? source)
        {
            return (this, signal, null/*, () => new Backpropagation(this)*/);
        }
    }
}
