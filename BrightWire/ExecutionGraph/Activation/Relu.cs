using System;
using BrightData;
using BrightWire.ExecutionGraph.Node;

namespace BrightWire.ExecutionGraph.Activation
{
    /// <summary>
    /// RELu activation
    /// https://en.wikipedia.org/wiki/Rectifier_(neural_networks)
    /// </summary>
    internal class Relu(string? name = null) : NodeBase(name)
    {
        class Backpropagation(Relu source, IMatrix matrix) : SingleBackpropagationBase<Relu>(source)
        {
            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                using var od = matrix.ReluDerivative();
                var delta = errorSignal.GetMatrix().PointwiseMultiply(od);
                return errorSignal.ReplaceWith(delta);
            }
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            var input = signal.GetMatrix();
            var output = signal.ReplaceWith(input.Relu());
            return (this, output, () => new Backpropagation(this, input));
        }
    }
}
