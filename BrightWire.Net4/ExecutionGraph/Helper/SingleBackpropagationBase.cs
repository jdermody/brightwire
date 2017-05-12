using BrightWire.ExecutionGraph.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Helper
{
    abstract class SingleBackpropagationBase : BackpropagationBase
    {
        public override void Backward(IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
        {
            var ret = _Backward(errorSignal, context, parents);
            if (ret != null && parents?.Any() == true) {
                foreach (var parent in parents)
                    context.AddBackward(ret, parent);
            }
        }

        protected abstract IGraphData _Backward(IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents);
    }
}
