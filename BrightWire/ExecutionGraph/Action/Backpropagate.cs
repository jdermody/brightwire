using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Action
{
    public class Backpropagate : IAction
    {
        readonly IErrorMetric _errorMetric;

        public Backpropagate(IErrorMetric errorMetric)
        {
            _errorMetric = errorMetric;
        }

        public void Execute(IMatrix input, int channel, IBatchContext context)
        {
            IMatrix target = context.Batch.CurrentSequence.Target, gradient = null;
            if (context.IsTraining)
                gradient = _errorMetric.CalculateGradient(input, target);
            context.SetOutput(input, target, gradient, channel);
        }
    }
}
