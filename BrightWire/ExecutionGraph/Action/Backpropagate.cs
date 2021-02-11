using System;
using System.Linq;
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
            //_errorMetric = GenericActivator.Create<IErrorMetric>(TypeLoader.LoadType(data));
        }

        public string Serialise()
        {
            //return TypeLoader.GetTypeName(_errorMetric);
            return "";
        }

        public IGraphData Execute(IGraphData input, IGraphSequenceContext context)
        {
            var output = input.GetMatrix();
            if (context.LearningContext != null) {
                var target = context.BatchSequence.Target;
                if (target == null)
                    throw new Exception("Did not find a single target in the batch sequence");

	            var gradient = context.LearningContext.ErrorMetric.CalculateGradient(context, output, target.GetMatrix());
                context.Backpropagate(input.ReplaceWith(gradient));
            }
            return input;
        }
    }
}
