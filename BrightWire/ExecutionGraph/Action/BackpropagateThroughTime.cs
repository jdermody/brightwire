using BrightData.Helper;
using BrightWire.Helper;

namespace BrightWire.ExecutionGraph.Action
{
    /// <summary>
    /// Backpropagates through time (for recurrent neural networks)
    /// </summary>
    internal class BackpropagateThroughTime : IAction
    {
        public BackpropagateThroughTime()
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
                var target = context.BatchSequence.Target?.GetMatrix();
                if (target == null)
                    context.LearningContext.DeferBackpropagation(null, context.Backpropagate);
                else {
                    var errorMetric = context.LearningContext.ErrorMetric;
                    var gradient = errorMetric.CalculateGradient(context, output, target);
                    context.LearningContext.DeferBackpropagation(input.ReplaceWith(gradient), context.Backpropagate);
                }
            }
            return input;
        }
    }
}
