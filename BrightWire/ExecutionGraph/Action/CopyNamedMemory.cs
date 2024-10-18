using System;
using System.Text;
using BrightWire.ExecutionGraph.Node;

namespace BrightWire.ExecutionGraph.Action
{
    /// <summary>
    /// Copies named memory from one slot to another
    /// </summary>
    internal class CopyNamedMemory(string slotName, IHaveMemoryNode node) : IAction
    {
        string _readFrom = node.Memory.Id;
        string _slotName = slotName;

        public IGraphData Execute(IGraphData input, IGraphContext context, NodeBase node)
        {
            var ec = context.ExecutionContext;
            var memory = ec.GetMemory(_readFrom);
            ec.SetMemory(_slotName, memory);
            return input;
        }

        public void Initialise(string data)
        {
            var str = Encoding.UTF8.GetString(Convert.FromBase64String(data));
            var pos = str.IndexOf(':');
            _readFrom = str[..pos];
            _slotName = str[(pos + 1)..];
        }

        public string Serialise()
        {
            var concat = _readFrom + ":" + _slotName;
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(concat));
        }
    }
}
