using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Component
{
    class OneMinusInput : IComponent
    {
        class Backpropagation : IBackpropagation
        {
            readonly ILinearAlgebraProvider _lap;

            public Backpropagation(ILinearAlgebraProvider lap)
            {
                _lap = lap;
            }

            public void Backward(IMatrix errorSignal, int channel, IBatchContext context, bool calculateOutput)
            {
                if (calculateOutput) {
                    using (var minusOne = _lap.Create(errorSignal.RowCount, errorSignal.ColumnCount, -1f))
                        context.Backpropagate(errorSignal.PointwiseMultiply(errorSignal), channel);
                }
            }

            public void Dispose()
            {
                // nop
            }
        }

        readonly ILinearAlgebraProvider _lap;


        public OneMinusInput(ILinearAlgebraProvider lap)
        {
            _lap = lap;
        }

        public void Dispose()
        {
            // nop
        }

        public IMatrix Execute(IMatrix input, int channel, IBatchContext context)
        {
            using (var ones = _lap.Create(input.RowCount, input.ColumnCount, 1f))
                return ones.Subtract(input);
        }

        public IMatrix Train(IMatrix input, int channel, IBatchContext context)
        {
            context.RegisterBackpropagation(new Backpropagation(_lap), channel);
            return Execute(input, channel, context);
        }
    }
}
