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
        readonly int _maxDepth;

        public BackpropagateThroughTime(IErrorMetric errorMetric, int maxDepth = int.MaxValue)
        {
            _errorMetric = errorMetric;
            _maxDepth = maxDepth;
        }

        public void Initialise(string data)
        {
            _errorMetric = (IErrorMetric)Activator.CreateInstance(Type.GetType(data));
        }

        public string Serialise()
        {
            return _errorMetric.GetType().FullName;
        }

        public void Execute(IGraphData input, IContext context)
        {
            var output = input.GetMatrix();
            context.Output = output;
            if (context.IsTraining) {
                var target = context.BatchSequence.Target;
                if (target == null)
                    context.LearningContext.DeferBackpropagation(signal => context.Backpropagate(signal));
                else {
                    var gradient = _errorMetric.CalculateGradient(output, target);
                    context.LearningContext.BackpropagateThroughTime(gradient.ToGraphData(), _maxDepth);
                }
            }
        }
    }
}
