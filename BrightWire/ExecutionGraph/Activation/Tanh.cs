using BrightWire.ExecutionGraph.Node;
using BrightData;

namespace BrightWire.ExecutionGraph.Activation
{
    /// <summary>
    /// Tanh activation function
    /// https://en.wikipedia.org/wiki/Activation_function
    /// </summary>
    internal class Tanh : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<Tanh>
        {
            readonly IFloatMatrix _input;

            public Backpropagation(Tanh source, IFloatMatrix matrix) : base(source)
            {
                _input = matrix;
            }

            protected override IGraphData Backpropagate(INode? fromNode, IGraphData errorSignal, IGraphSequenceContext context, INode[] parents)
            {
                using var od = _input.TanhDerivative();
                var delta = errorSignal.GetMatrix().PointwiseMultiply(od);
                return errorSignal.ReplaceWith(delta);
            }

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphSequenceContext context)
            {
                using var od = _input.TanhDerivative();
                var delta = errorSignal.GetMatrix().PointwiseMultiply(od);
                return errorSignal.ReplaceWith(delta);
            }
        }

        public Tanh(string? name = null) : base(name) { }

        public override void ExecuteForward(IGraphSequenceContext context)
        {
            var input = context.Data.GetMatrix();
            var output = context.Data.ReplaceWith(input.TanhActivation());
            AddNextGraphAction(context, output, () => new Backpropagation(this, input));
        }
    }
}
