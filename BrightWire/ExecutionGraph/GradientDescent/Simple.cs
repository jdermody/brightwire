using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.GradientDescent
{
    class Simple : IGradientDescentOptimisation
    {
        public void Dispose()
        {
            // nop
        }

        public void Update(IMatrix source, IMatrix delta, ILearningContext context)
        {
            source.AddInPlace(delta, 1f, context.LearningRate);
        }
    }
}
