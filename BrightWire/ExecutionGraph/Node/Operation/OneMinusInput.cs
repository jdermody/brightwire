using System.Collections.Generic;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Operation
{
    /// <summary>
    /// Subtracts the input from one (1-x)
    /// </summary>
    class OneMinusInput : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<OneMinusInput>
        {
            public Backpropagation(OneMinusInput source) : base(source) { }

            protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IContext context, INode[] parents)
            {
                var es = errorSignal.GetMatrix();
                using (var minusOne = context.LinearAlgebraProvider.CreateMatrix(es.RowCount, es.ColumnCount, -1f))
                    return errorSignal.ReplaceWith(minusOne.PointwiseMultiply(es));
            }
        }

        public OneMinusInput(string name = null) : base(name)
        {
        }

        public override void ExecuteForward(IContext context)
        {
            var input = context.Data.GetMatrix();
            using var ones = context.LinearAlgebraProvider.CreateMatrix(input.RowCount, input.ColumnCount, 1f);
            var output = ones.Subtract(input);
            _AddNextGraphAction(context, context.Data.ReplaceWith(output), () => new Backpropagation(this));
        }
    }
}
