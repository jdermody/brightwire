using System;

namespace BrightWire.ExecutionGraph.Node.Output
{
    internal class StoreOutput(int channel, string? name = null) : NodeBase(name)
    {
        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel1, IGraphContext context, NodeBase? source)
        {
            var data = context.Data;
            context.SetOutput(data, channel);
            return (this, data, null);
        }
    }
}
