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

        public virtual void Update(IMatrix source, IMatrix delta, ILearningContext context)
        {
            _Update(source, delta, context, 1f);
        }

        protected void _Update(IMatrix source, IMatrix delta, ILearningContext context, float coefficient)
        {
            source.AddInPlace(delta, coefficient, context.LearningRate);
        }
    }
}
