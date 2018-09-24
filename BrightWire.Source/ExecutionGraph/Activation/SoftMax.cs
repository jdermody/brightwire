using BrightWire.ExecutionGraph.Node;
using System.Collections.Generic;

namespace BrightWire.ExecutionGraph.Activation
{
    /// <summary>
    /// Softmax activation function
    /// https://en.wikipedia.org/wiki/Softmax_function
    /// </summary>
    class SoftMax : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<SoftMax>
        {
            readonly IReadOnlyList<IVector> _rows;

            public Backpropagation(SoftMax source, IReadOnlyList<IVector> rows) : base(source)
            {
                _rows = rows;
            }

            protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                var matrix = errorSignal.GetMatrix();
                var rowList = new List<IVector>();
                for (var i = 0; i < matrix.RowCount; i++) {
                    using (var derivative = _rows[i].SoftmaxDerivative()) {
                        var sm = derivative.Multiply(matrix.Row(i));
                        rowList.Add(sm.ReshapeAsVector());
                    }
                }
                var ret = context.LinearAlgebraProvider.CreateMatrixFromRows(rowList);
                foreach (var item in rowList)
                    item.Dispose();
                return errorSignal.ReplaceWith(ret);
            }
        }

        public SoftMax(string name = null) : base(name) { }

        public override void ExecuteForward(IContext context)
        {
            var input = context.Data.GetMatrix();
            var rowList = new List<IVector>();
            for (var i = 0; i < input.RowCount; i++) {
                using (var row = input.Row(i))
                    rowList.Add(row.Softmax());
            }
            var output = context.LinearAlgebraProvider.CreateMatrixFromRows(rowList);
            _AddNextGraphAction(context, context.Data.ReplaceWith(output), () => new Backpropagation(this, rowList));
        }
    }
}
