using BrightData.Helper;
using BrightWire.Helper;

namespace BrightWire.ExecutionGraph.Action
{
    /// <summary>
    /// Backpropagates the graph against the error metric
    /// </summary>
    internal class Backpropagate : IAction
    {
        IErrorMetric _errorMetric;

        public Backpropagate(IErrorMetric errorMetric)
        {
            _errorMetric = errorMetric;
        }

        public void Initialise(string data)
        {
            _errorMetric = GenericActivator.Create<IErrorMetric>(TypeLoader.LoadType(data));
        }

        public string Serialise()
        {
            return TypeLoader.GetTypeName(_errorMetric);
        }

        public IGraphData Execute(IGraphData input, IGraphContext context)
        {
            var output = input.GetMatrix();
            if (context.LearningContext != null) {
				context.LearningContext.ErrorMetric ??= _errorMetric;

	            var gradient = _errorMetric.CalculateGradient(context, output, context.BatchSequence.Target!.GetMatrix());
                context.Backpropagate(input.ReplaceWith(gradient));
            }
            return input;
        }
    }
}
