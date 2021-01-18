using System;

namespace BrightWire.ExecutionGraph.Node.Helper
{
    /// <summary>
    /// Provides a hook into the backpropagation signal
    /// </summary>
    internal class HookErrorSignal : NodeBase
    {
        class Backpropagation : BackpropagationBase<HookErrorSignal>
        {
            public Backpropagation(HookErrorSignal source) : base(source)
            {
            }

            public override void _Backward(INode fromNode, IGraphData errorSignal, IGraphContext context, INode[] parents)
            {
                _source._tryRestore(context);
                _SendErrorTo(errorSignal, context, parents);
            }
        }

        readonly Action<IGraphContext> _tryRestore;

        public HookErrorSignal(Action<IGraphContext> tryRestore, string name = null) : base(name)
        {
            _tryRestore = tryRestore;
        }

        public override void ExecuteForward(IGraphContext context)
        {
            _AddNextGraphAction(context, context.Data, () => new Backpropagation(this));
        }
    }
}
