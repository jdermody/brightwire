using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightWire.ExecutionGraph.Component
{
    class Backpropagate : IComponent
    {
        readonly IErrorMetric _errorMetric;

        public Backpropagate(IErrorMetric errorMetric)
        {
            _errorMetric = errorMetric;
        }

        public void Dispose()
        {
            // nop
        }

        public IMatrix Execute(IMatrix input, int channel, IBatchContext context)
        {
            context.SetOutput(input, context.Batch.CurrentSequence.Target, null, channel);
            return input;
        }

        public IMatrix Train(IMatrix input, int channel, IBatchContext context)
        {
            var target = context.Batch.CurrentSequence.Target;
            var gradient = _errorMetric.CalculateGradient(input, target);
            context.SetOutput(input, target, gradient, channel);
            return input;
        }
    }
}
