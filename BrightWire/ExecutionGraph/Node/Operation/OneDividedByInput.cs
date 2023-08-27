using System;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Operation
{
    /// <summary>
    /// Calculates 1/x
    /// </summary>
    internal class OneDividedByInput : NodeBase
    {
        // Derivative of 1/x is -1/(x squared)
        class Backpropagation : SingleBackpropagationBase<OneDividedByInput>
        {
            readonly IMatrix _input;

            public Backpropagation(OneDividedByInput source, IMatrix input) : base(source)
            {
                _input = input;
            }

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                var es = errorSignal.GetMatrix();
                using var minusOne = context.GetLinearAlgebraProvider().CreateMatrix(es.RowCount, es.ColumnCount, (_, _) => -1f);
                using var inputSquared = _input.PointwiseMultiply(_input);
                using var delta = minusOne.PointwiseDivide(inputSquared);
                return errorSignal.ReplaceWith(delta.PointwiseMultiply(es));
            }
        }

        public OneDividedByInput(string? name = null) : base(name)
        {
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
