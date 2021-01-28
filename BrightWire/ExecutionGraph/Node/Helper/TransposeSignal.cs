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

            protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IGraphContext context, INode[] parents)
            {
                var matrix = errorSignal.GetMatrix();
                return errorSignal.ReplaceWith(matrix.Transpose());
            }
        }

        public TransposeSignal(string? name = null) : base(name)
        {
        }

        public override void ExecuteForward(IGraphContext context)
        {
            var output = context.Data.GetMatrix().Transpose();
            _AddNextGraphAction(context, new MatrixGraphData(output), () => new Backpropagation(this));
        }
    }
}
