namespace BrightWire.ExecutionGraph.Node.Output
{
	class StoreOutput : NodeBase
	{
		readonly int _channel;

		public StoreOutput(int channel, string name = null) : base(name)
		{
			_channel = channel;
		}

		public override void ExecuteForward(IGraphContext context)
		{
			var data = context.Data;
			context.SetOutput(data, _channel);
			_AddNextGraphAction(context, data, null);
		}
	}
}
