using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Action
{
    public class BackpropagateThroughTime : IAction
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
            return _errorMetric.GetType().FullName;
        }

        public IGraphData Execute(IGraphData input, IContext context)
        {
            var output = input.GetMatrix();
            if (context.IsTraining) {
                var target = context.BatchSequence.Target;
                if (target == null)
                    context.LearningContext.DeferBackpropagation(null, signal => context.Backpropagate(signal));
                else {
                    var gradient = _errorMetric.CalculateGradient(output, target);
                    context.LearningContext.DeferBackpropagation(gradient.ToGraphData(), signal => context.Backpropagate(signal));
                }
            }
            return input;
        }
    }
}
