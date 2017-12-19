using BrightWire.ExecutionGraph.Node;
using System.Collections.Generic;

namespace BrightWire.ExecutionGraph.Activation
{
    /// <summary>
    /// Sigmoid activation function
    /// https://en.wikipedia.org/wiki/Sigmoid_function
    /// </summary>
    class Sigmoid : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<Sigmoid>
        {
            readonly IMatrix _input;

            public Backpropagation(Sigmoid source, IMatrix matrix) : base(source)
            {
                _input = matrix;
            }

            protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                using (var od = _input.SigmoidDerivative()) {
                    var delta = errorSignal.GetMatrix().PointwiseMultiply(od);
                    return errorSignal.ReplaceWith(delta);
                }
            }
        }

        public Sigmoid(string name = null) : base(name) { }

        public override void ExecuteForward(IContext context)
        {
            var input = context.Data.GetMatrix();
            var output = context.Data.ReplaceWith(input.SigmoidActivation());
            _AddNextGraphAction(context, output, () => new Backpropagation(this, input));
        }
    }
}
