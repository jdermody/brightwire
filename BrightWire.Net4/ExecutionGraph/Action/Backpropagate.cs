using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Action
{
    public class Backpropagate : IAction
    {
        readonly IErrorMetric _errorMetric;

        public Backpropagate(IErrorMetric errorMetric)
        {
            _errorMetric = errorMetric;
        }

        public void Execute(IMatrix input, IContext context)
        {
            IMatrix target = context.BatchSequence.Target, gradient = null;
            if (context.IsTraining) {
                gradient = _errorMetric.CalculateGradient(input, target);
                context.LearningContext?.Log("backprogation-error", gradient);
            }

            context.Backpropagate(input, target, gradient);
        }
    }
}
