using System;
using BrightData;
using BrightWire.ExecutionGraph.Node;

namespace BrightWire.ExecutionGraph.Activation
{
    /// <summary>
    /// Leaky RELU activation
    /// https://en.wikipedia.org/wiki/Rectifier_(neural_networks)
    /// </summary>
    internal class LeakyRelu(string? name = null) : NodeBase(name)
    {
        class Backpropagation(LeakyRelu source, IMatrix matrix) : SingleBackpropagationBase<LeakyRelu>(source)
        {
            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                using var od = matrix.LeakyReluDerivative();
                var delta = errorSignal.GetMatrix().PointwiseMultiply(od);
                return errorSignal.ReplaceWith(delta);
            }
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            var input = signal.GetMatrix();
            var output = signal.ReplaceWith(input.LeakyRelu());
            return (this, output, () => new Backpropagation(this, input));
        }
    }
}
