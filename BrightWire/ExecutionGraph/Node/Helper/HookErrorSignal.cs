using System;
using System.Collections.Generic;

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

            public override IEnumerable<(IGraphData Signal, IGraphSequenceContext Context, NodeBase ToNode)> Backward(IGraphData errorSignal, IGraphSequenceContext context, NodeBase[] parents)
            {
                _source._tryRestore(context);
                return ErrorTo(errorSignal, context, parents);
            }
        }

        readonly Action<IGraphSequenceContext> _tryRestore;

        public HookErrorSignal(Action<IGraphSequenceContext> tryRestore, string? name = null) : base(name)
        {
            _tryRestore = tryRestore;
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardInternal(IGraphData signal, uint channel, IGraphSequenceContext context, NodeBase? source)
        {
            return (this, signal, () => new Backpropagation(this));
        }
    }
}
