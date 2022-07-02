using System;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Operation
{
    /// <summary>
    /// Subtracts the input from one (1-x)
    /// </summary>
    internal class OneMinusInput : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<OneMinusInput>
        {
            public Backpropagation(OneMinusInput source) : base(source) { }

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphSequenceContext context)
            {
                var es = errorSignal.GetMatrix();
                using var minusOne = context.GetLinearAlgebraProvider().CreateMatrix(es.RowCount, es.ColumnCount, (_, _) => -1f);
                return errorSignal.ReplaceWith(minusOne.PointwiseMultiply(es));
            }
        }

        public OneMinusInput(string? name = null) : base(name)
        {
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphSequenceContext context, NodeBase? source)
        {
            var input = signal.GetMatrix();
            using var ones = context.GetLinearAlgebraProvider().CreateMatrix(input.RowCount, input.ColumnCount, (_, _) => 1f);
            var output = ones.Subtract(input);
            return (this, signal.ReplaceWith(output), () => new Backpropagation(this));
        }
    }
}
