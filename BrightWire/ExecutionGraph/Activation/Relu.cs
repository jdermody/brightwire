using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Activation
{
    class Relu : IComponent
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

            public void Backward(IMatrix errorSignal, int channel, IBatchContext context, bool calculateOutput)
            {
                using (var od = _input.ReluDerivative()) {
                    var delta = errorSignal.PointwiseMultiply(od);
                    context.Backpropagate(delta, channel);
                }
            }
        }

        public void Dispose()
        {
            // nop
        }

        public IMatrix Train(IMatrix input, int channel, IBatchContext context)
        {
            context.RegisterBackpropagation(new Backpropagation(input), channel);
            return Execute(input, channel, context);
        }

        public IMatrix Execute(IMatrix input, int channel, IBatchContext context)
        {
            return input.ReluActivation();
        }
    }
}
