using BrightWire.ExecutionGraph.Node;

namespace BrightWire.ExecutionGraph.Action
{
    /// <summary>
    /// Saves the current graph signal into named memory
    /// </summary>
    internal class SetMemory : IAction
    {
        string _id;
        readonly string? _contextName;

        public SetMemory(string id, string? contextName)
        {
            _id = id;
            _contextName = contextName;
        }

        public void Initialise(string data)
        {
            _id = data;
        }

        public string Serialise() => _id;

        public IGraphData Execute(IGraphData input, IGraphContext context, NodeBase node)
        {
            if(_contextName != null)
                context.SetData(_contextName, "hidden-forward", input);
            context.ExecutionContext.SetMemory(_id, input.GetMatrix());
            return input;
        }
    }
}
