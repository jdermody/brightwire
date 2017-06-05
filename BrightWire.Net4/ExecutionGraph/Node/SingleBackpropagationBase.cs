using BrightWire.ExecutionGraph.Node;
using System.Collections.Generic;

namespace BrightWire.ExecutionGraph
{
    /// <summary>
    /// Base class for nodes that back propagate to a single parent
    /// </summary>
    /// <typeparam name="T">The node type</typeparam>
    abstract class SingleBackpropagationBase<T> : BackpropagationBase<T>
        where T : INode
    {
        protected SingleBackpropagationBase(T source) : base(source) { }

        public override void _Backward(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
        {
            IGraphData nextError = null;
            if(errorSignal != null)
                nextError = _Backpropagate(fromNode, errorSignal, context, parents);
            if(nextError != null)
            _SendErrorTo(nextError, context, parents);
        }

        protected abstract IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents);
    }
}
