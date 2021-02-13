using BrightData.Helper;
using BrightWire.Helper;

namespace BrightWire.ExecutionGraph.Action
{
    /// <summary>
    /// Backpropagates the graph against the error metric
    /// </summary>
    internal class Backpropagate : IAction
    {
        public Backpropagate()
        {
        }

        public void Initialise(string data)
        {
        }

        public string Serialise()
        {
            return "";
        }

        public IGraphData Execute(IGraphData input, IGraphContext context)
        {
            var output = input.GetMatrix();
            if (context.LearningContext != null) {
				var errorMetric = context.LearningContext.ErrorMetric;
                var gradient = errorMetric.CalculateGradient(context, output, context.BatchSequence.Target!.GetMatrix());
                context.Backpropagate(input.ReplaceWith(gradient));
            }
            return input;
        }
    }
}
