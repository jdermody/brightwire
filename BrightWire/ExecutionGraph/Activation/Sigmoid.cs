using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Activation
{
    public class Sigmoid : ILayer
    {
        class Backpropagation : IBackpropagation
        {
            readonly IMatrix _input;

            public Backpropagation(IMatrix matrix)
            {
                _input = matrix;
            }

            public IMatrix Backward(IMatrix errorSignal, ILearningContext context, bool calculateOutput)
            {
                using (var od = _input.SigmoidDerivative())
                    return errorSignal.PointwiseMultiply(od);
            }
        }

        public Sigmoid()
        {
        }

        public void Dispose()
        {
            // nop
        }

        public (IMatrix Output, IBackpropagation BackProp) Forward(IMatrix input)
        {
            var ret = input.SigmoidActivation();
            return (
                ret,
                new Backpropagation(input)
            );
        }
    }
}
