using BrightWire.ExecutionGraph.Node;

namespace BrightWire.ExecutionGraph.Action
{
    /// <summary>
    /// Saves the current graph signal into named memory
    /// </summary>
    internal class SetMemory(string id, string? contextName) : IAction
    {
        string _id = id;

        public void Initialise(string data)
        {
            _id = data;
        }

        public string Serialise() => _id;

        public IGraphData Execute(IGraphData input, IGraphContext context, NodeBase node)
        {
            if(contextName != null)
                context.SetData(contextName, "hidden-forward", input);
            context.ExecutionContext.SetMemory(_id, input.GetMatrix());
            return input;
        }
    }
}
