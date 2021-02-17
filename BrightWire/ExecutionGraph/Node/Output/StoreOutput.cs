using System;

namespace BrightWire.ExecutionGraph.Node.Output
{
    internal class StoreOutput : NodeBase
	{
		readonly int _channel;

		public StoreOutput(int channel, string? name = null) : base(name)
		{
			_channel = channel;
		}

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphSequenceContext context, NodeBase? source)
        {
            var data = context.Data;
            context.SetOutput(data, _channel);
            return (this, data, null);
        }
    }
}
