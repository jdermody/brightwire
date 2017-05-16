using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Action
{
    public class SetMemory : IAction
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

        public string Serialise()
        {
            return _id;
        }

        public IGraphData Execute(IGraphData input, IContext context)
        {
            context.ExecutionContext.SetMemory(_id, input.GetMatrix());
            return input;
        }
    }
}
