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

        public IMatrix Execute(IMatrix input, IWireContext context)
        {
            if (context != null) {
                var gradient = _errorMetric.CalculateGradient(input, context.Target);
                context.Backpropagate(gradient);
            }
            return input;
        }
    }
}
