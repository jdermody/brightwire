using System;
using System.Collections.Generic;
using BrightData;
using BrightWire.ExecutionGraph.Node;

namespace BrightWire.ExecutionGraph.Activation
{
    /// <summary>
    /// Softmax activation function
    /// https://en.wikipedia.org/wiki/Softmax_function
    /// </summary>
    internal class SoftMax : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<SoftMax>
        {
            readonly IVector[] _rows;

            public Backpropagation(SoftMax source, IVector[] rows) : base(source)
            {
                _rows = rows;
            }

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphSequenceContext context)
            {
                var lap = context.LinearAlgebraProvider;
                var matrix = errorSignal.GetMatrix();
                var rowList = new IVector[matrix.RowCount];
                for (uint i = 0; i < matrix.RowCount; i++) {
                    using var derivative = _rows[(int)i].SoftmaxDerivative();
                    var sm = derivative.Multiply(matrix.Row(i).ToVector(lap));
                    rowList[i] = sm.Reshape();
                }
                var ret = lap.CreateMatrixFromRows(rowList);
                foreach (var item in rowList)
                    item.Dispose();
                return errorSignal.ReplaceWith(ret);
            }
        }

        public SoftMax(string? name = null) : base(name) { }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphSequenceContext context, NodeBase? source)
        {
            var lap = context.LinearAlgebraProvider;
            var input = signal.GetMatrix();
            var rowList = new IVector[input.RowCount];
            for (uint i = 0; i < input.RowCount; i++) {
                using var row = input.Row(i).ToVector(lap);
                rowList[i] = row.Softmax();
            }
            var output = context.LinearAlgebraProvider.CreateMatrixFromRows(rowList);
            return (this, signal.ReplaceWith(output), () => new Backpropagation(this, rowList));
        }
    }
}
