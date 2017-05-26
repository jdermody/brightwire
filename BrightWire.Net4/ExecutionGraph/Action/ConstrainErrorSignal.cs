using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Action
{
    internal class ConstrainErrorSignal : IAction
    {
        float _min, _max;

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
            var pos = data.IndexOf(':');
            _min = float.Parse(data.Substring(0, pos));
            _max = float.Parse(data.Substring(pos + 1));
        }

        public string Serialise()
        {
            return $"{_min}:{_max}";
        }
    }
}
