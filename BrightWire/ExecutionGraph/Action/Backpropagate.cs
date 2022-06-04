using System;
using BrightWire.ExecutionGraph.Node;

namespace BrightWire.ExecutionGraph.Action
{
    /// <summary>
    /// Backpropagates the graph against the error metric
    /// </summary>
    internal class Backpropagate : IAction
    {
        public void Initialise(string data)
        {
        }

        public string Serialise()
        {
            return "";
        }

        public IGraphData Execute(IGraphData input, IGraphSequenceContext context, NodeBase node)
        {
            context.Data = input;
            if (context.LearningContext != null) {
                var target = context.BatchSequence.Target;
                if (target == null)
                    throw new Exception("Did not find a single target in the batch sequence");

	            var gradient = context.LearningContext.ErrorMetric.CalculateGradient(context, input.GetMatrix(), target.GetMatrix());
                context.Backpropagate(input.ReplaceWith(gradient));
            }
            return input;
        }
    }
}
