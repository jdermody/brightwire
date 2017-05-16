using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Action
{
    public class ConstrainErrorSignal : IAction
    {
        readonly float _min, _max;

        public ConstrainErrorSignal(float min = -1f, float max = 1f)
        {
            _min = min;
            _max = max;
        }

        public IGraphData Execute(IGraphData input, IContext context)
        {
            var matrix = input.GetMatrix();
            matrix.Constrain(_min, _max);
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
