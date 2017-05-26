using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Action
{
    internal class WriteNodeMemoryToSlot : IAction
    {
        string _writeTo;
        string _readFrom;

        public WriteNodeMemoryToSlot(string slotName, IHaveMemoryNode node)
        {
            _writeTo = slotName;
            _readFrom = node.Memory.Id;
        }

        public IGraphData Execute(IGraphData input, IContext context)
        {
            var ec = context.ExecutionContext;
            var memory = ec.GetMemory(_readFrom);
            ec.SetMemory(_writeTo, memory);
            return input;
        }

        public void Initialise(string data)
        {
            var str = Encoding.UTF8.GetString(Convert.FromBase64String(data));
            var pos = str.IndexOf(':');
            _readFrom = str.Substring(0, pos);
            _writeTo = str.Substring(pos + 1);
        }

        public string Serialise()
        {
            var concat = _readFrom + ":" + _writeTo;
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(concat));
        }
    }
}
