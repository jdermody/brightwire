using System;

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
            _errorMetric = (IErrorMetric)Activator.CreateInstance(Type.GetType(data));
        }

        public string Serialise()
        {
            return _errorMetric.GetType().AssemblyQualifiedName;
        }

        public IGraphData Execute(IGraphData input, IContext context)
        {
            var output = input.GetMatrix();
            if (context.IsTraining) {
                var target = context.BatchSequence.Target?.GetMatrix();
                if (target == null)
                    context.LearningContext.DeferBackpropagation(null, signal => context.Backpropagate(signal));
                else {
                    var gradient = _errorMetric.CalculateGradient(context, output, target);
                    context.LearningContext.DeferBackpropagation(input.ReplaceWith(gradient), signal => context.Backpropagate(signal));
                }
            }
            return input;
        }
    }
}
