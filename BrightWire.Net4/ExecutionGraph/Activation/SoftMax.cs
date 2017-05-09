using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Activation
{
    class SoftMax : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase
        {
            readonly IReadOnlyList<IVector> _rows;
            readonly SoftMax _source;

            public Backpropagation(SoftMax source, IReadOnlyList<IVector> rows)
            {
                _source = source;
                _rows = rows;
            }

            protected override IMatrix _Backward(IMatrix errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                var rowList = new List<IVector>();
                for (var i = 0; i < errorSignal.RowCount; i++) {
                    using (var derivative = _rows[i].SoftmaxDerivative()) {
                        var sm = derivative.Multiply(errorSignal.Row(i));
                        rowList.Add(sm.ConvertInPlaceToVector());
                    }
                }
                var ret = context.LinearAlgebraProvider.Create(rowList);
                foreach (var item in rowList)
                    item.Dispose();
                //context.LearningContext.Log("softmax-backpropagation", channel, _source.GetHashCode(), errorSignal, ret);
                return ret;
            }

            protected override void _Dispose(bool isDisposing)
            {
                foreach (var item in _rows)
                    item.Dispose();
            }
        }

        public SoftMax(string name = null) : base(name) { }

        public override void ExecuteForward(IContext context)
        {
            var input = context.Data.GetAsMatrix();
            var rowList = new List<IVector>();

            for (var i = 0; i < input.RowCount; i++) {
                using (var row = input.Row(i))
                    rowList.Add(row.Softmax());
            }

            var output = context.LinearAlgebraProvider.Create(rowList);
            _AddNextGraphAction(context, new MatrixGraphData(output), () => new Backpropagation(this, rowList));
        }
    }
}
