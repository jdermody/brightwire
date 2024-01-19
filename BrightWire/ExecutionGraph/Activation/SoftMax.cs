using System;
using BrightData;
using BrightWire.ExecutionGraph.Node;

namespace BrightWire.ExecutionGraph.Activation
{
    /// <summary>
    /// Softmax activation function
    /// https://en.wikipedia.org/wiki/Softmax_function
    /// </summary>
    internal class SoftMax(string? name = null) : NodeBase(name)
    {
        class Backpropagation(SoftMax source, INumericSegment<float>[] rows) : SingleBackpropagationBase<SoftMax>(source)
        {
            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                var lap = context.GetLinearAlgebraProvider();
                var matrix = errorSignal.GetMatrix();
                IReadOnlyNumericSegment<float>[] rowList = matrix.SoftmaxDerivativePerRow(rows);
                var ret = lap.CreateMatrixFromRows(rowList);
                rowList.DisposeAll();
                return errorSignal.ReplaceWith(ret);
            }
        }

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
