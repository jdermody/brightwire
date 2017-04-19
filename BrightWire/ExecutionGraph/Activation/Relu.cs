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
            readonly Relu _source;

            public Backpropagation(Relu source, IMatrix matrix)
            {
                _input = matrix;
                _source = source;
            }

            public void Dispose()
            {
                _input.Dispose();
            }

            public void Backward(IMatrix errorSignal, int channel, IBatchContext context, bool calculateOutput)
            {
                using (var od = _input.ReluDerivative()) {
                    var delta = errorSignal.PointwiseMultiply(od);
                    context.LearningContext.Log("relu-backpropagation", channel, _source.GetHashCode(), errorSignal, delta);
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
            context.RegisterBackpropagation(new Backpropagation(this, input), channel);
            var output = Execute(input, channel, context);
            context.LearningContext.Log("relu", channel, GetHashCode(), input, output);
            return output;
        }

        public IMatrix Execute(IMatrix input, int channel, IBatchContext context)
        {
            return input.ReluActivation();
        }
    }
}
