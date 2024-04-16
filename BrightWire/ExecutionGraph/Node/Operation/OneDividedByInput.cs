using System;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Operation
{
    /// <summary>
    /// Calculates 1/x
    /// </summary>
    internal class OneDividedByInput(string? name = null) : NodeBase(name)
    {
        // Derivative of 1/x is -1/(x squared)
        class Backpropagation(OneDividedByInput source, IMatrix<float> input) : SingleBackpropagationBase<OneDividedByInput>(source)
        {
            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                var es = errorSignal.GetMatrix();
                using var minusOne = context.GetLinearAlgebraProvider().CreateMatrix(es.RowCount, es.ColumnCount, (_, _) => -1f);
                using var inputSquared = input.PointwiseMultiply(input);
                using var delta = minusOne.PointwiseDivide(inputSquared);
                return errorSignal.ReplaceWith(delta.PointwiseMultiply(es));
            }
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            var input = signal.GetMatrix();
            using var ones = context.GetLinearAlgebraProvider().CreateMatrix(input.RowCount, input.ColumnCount, (_, _) => 1f);
            var output = ones.PointwiseDivide(input);
            return (this, signal.ReplaceWith(output), () => new Backpropagation(this, input));
        }
    }
}
