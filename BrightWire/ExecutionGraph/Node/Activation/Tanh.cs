using System;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Activation
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

        public override void ExecuteForward(IGraphSequenceContext context)
        {
            var input = context.Data.GetMatrix();
            var output = context.Data.ReplaceWith(input.TanhActivation());
            AddNextGraphAction(context, output, () => new Backpropagation(this, input));
        }

        public override (IGraphData Next, Func<IBackpropagate>? BackProp) Forward(IGraphData signal, uint channel, IGraphSequenceContext context, INode? source)
        {
            var input = signal.GetMatrix();
            var output = signal.ReplaceWith(input.TanhActivation());
            return (output, () => new Backpropagation(this, input));
        }
    }
}
