using System;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Activation
{
    /// <summary>
    /// Leaky RELU activation
    /// https://en.wikipedia.org/wiki/Rectifier_(neural_networks)
    /// </summary>
    internal class LeakyRelu : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<LeakyRelu>
        {
            readonly IFloatMatrix _input;

            public Backpropagation(LeakyRelu source, IFloatMatrix matrix) : base(source)
            {
                _input = matrix;
            }

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphSequenceContext context)
            {
                using var od = _input.LeakyReluDerivative();
                var delta = errorSignal.GetMatrix().PointwiseMultiply(od);
                return errorSignal.ReplaceWith(delta);
            }
        }

        public LeakyRelu(string? name = null) : base(name) { }

        public override void ExecuteForward(IGraphSequenceContext context)
        {
            var input = context.Data.GetMatrix();
            var output = context.Data.ReplaceWith(input.LeakyReluActivation());
            AddNextGraphAction(context, output, () => new Backpropagation(this, input));
        }

        public override (INode FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) Forward(IGraphData signal, uint channel, IGraphSequenceContext context, INode? source)
        {
            var input = signal.GetMatrix();
            var output = signal.ReplaceWith(input.LeakyReluActivation());
            return (this, output, () => new Backpropagation(this, input));
        }
    }
}
