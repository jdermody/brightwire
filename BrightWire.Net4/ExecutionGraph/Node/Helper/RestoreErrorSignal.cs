using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Helper
{
    internal class RestoreErrorSignal : NodeBase
    {
        class Backpropagation : BackpropagationBase<RestoreErrorSignal>
        {
            public Backpropagation(RestoreErrorSignal source) : base(source)
            {
            }

            public override void _Backward(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                _source._tryRestore(context);
                _SendErrorTo(errorSignal, context, parents);
            }
        }

        readonly Action<IContext> _tryRestore;

        public RestoreErrorSignal(Action<IContext> tryRestore, string name = null) : base(name)
        {
            _tryRestore = tryRestore;
        }

        public override void ExecuteForward(IContext context)
        {
            _AddNextGraphAction(context, context.Data, () => new Backpropagation(this));
        }
    }
}
