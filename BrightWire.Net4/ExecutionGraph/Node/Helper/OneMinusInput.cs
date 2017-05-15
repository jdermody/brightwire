using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Helper
{
    class OneMinusInput : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<OneMinusInput>
        {
            public Backpropagation(OneMinusInput source) : base(source) { }

            protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                var es = errorSignal.GetMatrix();
                using (var minusOne = context.LinearAlgebraProvider.Create(es.RowCount, es.ColumnCount, -1f))
                    return minusOne.PointwiseMultiply(es).ToGraphData();
            }
        }

        public OneMinusInput(string name = null) : base(name)
        {
        }

        public override void ExecuteForward(IContext context)
        {
            var input = context.Data.GetMatrix();
            using (var ones = context.LinearAlgebraProvider.Create(input.RowCount, input.ColumnCount, 1f)) {
                var output = ones.Subtract(input);
                _AddNextGraphAction(context, new MatrixGraphData(output), () => new Backpropagation(this));
            }
        }
    }
}
