using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightWire.ExecutionGraph.Activation
{
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
                var rowList = new List<IVector>();
                var error = errorSignal.GetMatrix();
                for (var i = 0; i < error.RowCount; i++) {
                    using (var derivative = _rows[i].SoftmaxDerivative()) {
                        var sm = derivative.Multiply(error.Row(i));
                        rowList.Add(sm.ConvertInPlaceToVector());
                    }
                }
                var ret = context.LinearAlgebraProvider.CreateMatrix(rowList);
                foreach (var item in rowList)
                    item.Dispose();
                return errorSignal.ReplaceWith(ret);

                //return context.ToGraphData(errorSignal.Decompose().Select((e, ind) => {
                //    var row = _rows[ind];
                //    var rowList = new List<IVector>();
                //    for (var i = 0; i < e.RowCount; i++) {
                //        using (var derivative = _rows[i].SoftmaxDerivative()) {
                //            var sm = derivative.Multiply(e.Row(i));
                //            rowList.Add(sm.ConvertInPlaceToVector());
                //        }
                //    }
                //    var ret = context.LinearAlgebraProvider.CreateMatrix(rowList);
                //    foreach (var item in rowList)
                //        item.Dispose();
                //    //context.LearningContext.Log("softmax-backpropagation", channel, _source.GetHashCode(), errorSignal, ret);
                //    return ret;
                //}));
            }

            protected override void _Dispose(bool isDisposing)
            {
                //foreach (var row in _rows) {
                //    foreach (var item in row)
                //        item.Dispose();
                //}
            }
        }

        public SoftMax(string name = null) : base(name) { }

        public override void ExecuteForward(IContext context)
        {
            // TODO: break tensors into matrices?
            var input = context.Data.GetMatrix();
            var rowList = new List<IVector>();

            for (var i = 0; i < input.RowCount; i++) {
                using (var row = input.Row(i))
                    rowList.Add(row.Softmax());
            }
            var output = context.LinearAlgebraProvider.CreateMatrix(rowList);

            _AddNextGraphAction(context, context.Data.ReplaceWith(output), () => new Backpropagation(this, rowList));
        }
    }
}
