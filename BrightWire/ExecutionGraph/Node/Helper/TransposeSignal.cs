using BrightWire.ExecutionGraph.Helper;

namespace BrightWire.ExecutionGraph.Node.Helper
{
    /// <summary>
    /// Transpose the input - used when translating between tensor and matrix based signals
    /// </summary>
    internal class TransposeSignal : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<TransposeSignal>
        {
            public Backpropagation(TransposeSignal source) : base(source)
            {
            }

            protected override IGraphData Backpropagate(INode? fromNode, IGraphData errorSignal, IGraphSequenceContext context, INode[] parents)
            {
                var matrix = errorSignal.GetMatrix();
                return errorSignal.ReplaceWith(matrix.Transpose());
            }

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphSequenceContext context)
            {
                var matrix = errorSignal.GetMatrix();
                return errorSignal.ReplaceWith(matrix.Transpose());
            }
        }

        public TransposeSignal(string? name = null) : base(name)
        {
        }

        public override void ExecuteForward(IGraphSequenceContext context)
        {
            var output = context.Data.GetMatrix().Transpose();
            AddNextGraphAction(context, new MatrixGraphData(output), () => new Backpropagation(this));
        }
    }
}
