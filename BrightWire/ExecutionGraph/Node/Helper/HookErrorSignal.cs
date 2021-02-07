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

            public override void BackwardInternal(INode? fromNode, IGraphData errorSignal, IGraphSequenceContext context, INode[] parents)
            {
                _source._tryRestore(context);
                SendErrorTo(errorSignal, context, parents);
            }
        }

        readonly Action<IGraphSequenceContext> _tryRestore;

        public HookErrorSignal(Action<IGraphSequenceContext> tryRestore, string? name = null) : base(name)
        {
            _tryRestore = tryRestore;
        }

        public override void ExecuteForward(IGraphSequenceContext context)
        {
            AddNextGraphAction(context, context.Data, () => new Backpropagation(this));
        }
    }
}
