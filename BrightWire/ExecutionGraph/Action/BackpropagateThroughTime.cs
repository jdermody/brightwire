using System;
using BrightData.Helper;
using BrightWire.Helper;

namespace BrightWire.ExecutionGraph.Action
{
    /// <summary>
    /// Backpropagates through time (for recurrent neural networks)
    /// </summary>
    internal class BackpropagateThroughTime : IAction
    {
        IErrorMetric _errorMetric;

        public BackpropagateThroughTime(IErrorMetric errorMetric)
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
            if (context.IsTraining) {
	            context.LearningContext.ErrorMetric ??= _errorMetric;

                var target = context.BatchSequence.Target?.GetMatrix();
                if (target == null)
                    context.LearningContext.DeferBackpropagation(null, context.Backpropagate);
                else {
                    var gradient = _errorMetric.CalculateGradient(context, output, target);
                    context.LearningContext.DeferBackpropagation(input.ReplaceWith(gradient), context.Backpropagate);
                }
            }
            return input;
        }
    }
}
