using System;
using System.Collections.Generic;
using System.Linq;
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
            readonly ITensorSegment[] _rows;

            public Backpropagation(SoftMax source, ITensorSegment[] rows) : base(source)
            {
                _rows = rows;
            }

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                var lap = context.GetLinearAlgebraProvider();
                var matrix = errorSignal.GetMatrix();
                var rowList = matrix.SoftmaxDerivativePerRow(_rows);
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
            var rowList = input.SoftmaxPerRow();
            var output = lap.CreateMatrixFromRows(rowList);
            return (this, signal.ReplaceWith(output), () => new Backpropagation(this, rowList));
        }
    }
}
