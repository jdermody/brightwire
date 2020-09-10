using System.Collections.Generic;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Operation
{
    /// <summary>
    /// Calculates the square root of the input
    /// </summary>
    class SquareRootOfInput : NodeBase
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

            protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                var es = errorSignal.GetMatrix();
                using (var oneHalf = context.LinearAlgebraProvider.CreateMatrix(es.RowCount, es.ColumnCount, 0.5f))
                using (var delta = oneHalf.PointwiseMultiply(_sqrtOutput))
                    return errorSignal.ReplaceWith(delta.PointwiseMultiply(es));
            }
        }
        public SquareRootOfInput(string name = null) : base(name)
        {
        }

        public override void ExecuteForward(IContext context)
        {
            var input = context.Data.GetMatrix();
            var output = input.Sqrt(1e-8f);
            _AddNextGraphAction(context, context.Data.ReplaceWith(output), () => new Backpropagation(this, output));
        }
    }
}
