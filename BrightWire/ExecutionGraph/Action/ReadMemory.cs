namespace BrightWire.ExecutionGraph.Action
{
    internal class ReadMemory : IAction
	{
		string _id;

		public ReadMemory(string id)
		{
			_id = id;
		}

		public IGraphData Execute(IGraphData input, IGraphSequenceContext context)
		{
			var memory = context.ExecutionContext.GetMemory(_id);
			return memory.AsGraphData();
		}

		public void Initialise(string data) => _id = data;

        public string Serialise() => _id;
    }
}
