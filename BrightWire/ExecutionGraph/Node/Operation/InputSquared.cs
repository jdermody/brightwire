using System;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Operation
{
    internal class InputSquared(string? name = null) : NodeBase(name)
    {
        class Backpropagation(InputSquared source, IMatrix<float> input) : SingleBackpropagationBase<InputSquared>(source)
        {
            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                var es = errorSignal.GetMatrix();
                var err = es.PointwiseMultiply(input);
                err.MultiplyInPlace(2f);
                return errorSignal.ReplaceWith(err);
            }
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            var input = signal.GetMatrix();
            var inputSquared = input.PointwiseMultiply(input);
            return (this, signal.ReplaceWith(inputSquared), () => new Backpropagation(this, input));
        }
    }
}
