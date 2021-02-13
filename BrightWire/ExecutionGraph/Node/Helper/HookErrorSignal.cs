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

            public override IEnumerable<(IGraphData Signal, INode ToNode)> Backward(IGraphData errorSignal, IGraphSequenceContext context, INode[] parents)
            {
                _source._tryRestore(context);
                return ErrorTo(errorSignal, parents);
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

        public override (IGraphData Next, Func<IBackpropagate>? BackProp) Forward(IGraphData signal, uint channel, IGraphSequenceContext context, INode? source)
        {
            return (signal, () => new Backpropagation(this));
        }
    }
}
