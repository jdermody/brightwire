using BrightWire.ExecutionGraph.Node;

namespace BrightWire.ExecutionGraph.Action
{
    /// <summary>
    /// Saves the current graph signal into named memory
    /// </summary>
    internal class SetMemory(string id, string? contextName) : IAction
    {
        public void Initialise(string data)
        {
            id = data;
        }

        public string Serialise() => id;

        public IGraphData Execute(IGraphData input, IGraphContext context, NodeBase node)
        {
            if(contextName != null)
                context.SetData(contextName, "hidden-forward", input);
            context.ExecutionContext.SetMemory(id, input.GetMatrix());
            return input;
        }
    }
}
