using BrightWire.ExecutionGraph.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Helper
{
    abstract class SingleBackpropagationBase<T> : BackpropagationBase<T>
        where T : INode
    {
        protected SingleBackpropagationBase(T source) : base(source) { }

        public override void _Backward(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
        {
            IGraphData nextError = null;
            if(errorSignal != null)
                nextError = _Backpropagate(fromNode, errorSignal, context, parents);

            _SendErrorTo(nextError, context, parents);
        }

        protected abstract IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents);
    }
}
