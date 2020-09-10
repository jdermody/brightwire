using BrightWire.ExecutionGraph.Node;
using System.Collections.Generic;
using BrightData;

namespace BrightWire.ExecutionGraph.Activation
{
    /// <summary>
    /// Tanh activation function
    /// https://en.wikipedia.org/wiki/Activation_function
    /// </summary>
    class Tanh : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<Tanh>
        {
            readonly IFloatMatrix _input;

            public Backpropagation(Tanh source, IFloatMatrix matrix) : base(source)
            {
                _input = matrix;
            }

            protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                using (var od = _input.TanhDerivative()) {
                    var delta = errorSignal.GetMatrix().PointwiseMultiply(od);
                    return errorSignal.ReplaceWith(delta);
                }
            }
        }

        public Tanh(string name = null) : base(name) { }

        public override void ExecuteForward(IContext context)
        {
            var input = context.Data.GetMatrix();
            var output = context.Data.ReplaceWith(input.TanhActivation());
            _AddNextGraphAction(context, output, () => new Backpropagation(this, input));
        }
    }
}
