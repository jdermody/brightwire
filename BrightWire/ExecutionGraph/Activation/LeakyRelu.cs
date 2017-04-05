using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Activation
{
    class LeakyRelu : ILayer
    {
        class Backpropagation : IBackpropagation
        {
            readonly IMatrix _input;

            public Backpropagation(IMatrix matrix)
            {
                _input = matrix;
            }

            public void Dispose()
            {
                _input.Dispose();
            }

            public IMatrix Backward(IMatrix errorSignal, ILearningContext context, bool calculateOutput)
            {
                using (var od = _input.LeakyReluDerivative())
                    return errorSignal.PointwiseMultiply(od);
            }
        }

        public void Dispose()
        {
            // nop
        }

        public (IMatrix Output, IBackpropagation BackProp) Forward(IMatrix input)
        {
            return (
                Execute(input),
                new Backpropagation(input)
            );
        }

        public IMatrix Execute(IMatrix input)
        {
            return input.LeakyReluActivation();
        }
    }
}
