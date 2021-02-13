﻿namespace BrightWire.ExecutionGraph.Node.Action
{
    /// <summary>
    /// Saves the current graph signal into named memory
    /// </summary>
    internal class SetMemory : IAction
    {
        string _id;

        public SetMemory(string id)
        {
            _id = id;
        }

        public void Initialise(string data)
        {
            _id = data;
        }

        public string Serialise() => _id;

        public IGraphData Execute(IGraphData input, IGraphSequenceContext context, NodeBase node)
        {
            context.ExecutionContext.SetMemory(_id, input.GetMatrix());
            return input;
        }
    }
}
