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
            readonly IFloatMatrix _input;

            public Backpropagation(OneDividedByInput source, IFloatMatrix input) : base(source)
            {
                _input = input;
            }

            protected override IGraphData Backpropagate(INode? fromNode, IGraphData errorSignal, IGraphContext context, INode[] parents)
            {
                var es = errorSignal.GetMatrix();
                using var minusOne = context.LinearAlgebraProvider.CreateMatrix(es.RowCount, es.ColumnCount, -1f);
                using var inputSquared = _input.PointwiseMultiply(_input);
                using var delta = minusOne.PointwiseDivide(inputSquared);
                return errorSignal.ReplaceWith(delta.PointwiseMultiply(es));
            }
        }

        public OneDividedByInput(string? name = null) : base(name)
        {
        }

        public override void ExecuteForward(IGraphContext context)
        {
            var input = context.Data.GetMatrix();
            using var ones = context.LinearAlgebraProvider.CreateMatrix(input.RowCount, input.ColumnCount, 1f);
            var output = ones.PointwiseDivide(input);
            AddNextGraphAction(context, context.Data.ReplaceWith(output), () => new Backpropagation(this, input));
        }
    }
}
