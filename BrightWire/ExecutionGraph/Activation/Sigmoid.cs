using BrightWire.ExecutionGraph.Node;
using BrightData;

namespace BrightWire.ExecutionGraph.Activation
{
    /// <summary>
    /// Sigmoid activation function
    /// https://en.wikipedia.org/wiki/Sigmoid_function
    /// </summary>
    internal class Sigmoid : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<Sigmoid>
        {
            readonly IFloatMatrix _input;

            public Backpropagation(Sigmoid source, IFloatMatrix matrix) : base(source)
            {
                _input = matrix;
            }

            protected override IGraphData Backpropagate(INode? fromNode, IGraphData errorSignal, IGraphSequenceContext context, INode[] parents)
            {
                using var od = _input.SigmoidDerivative();
                var delta = errorSignal.GetMatrix().PointwiseMultiply(od);
                return errorSignal.ReplaceWith(delta);
            }

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphSequenceContext context)
            {
                using var od = _input.SigmoidDerivative();
                var delta = errorSignal.GetMatrix().PointwiseMultiply(od);
                return errorSignal.ReplaceWith(delta);
            }
        }

        public Sigmoid(string? name = null) : base(name) { }

        public override void ExecuteForward(IGraphSequenceContext context)
        {
            var input = context.Data.GetMatrix();
            var output = context.Data.ReplaceWith(input.SigmoidActivation());
            AddNextGraphAction(context, output, () => new Backpropagation(this, input));
        }
    }
}
