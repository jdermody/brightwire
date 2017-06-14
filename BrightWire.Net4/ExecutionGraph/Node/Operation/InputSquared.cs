using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Operation
{
    class InputSquared : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<InputSquared>
        {
            public Backpropagation(InputSquared source) : base(source)
            {
            }

            protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                var es = errorSignal.GetMatrix();
                es.Multiply(2f);
                return errorSignal.ReplaceWith(es);
            }
        }

        public InputSquared(string name = null) : base(name)
        {
        }

        public override void ExecuteForward(IContext context)
        {
            var input = context.Data.GetMatrix();
            var inputSquared = input.PointwiseMultiply(input);
            _AddNextGraphAction(context, context.Data.ReplaceWith(inputSquared), () => new Backpropagation(this));
        }
    }
}
