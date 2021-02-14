using System;
using BrightData;
using BrightWire.ExecutionGraph.Node;

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

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphSequenceContext context)
            {
                using var od = _input.TanhDerivative();
                var delta = errorSignal.GetMatrix().PointwiseMultiply(od);
                return errorSignal.ReplaceWith(delta);
            }
        }

        public Tanh(string? name = null) : base(name) { }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardInternal(IGraphData signal, uint channel, IGraphSequenceContext context, NodeBase? source)
        {
            var input = signal.GetMatrix();
            var output = signal.ReplaceWith(input.TanhActivation());
            return (this, output, () => new Backpropagation(this, input));
        }
    }
}
