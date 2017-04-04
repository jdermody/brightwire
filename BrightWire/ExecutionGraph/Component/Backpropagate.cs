using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightWire.ExecutionGraph.Component
{
    public class Backpropagate : IComponent
    {
        readonly IErrorMetric _errorMetric;

        public Backpropagate(IErrorMetric errorMetric)
        {
            _errorMetric = errorMetric;
        }

        public IMatrix Execute(IMatrix input, IWireContext context)
        {
            var delta = _errorMetric.CalculateDelta(input, context.Target);
            context.Backpropagate(delta);
            return input;
        }
    }
}
