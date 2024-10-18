using BrightWire.ExecutionGraph.Node;

namespace BrightWire.ExecutionGraph.Action
{
    internal class ReadMemory(string id) : IAction
    {
        string _id = id;

        public IGraphData Execute(IGraphData input, IGraphContext context, NodeBase node)
		{
			var memory = context.ExecutionContext.GetMemory(_id);
			return memory.AsGraphData();
		}

		public void Initialise(string data) => _id = data;

        public string Serialise() => _id;
    }
}
