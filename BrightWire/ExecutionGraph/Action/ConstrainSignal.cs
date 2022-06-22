using BrightWire.ExecutionGraph.Node;

namespace BrightWire.ExecutionGraph.Action
{
    /// <summary>
    /// Constrains the signal through the graph to lie between two values
    /// </summary>
    internal class ConstrainSignal : IAction
    {
        float _min, _max;

        public ConstrainSignal(float min = -1f, float max = 1f)
        {
            _min = min;
            _max = max;
        }

        public IGraphData Execute(IGraphData input, IGraphSequenceContext context, NodeBase node)
        {
            var matrix = input.GetMatrix();
            matrix.ConstrainInPlace(_min, _max);
            return input;
        }

        public void Initialise(string data)
        {
            var pos = data.IndexOf(':');
            _min = float.Parse(data[..pos]);
            _max = float.Parse(data[(pos + 1)..]);
        }

        public string Serialise() => $"{_min}:{_max}";
    }
}
