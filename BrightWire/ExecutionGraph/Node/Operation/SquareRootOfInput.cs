using System;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Operation
{
    /// <summary>
    /// Calculates the square root of the input
    /// </summary>
    internal class SquareRootOfInput : NodeBase
    {
        /// <summary>
        /// Derivative of sqrt(x) is 0.5/sqrt(x)
        /// </summary>
        class Backpropagation : SingleBackpropagationBase<SquareRootOfInput>
        {
            readonly IMatrix _sqrtOutput;

            public Backpropagation(SquareRootOfInput source, IMatrix output) : base(source)
            {
	            _sqrtOutput = output;
            }

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                var es = errorSignal.GetMatrix();
                using var oneHalf = context.GetLinearAlgebraProvider().CreateMatrix(es.RowCount, es.ColumnCount, (i, j) => 0.5f);
                using var delta = oneHalf.PointwiseMultiply(_sqrtOutput);
                return errorSignal.ReplaceWith(delta.PointwiseMultiply(es));
            }
        }
        public SquareRootOfInput(string? name = null) : base(name)
        {
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            var input = signal.GetMatrix();
            var output = input.Sqrt();
            return (this, signal.ReplaceWith(output), () => new Backpropagation(this, output));
        }
    }
}
