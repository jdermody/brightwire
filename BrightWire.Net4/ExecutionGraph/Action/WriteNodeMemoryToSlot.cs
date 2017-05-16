using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Action
{
    public class WriteNodeMemoryToSlot : IAction
    {
        readonly string _writeTo;
        readonly string _readFrom;

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
            throw new NotImplementedException();
        }

        public string Serialise()
        {
            throw new NotImplementedException();
        }
    }
}
