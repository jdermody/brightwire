using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Operation
{
    /// <summary>
    /// Calculates 1/x
    /// </summary>
    class OneDividedByInput : NodeBase
    {
        // Derivative of 1/x is -1/(x squared)
        class Backpropagation : SingleBackpropagationBase<OneDividedByInput>
        {
            IMatrix _input;

            public Backpropagation(OneDividedByInput source, IMatrix input) : base(source)
            {
                _input = input;
            }

            protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                var es = errorSignal.GetMatrix();
                using (var minusOne = context.LinearAlgebraProvider.CreateMatrix(es.RowCount, es.ColumnCount, -1f))
                using (var inputSquared = _input.PointwiseMultiply(_input))
                using (var delta = minusOne.PointwiseDivide(inputSquared)) {
                    return errorSignal.ReplaceWith(delta.PointwiseMultiply(es));
                }
            }
        }

        public OneDividedByInput(string name = null) : base(name)
        {
        }

        public override void ExecuteForward(IContext context)
        {
            var input = context.Data.GetMatrix();
            using (var ones = context.LinearAlgebraProvider.CreateMatrix(input.RowCount, input.ColumnCount, 1f)) {
                var output = ones.PointwiseDivide(input);
                _AddNextGraphAction(context, context.Data.ReplaceWith(output), () => new Backpropagation(this, input));
            }
        }
    }
}
