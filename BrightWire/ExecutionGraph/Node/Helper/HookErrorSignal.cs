using System;
using System.Collections.Generic;

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

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphSequenceContext context)
            {
                _source._tryRestore(context);
                return errorSignal;
            }
        }

        readonly Action<IGraphSequenceContext> _tryRestore;

        public HookErrorSignal(Action<IGraphSequenceContext> tryRestore, string? name = null) : base(name)
        {
            _tryRestore = tryRestore;
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphSequenceContext context, NodeBase? source)
        {
            return (this, signal, () => new Backpropagation(this));
        }
    }
}
