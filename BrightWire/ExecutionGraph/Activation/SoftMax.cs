using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Activation
{
    class SoftMax : ILayer
    {
        readonly ILinearAlgebraProvider _lap;

        public SoftMax(ILinearAlgebraProvider lap)
        {
            _lap = lap;
        }

        class Backpropagation : IBackpropagation
        {
            IReadOnlyList<IVector> _rows;

            public Backpropagation(IReadOnlyList<IVector> rows)
            {
                _rows = rows;
            }

            public IMatrix Backward(IMatrix errorSignal, ILearningContext context, bool calculateOutput)
            {
                var lap = context.LinearAlgebraProvider;
                var rowList = new List<IVector>();
                for (var i = 0; i < errorSignal.RowCount; i++) {
                    var sm = _rows[i].SoftmaxDerivative().Multiply(errorSignal.Row(i));
                    rowList.Add(sm.ConvertInPlaceToVector());
                }
                return lap.Create(rowList);
            }

            public void Dispose()
            {
                // nop
            }
        }

        public void Dispose()
        {
            // nop
        }

        public IMatrix Execute(IMatrix input)
        {
            var ret = _Execute(input);
            return ret.Item2;
        }

        (IReadOnlyList<IVector>, IMatrix) _Execute(IMatrix input)
        {
            var rowList = new List<IVector>();
            for (var i = 0; i < input.RowCount; i++) {
                using (var row = input.Row(i))
                    rowList.Add(row.Softmax());
            }

            var ret = _lap.Create(rowList);
            return (rowList, ret);
        }

        public (IMatrix Output, IBackpropagation BackProp) Forward(IMatrix input)
        {
            var ret = _Execute(input);
            return (
                ret.Item2,
                new Backpropagation(ret.Item1)
            );
        }
    }
}
