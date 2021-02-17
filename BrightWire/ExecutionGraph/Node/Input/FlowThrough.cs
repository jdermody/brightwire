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

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphSequenceContext context, NodeBase? source)
        {
            return (this, signal, null);
        }
    }
}
