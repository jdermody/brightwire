using System;

namespace BrightWire.ExecutionGraph.Node.Action
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
            //_errorMetric = GenericActivator.Create<IErrorMetric>(TypeLoader.LoadType(data));
        }

        public string Serialise()
        {
            //return TypeLoader.GetTypeName(_errorMetric);
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
