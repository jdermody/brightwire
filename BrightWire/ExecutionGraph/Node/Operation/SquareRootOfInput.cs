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
            readonly IFloatMatrix _sqrtOutput;

            public Backpropagation(SquareRootOfInput source, IFloatMatrix output) : base(source)
            {
	            _sqrtOutput = output;
            }

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphSequenceContext context)
            {
                var es = errorSignal.GetMatrix();
                using var oneHalf = context.LinearAlgebraProvider.CreateMatrix(es.RowCount, es.ColumnCount, 0.5f);
                using var delta = oneHalf.PointwiseMultiply(_sqrtOutput);
                return errorSignal.ReplaceWith(delta.PointwiseMultiply(es));
            }
        }
        public SquareRootOfInput(string? name = null) : base(name)
        {
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardInternal(IGraphData signal, uint channel, IGraphSequenceContext context, NodeBase? source)
        {
            var input = signal.GetMatrix();
            var output = input.Sqrt();
            return (this, signal.ReplaceWith(output), () => new Backpropagation(this, output));
        }
    }
}
