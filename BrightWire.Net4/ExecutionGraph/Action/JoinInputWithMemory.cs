using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Action
{
    internal class JoinInputWithMemory : IAction
    {
        string _slotName;

        public JoinInputWithMemory(string slotName)
        {
            _slotName = slotName;
        }

        public IGraphData Execute(IGraphData input, IContext context)
        {
            var memory = context.ExecutionContext.GetMemory(_slotName);
            return input.GetMatrix().ConcatRows(memory).ToGraphData();
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
