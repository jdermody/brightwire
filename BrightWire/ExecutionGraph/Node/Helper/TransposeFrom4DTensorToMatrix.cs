using System;

namespace BrightWire.ExecutionGraph.Node.Helper
{
    /// <summary>
    /// Transpose the input - used when translating between tensor and matrix based signals
    /// </summary>
    internal class TransposeFrom4DTensorToMatrix(string? name = null) : NodeBase(name)
    {
        class Backpropagation(TransposeFrom4DTensorToMatrix source, IGraphData shape) : SingleBackpropagationBase<TransposeFrom4DTensorToMatrix>(source)
        {
            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                var matrix = errorSignal.GetMatrix();
                return shape.ReplaceWith(matrix.Transpose());
            }
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            var output = signal.GetMatrix().Transpose();
            return (this, output.AsGraphData(), () => new Backpropagation(this, signal));
        }
    }
}
