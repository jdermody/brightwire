using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Layer
{
    public class FeedForward : ILayer
    {
        protected readonly IVector _bias;
        protected readonly IMatrix _weight;
        protected readonly IGradientDescentOptimisation _updater;

        class Backpropagation : IBackpropagation
        {
            readonly FeedForward _layer;
            readonly IMatrix _input = null;

            public Backpropagation(FeedForward layer, IMatrix input)
            {
                _layer = layer;
                _input = input;
            }

            public IMatrix Backward(IMatrix errorSignal, ILearningContext context, bool calculateOutput)
            {
                // work out the next error signal
                IMatrix ret = null;
                if (calculateOutput)
                    ret = errorSignal.TransposeAndMultiply(_layer._weight);

                // calculate the update to the weights
                var weightUpdate = _input.TransposeThisAndMultiply(errorSignal);

                // store the updates
                context.Store(errorSignal, err => _UpdateBias(err, context));
                context.Store(weightUpdate, err => _layer.Update(err, context));

                return ret;
            }

            void _UpdateBias(IMatrix delta, ILearningContext context)
            {
                using (var columnSums = delta.ColumnSums())
                    _layer._bias.AddInPlace(columnSums, 1f / columnSums.Count, context.LearningRate);
            }
        }

        public FeedForward(IVector bias, IMatrix weight, IGradientDescentOptimisation updater)
        {
            _bias = bias;
            _weight = weight;
            _updater = updater;
        }

        public void Dispose()
        {
            _bias.Dispose();
            _weight.Dispose();
        }

        public void Update(IMatrix delta, ILearningContext context)
        {
            _updater.Update(_weight, delta, context);
        }

        public (IMatrix Output, IBackpropagation BackProp) Forward(IMatrix input)
        {
            var output = input.Multiply(_weight);
            output.AddToEachRow(_bias);
            return (
                output,
                new Backpropagation(this, input)
            );
        }
    }
}
