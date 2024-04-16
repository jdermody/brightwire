using BrightWire.ExecutionGraph.Node;

namespace BrightWire.ExecutionGraph.Action
{
    internal class ReadMemory(string id) : IAction
    {
        public IGraphData Execute(IGraphData input, IGraphContext context, NodeBase node)
		{
			var memory = context.ExecutionContext.GetMemory(id);
			return memory.AsGraphData();
		}

		public void Initialise(string data) => id = data;

        public string Serialise() => id;
    }
}
