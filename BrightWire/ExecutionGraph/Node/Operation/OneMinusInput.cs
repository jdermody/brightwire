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
                using var minusOne = context.LinearAlgebraProvider.CreateMatrix(es.RowCount, es.ColumnCount, -1f);
                return errorSignal.ReplaceWith(minusOne.PointwiseMultiply(es));
            }
        }

        public OneMinusInput(string? name = null) : base(name)
        {
        }

        public override void ExecuteForward(IGraphSequenceContext context)
        {
            var input = context.Data.GetMatrix();
            using var ones = context.LinearAlgebraProvider.CreateMatrix(input.RowCount, input.ColumnCount, 1f);
            var output = ones.Subtract(input);
            AddNextGraphAction(context, context.Data.ReplaceWith(output), () => new Backpropagation(this));
        }

        public override (INode FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) Forward(IGraphData signal, uint channel, IGraphSequenceContext context, INode? source)
        {
            var input = context.Data.GetMatrix();
            using var ones = context.LinearAlgebraProvider.CreateMatrix(input.RowCount, input.ColumnCount, 1f);
            var output = ones.Subtract(input);
            return (this, context.Data.ReplaceWith(output), () => new Backpropagation(this));
        }
    }
}
