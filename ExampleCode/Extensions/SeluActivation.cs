using System;
using BrightData.Helper;
using BrightWire;
using BrightWire.ExecutionGraph.Node;

namespace ExampleCode.Extensions
{
    /// <summary>
    /// Example of custom activation function, implemented from:
    /// https://arxiv.org/abs/1706.02515
    /// </summary>
    internal class SeluActivation : NodeBase
    {
        const float Alpha = 1.6732632423543772848170429916717f;
        const float Scale = 1.0507009873554804934193349852946f;

        /// <summary>
        /// Backpropagation of SELU activation
        /// </summary>
        class Backpropagation : SingleBackpropagationBase<SeluActivation>
        {
            public Backpropagation(SeluActivation source) : base(source) { }

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphSequenceContext context)
            {
                var matrix = errorSignal.GetMatrix().AsIndexable();
                var delta = context.LinearAlgebraProvider.CreateMatrix(matrix.RowCount, matrix.ColumnCount, (i, j) =>
                {
                    var x = matrix[i, j];
                    if (x >= 0)
                        return Scale;
                    return Scale * Alpha * FloatMath.Exp(x);
                });
                return errorSignal.ReplaceWith(delta);
            }
        }

        public SeluActivation(string? name = null) : base(name) { }

        public override void ExecuteForward(IGraphSequenceContext context)
        {
            var matrix = context.Data.GetMatrix().AsIndexable();
            var output = context.LinearAlgebraProvider.CreateMatrix(matrix.RowCount, matrix.ColumnCount, (i, j) =>
            {
                var x = matrix[i, j];
                if (x >= 0)
                    return Scale * x;
                return Scale * (Alpha * FloatMath.Exp(x) - Alpha);
            });
            AddNextGraphAction(context, context.Data.ReplaceWith(output), () => new Backpropagation(this));
        }

        public override (IGraphData Next, Func<IBackpropagate>? BackProp) Forward(IGraphData signal, uint channel, IGraphSequenceContext context, INode? source)
        {
            var matrix = context.Data.GetMatrix().AsIndexable();
            var output = context.LinearAlgebraProvider.CreateMatrix(matrix.RowCount, matrix.ColumnCount, (i, j) =>
            {
                var x = matrix[i, j];
                if (x >= 0)
                    return Scale * x;
                return Scale * (Alpha * FloatMath.Exp(x) - Alpha);
            });
            return (context.Data.ReplaceWith(output), () => new Backpropagation(this));
        }
    }
}
