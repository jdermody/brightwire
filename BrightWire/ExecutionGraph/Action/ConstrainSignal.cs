using BrightWire.ExecutionGraph.Node;

namespace BrightWire.ExecutionGraph.Action
{
    /// <summary>
    /// Constrains the signal through the graph to lie between two values
    /// </summary>
    internal class ConstrainSignal(float min = -1f, float max = 1f) : IAction
    {
        public IGraphData Execute(IGraphData input, IGraphContext context, NodeBase node)
        {
            var matrix = input.GetMatrix();
            matrix.ConstrainInPlace(min, max);
            return input;
        }

        public void Initialise(string data)
        {
            var pos = data.IndexOf(':');
            min = float.Parse(data[..pos]);
            max = float.Parse(data[(pos + 1)..]);
        }

        public string Serialise() => $"{min}:{max}";
    }
}
