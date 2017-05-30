using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Helper
{
    class TransposeSignal : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<TransposeSignal>
        {
            public Backpropagation(TransposeSignal source) : base(source)
            {
            }

            protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                var matrix = errorSignal.GetMatrix();
                return errorSignal.ReplaceWith(matrix.Transpose());
            }
        }

        public TransposeSignal(string name = null) : base(name)
        {
        }

        public override void ExecuteForward(IContext context)
        {
            var output = context.Data.GetMatrix().Transpose();
            _AddNextGraphAction(context, new MatrixGraphData(output), () => new Backpropagation(this));
        }
    }
}
