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
        class Backpropagation : SingleBackpropagationBase
        {
            readonly IReadOnlyList<IReadOnlyList<IVector>> _rows;
            readonly SoftMax _source;

            public Backpropagation(SoftMax source, IReadOnlyList<IReadOnlyList<IVector>> rows)
            {
                _source = source;
                _rows = rows;
            }

            protected override IGraphData _Backward(IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                return context.ToGraphData(errorSignal.Decompose().Select((e, ind) => {
                    var row = _rows[ind];
                    var rowList = new List<IVector>();
                    for (var i = 0; i < e.RowCount; i++) {
                        using (var derivative = row[i].SoftmaxDerivative()) {
                            var sm = derivative.Multiply(e.Row(i));
                            rowList.Add(sm.ConvertInPlaceToVector());
                        }
                    }
                    var ret = context.LinearAlgebraProvider.Create(rowList);
                    foreach (var item in rowList)
                        item.Dispose();
                    //context.LearningContext.Log("softmax-backpropagation", channel, _source.GetHashCode(), errorSignal, ret);
                    return ret;
                }));
            }

            protected override void _Dispose(bool isDisposing)
            {
                foreach (var row in _rows) {
                    foreach (var item in row)
                        item.Dispose();
                }
            }
        }

        public SoftMax(string name = null) : base(name) { }

        public override void ExecuteForward(IContext context)
        {
            var input = context.Data.Decompose();
            var output = new List<List<IVector>>();
            var matrixList = new List<IMatrix>();

            foreach(var item in input) {
                var rowList = new List<IVector>();
                for (var i = 0; i < item.RowCount; i++) {
                    using (var row = item.Row(i))
                        rowList.Add(row.Softmax());
                }
                output.Add(rowList);
                matrixList.Add(context.LinearAlgebraProvider.Create(rowList));
            }
            
            _AddNextGraphAction(context, context.ToGraphData(matrixList), () => new Backpropagation(this, output));
        }
    }
}
