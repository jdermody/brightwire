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
                    context.LearningContext.DeferBackpropagation(signal => _BPTT(output, signal.GetMatrix(), context));
                else {
                    var signal = _BPTT(output, target, context);
                    context.LearningContext.BackpropagateThroughTime(signal, _maxDepth);
                }
            }
        }

        IGraphData _BPTT(IMatrix output, IMatrix target, IContext context)
        {
            var gradient = _errorMetric.CalculateGradient(output, target);
            context.LearningContext?.Log("backprogation-error", gradient);
            return context.Backpropagate(gradient.ToGraphData());
        }
    }
}
