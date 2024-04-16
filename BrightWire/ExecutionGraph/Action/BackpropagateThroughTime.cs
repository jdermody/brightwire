using BrightWire.ExecutionGraph.Node;

namespace BrightWire.ExecutionGraph.Action
{
    /// <summary>
    /// Backpropagates through time (for recurrent neural networks)
    /// </summary>
    internal class BackpropagateThroughTime : IAction
    {
        public void Initialise(string data)
        {
        }

        public string Serialise()
        {
            return "";
        }

        public IGraphData Execute(IGraphData input, IGraphContext context, NodeBase node)
        {
            context.Data = input;

            if (context.LearningContext != null) {
                var batchSequence = context.BatchSequence;
                var target = batchSequence.Target?.GetMatrix();

                if (target == null)
                    context.LearningContext.DeferBackpropagation(null, context.Backpropagate);
                else {
                    var gradient = context.LearningContext.ErrorMetric.CalculateGradient(input.GetMatrix(), target);
                    context.LearningContext.DeferBackpropagation(input.ReplaceWith(gradient), context.Backpropagate);
                }
            }
            return input;
        }
    }
}
