using System;
using BrightData;
using BrightWire.ExecutionGraph.Node;

namespace BrightWire.ExecutionGraph.Activation
{
    /// <summary>
    /// Sigmoid activation function
    /// https://en.wikipedia.org/wiki/Sigmoid_function
    /// </summary>
    internal class Sigmoid(string? name = null) : NodeBase(name)
    {
        class Backpropagation(Sigmoid source, IMatrix<float> matrix) : SingleBackpropagationBase<Sigmoid>(source)
        {
            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                using var od = matrix.SigmoidDerivative();
                var delta = errorSignal.GetMatrix().PointwiseMultiply(od);
                return errorSignal.ReplaceWith(delta);
            }
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            var input = signal.GetMatrix();
            var output = signal.ReplaceWith(input.Sigmoid());
            return (this, output, () => new Backpropagation(this, input));
        }
    }
}
