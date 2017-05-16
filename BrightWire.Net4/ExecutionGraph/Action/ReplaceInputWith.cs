using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Action
{
    public class ReplaceInputWith : IAction
    {
        readonly string _name;

        public ReplaceInputWith(string name)
        {
            _name = name;
        }

        public IGraphData Execute(IGraphData input, IContext context)
        {
            var curr = context.ExecutionContext.GetMemory(_name);
            if (curr != null && curr.RowCount == input.GetMatrix().RowCount) {
                context.ExecutionContext.SetMemory(_name, null);
                return curr.ToGraphData();
            }
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
