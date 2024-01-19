using System;
using BrightData;
using BrightWire.ExecutionGraph.Node;

namespace BrightWire.ExecutionGraph.Activation
{
    /// <summary>
    /// Tanh activation function
    /// https://en.wikipedia.org/wiki/Activation_function
    /// </summary>
    internal class Tanh(string? name = null) : NodeBase(name)
    {
        class Backpropagation(Tanh source, IMatrix matrix) : SingleBackpropagationBase<Tanh>(source)
        {
            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                using var od = matrix.TanhDerivative();
                var delta = errorSignal.GetMatrix().PointwiseMultiply(od);
                return errorSignal.ReplaceWith(delta);
            }
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            var input = signal.GetMatrix();
            var output = signal.ReplaceWith(input.Tanh());
            return (this, output, () => new Backpropagation(this, input));
        }
    }
}
