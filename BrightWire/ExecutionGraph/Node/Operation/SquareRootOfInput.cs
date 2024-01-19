using System;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Operation
{
    /// <summary>
    /// Calculates the square root of the input
    /// </summary>
    internal class SquareRootOfInput(string? name = null) : NodeBase(name)
    {
        /// <summary>
        /// Derivative of sqrt(x) is 0.5/sqrt(x)
        /// </summary>
        class Backpropagation(SquareRootOfInput source, IMatrix output) : SingleBackpropagationBase<SquareRootOfInput>(source)
        {
            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                var es = errorSignal.GetMatrix();
                using var oneHalf = context.GetLinearAlgebraProvider().CreateMatrix(es.RowCount, es.ColumnCount, (i, j) => 0.5f);
                using var delta = oneHalf.PointwiseMultiply(output);
                return errorSignal.ReplaceWith(delta.PointwiseMultiply(es));
            }
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            var input = signal.GetMatrix();
            var output = input.Sqrt();
            return (this, signal.ReplaceWith(output), () => new Backpropagation(this, output));
        }
    }
}
