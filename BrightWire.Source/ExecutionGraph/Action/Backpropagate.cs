using System;

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
                var gradient = _errorMetric.CalculateGradient(context, output, context.BatchSequence.Target.GetMatrix());
                context.Backpropagate(input.ReplaceWith(gradient));
            }
            return input;
        }
    }
}
