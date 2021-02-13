using System;

namespace BrightWire.ExecutionGraph.Node.Input
{
    /// <summary>
    /// Simple pass through of the input signal
    /// </summary>
    internal class FlowThrough : NodeBase
    {
        public FlowThrough() : base(null)
        {
        }

        public override void ExecuteForward(IGraphSequenceContext context)
        {
            AddNextGraphAction(context, context.Data, null);
        }

        public override (INode FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) Forward(IGraphData signal, uint channel, IGraphSequenceContext context, INode? source)
        {
            return (this, signal, null);
        }
    }
}
