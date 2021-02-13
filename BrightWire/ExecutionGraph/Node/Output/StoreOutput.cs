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

		public override void ExecuteForward(IGraphSequenceContext context)
		{
			var data = context.Data;
			context.SetOutput(data, _channel);
			AddNextGraphAction(context, data, null);
		}

        public override (IGraphData Next, Func<IBackpropagate>? BackProp) Forward(IGraphData signal, uint channel, IGraphSequenceContext context, INode? source)
        {
            var data = context.Data;
            context.SetOutput(data, _channel);
            return (data, null);
        }
    }
}
