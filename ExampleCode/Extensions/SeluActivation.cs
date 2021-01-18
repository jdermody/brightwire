using System;
using System.Collections.Generic;
using System.Text;
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
        const float ALPHA = 1.6732632423543772848170429916717f;
        const float SCALE = 1.0507009873554804934193349852946f;

        /// <summary>
        /// Backpropagation of SELU activation
        /// </summary>
        class Backpropagation : SingleBackpropagationBase<SeluActivation>
        {
            public Backpropagation(SeluActivation source) : base(source) { }

            protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IGraphContext context, INode[] parents)
            {
                var matrix = errorSignal.GetMatrix().AsIndexable();
                var delta = context.LinearAlgebraProvider.CreateMatrix(matrix.RowCount, matrix.ColumnCount, (i, j) =>
                {
                    var x = matrix[i, j];
                    if (x >= 0)
                        return SCALE;
                    return SCALE * ALPHA * FloatMath.Exp(x);
                });
                return errorSignal.ReplaceWith(delta);
            }
        }

        public SeluActivation(string name = null) : base(name) { }

        public override void ExecuteForward(IGraphContext context)
        {
            var matrix = context.Data.GetMatrix().AsIndexable();
            var output = context.LinearAlgebraProvider.CreateMatrix(matrix.RowCount, matrix.ColumnCount, (i, j) =>
            {
                var x = matrix[i, j];
                if (x >= 0)
                    return SCALE * x;
                return SCALE * (ALPHA * FloatMath.Exp(x) - ALPHA);
            });
            _AddNextGraphAction(context, context.Data.ReplaceWith(output), () => new Backpropagation(this));
        }
    }
}
