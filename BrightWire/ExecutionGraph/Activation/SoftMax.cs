using System;
using System.Collections.Generic;
using System.Threading;
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

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                var lap = context.GetLinearAlgebraProvider();
                var matrix = errorSignal.GetMatrix();
                var rowList = new IVector[matrix.RowCount];
                for (uint i = 0; i < matrix.RowCount; i++) {
                    using var derivative = _rows[(int)i].SoftmaxDerivative();
                    using var row = matrix.GetRowVector(i);
                    var sm = derivative.Multiply(row);
                    rowList[i] = sm.Reshape();
                }
                var ret = lap.CreateMatrixFromRows(rowList);
                rowList.DisposeAll();
                return errorSignal.ReplaceWith(ret);
            }
        }

        public SoftMax(string? name = null) : base(name) { }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            var lap = context.GetLinearAlgebraProvider();
            var input = signal.GetMatrix();
            var rowList = new IVector[input.RowCount];
            using var transposed = input.Transpose();
            for (uint i = 0; i < input.RowCount; i++) {
                using var row = transposed.GetColumn(i).Create(lap);
                rowList[i] = row.Softmax();
            }
            var output = lap.CreateMatrixFromRows(rowList);
            return (this, signal.ReplaceWith(output), () => new Backpropagation(this, rowList));
        }
    }
}
