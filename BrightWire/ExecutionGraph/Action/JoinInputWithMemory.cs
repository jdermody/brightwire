namespace BrightWire.ExecutionGraph.Action
{
    /// <summary>
    /// Joins the graph signal with a saved signal stored in named memory
    /// </summary>
    internal class JoinInputWithMemory : IAction
    {
        string _slotName;

        public JoinInputWithMemory(string slotName)
        {
            _slotName = slotName;
        }

        public IGraphData Execute(IGraphData input, IGraphContext context)
        {
            var memory = context.ExecutionContext.GetMemory(_slotName);
            return input.ReplaceWith(input.GetMatrix().ConcatRows(memory));
        }

        public void Initialise(string data)
        {
            _slotName = data;
        }

        public string Serialise()
        {
            return _slotName;
        }
    }
}
