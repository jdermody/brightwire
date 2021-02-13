using System;
using System.Collections.Generic;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Activation
{
    /// <summary>
    /// Softmax activation function
    /// https://en.wikipedia.org/wiki/Softmax_function
    /// </summary>
    internal class SoftMax : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<SoftMax>
        {
            readonly IFloatVector[] _rows;

            public Backpropagation(SoftMax source, IFloatVector[] rows) : base(source)
            {
                _rows = rows;
            }

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphSequenceContext context)
            {
                var matrix = errorSignal.GetMatrix();
                var rowList = new List<IFloatVector>();
                for (uint i = 0; i < matrix.RowCount; i++) {
                    using var derivative = _rows[(int)i].SoftmaxDerivative();
                    var sm = derivative.Multiply(matrix.Row(i));
                    rowList.Add(sm.ReshapeAsVector());
                }
                var ret = context.LinearAlgebraProvider.CreateMatrixFromRows(rowList);
                foreach (var item in rowList)
                    item.Dispose();
                return errorSignal.ReplaceWith(ret);
            }
        }

        public SoftMax(string? name = null) : base(name) { }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardInternal(IGraphData signal, uint channel, IGraphSequenceContext context, NodeBase? source)
        {
            var input = signal.GetMatrix();
            var rowList = new IFloatVector[input.RowCount];
            for (uint i = 0; i < input.RowCount; i++) {
                using var row = input.Row(i);
                rowList[i] = row.Softmax();
            }
            var output = context.LinearAlgebraProvider.CreateMatrixFromRows(rowList);
            return (this, signal.ReplaceWith(output), () => new Backpropagation(this, rowList));
        }
    }
}
