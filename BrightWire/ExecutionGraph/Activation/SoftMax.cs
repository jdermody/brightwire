using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Activation
{
    class SoftMax : IComponent
    {
        class Backpropagation : IBackpropagation
        {
            readonly IReadOnlyList<IVector> _rows;
            readonly SoftMax _source;

            public Backpropagation(SoftMax source, IReadOnlyList<IVector> rows)
            {
                _source = source;
                _rows = rows;
            }

            public void Backward(IMatrix errorSignal, int channel, IBatchContext context, bool calculateOutput)
            {
                var lap = context.LinearAlgebraProvider;
                var rowList = new List<IVector>();
                for (var i = 0; i < errorSignal.RowCount; i++) {
                    using (var derivative = _rows[i].SoftmaxDerivative()) {
                        var sm = derivative.Multiply(errorSignal.Row(i));
                        rowList.Add(sm.ConvertInPlaceToVector());
                    }
                }
                var ret = lap.Create(rowList);
                foreach (var item in rowList)
                    item.Dispose();
                context.LearningContext.Log("softmax-backpropagation", channel, _source.GetHashCode(), errorSignal, ret);
                context.Backpropagate(ret, channel);
            }

            public void Dispose()
            {
                foreach (var item in _rows)
                    item.Dispose();
            }
        }

        public void Dispose()
        {
            // nop
        }

        public IMatrix Execute(IMatrix input, int channel, IBatchContext context)
        {
            var ret = _Execute(input, context);
            foreach (var item in ret.Item1)
                item.Dispose();

            return ret.Item2;
        }

        (IReadOnlyList<IVector>, IMatrix) _Execute(IMatrix input, IBatchContext context)
        {
            var rowList = new List<IVector>();
            for (var i = 0; i < input.RowCount; i++) {
                using (var row = input.Row(i))
                    rowList.Add(row.Softmax());
            }

            var ret = context.LinearAlgebraProvider.Create(rowList);
            return (rowList, ret);
        }

        public IMatrix Train(IMatrix input, int channel, IBatchContext context)
        {
            var ret = _Execute(input, context);
            context.RegisterBackpropagation(new Backpropagation(this, ret.Item1), channel);
            context.LearningContext.Log("softmax", channel, GetHashCode(), input, ret.Item2);
            return ret.Item2;
        }
    }
}
