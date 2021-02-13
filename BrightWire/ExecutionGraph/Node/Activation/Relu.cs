using System;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Activation
{
    /// <summary>
    /// RELu activation
    /// https://en.wikipedia.org/wiki/Rectifier_(neural_networks)
    /// </summary>
    internal class Relu : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<Relu>
        {
            readonly IFloatMatrix _input;

            public Backpropagation(Relu source, IFloatMatrix matrix) : base(source)
            {
                _input = matrix;
            }

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphSequenceContext context)
            {
                using var od = _input.ReluDerivative();
                var delta = errorSignal.GetMatrix().PointwiseMultiply(od);
                return errorSignal.ReplaceWith(delta);
            }
        }

        public Relu(string? name = null) : base(name) { }

        public override void ExecuteForward(IGraphSequenceContext context)
        {
            var input = context.Data.GetMatrix();
            var output = context.Data.ReplaceWith(input.ReluActivation());
            AddNextGraphAction(context, output, () => new Backpropagation(this, input));
        }

        public override (IGraphData Next, Func<IBackpropagate>? BackProp) Forward(IGraphData signal, uint channel, IGraphSequenceContext context, INode? source)
        {
            var input = signal.GetMatrix();
            var output = signal.ReplaceWith(input.ReluActivation());
            return (output, () => new Backpropagation(this, input));
        }
    }
}
