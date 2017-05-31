using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node;
using System.Collections.Generic;
using System.Linq;

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
            readonly IReadOnlyList<IReadOnlyList<IVector>> _rows;

            public Backpropagation(SoftMax source, IReadOnlyList<IReadOnlyList<IVector>> rows) : base(source)
            {
                _rows = rows;
            }

            protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                var matrixList = errorSignal.AllMatrices.Select((e, ind) => {
                    var row = _rows[ind];
                    var rowList = new List<IVector>();
                    for (var i = 0; i < e.RowCount; i++) {
                        using (var derivative = row[i].SoftmaxDerivative()) {
                            var sm = derivative.Multiply(e.Row(i));
                            rowList.Add(sm.ConvertInPlaceToVector());
                        }
                    }
                    var ret = context.LinearAlgebraProvider.CreateMatrix(rowList);
                    foreach (var item in rowList)
                        item.Dispose();
                    return ret;
                }).ToList();
                return errorSignal.ReplaceWith(context, matrixList);
            }
        }

        public SoftMax(string name = null) : base(name) { }

        public override void ExecuteForward(IContext context)
        {
            var input = context.Data.AllMatrices;
            var output = new List<List<IVector>>();
            var matrixList = new List<IMatrix>();

            foreach (var item in input) {
                var rowList = new List<IVector>();
                for (var i = 0; i < item.RowCount; i++) {
                    using (var row = item.Row(i))
                        rowList.Add(row.Softmax());
                }
                output.Add(rowList);
                matrixList.Add(context.LinearAlgebraProvider.CreateMatrix(rowList));
            }

            _AddNextGraphAction(context, context.Data.ReplaceWith(context, matrixList), () => new Backpropagation(this, output));
        }
    }
}
