using System;

namespace BrightWire.ExecutionGraph.Node.Helper
{
    /// <summary>
    /// Provides a hook into the backpropagation signal
    /// </summary>
    internal class HookErrorSignal : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<HookErrorSignal>
        {
            public Backpropagation(HookErrorSignal source) : base(source)
            {
            }

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                _source._tryRestore(context);
                return errorSignal;
            }
        }

        readonly Action<IGraphContext> _tryRestore;

        public HookErrorSignal(Action<IGraphContext> tryRestore, string? name = null) : base(name)
        {
            _tryRestore = tryRestore;
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            return (this, signal, () => new Backpropagation(this));
        }
    }
}
