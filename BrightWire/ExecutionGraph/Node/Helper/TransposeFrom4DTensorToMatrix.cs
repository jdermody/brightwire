using BrightWire.ExecutionGraph.Helper;

namespace BrightWire.ExecutionGraph.Node.Helper
{
    /// <summary>
    /// Transpose the input - used when translating between tensor and matrix based signals
    /// </summary>
    internal class TransposeFrom4DTensorToMatrix : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<TransposeFrom4DTensorToMatrix>
        {
            readonly IGraphData _shape;

            public Backpropagation(TransposeFrom4DTensorToMatrix source, IGraphData shape) : base(source)
            {
                _shape = shape;
            }

            protected override IGraphData Backpropagate(INode? fromNode, IGraphData errorSignal, IGraphSequenceContext context, INode[] parents)
            {
                var matrix = errorSignal.GetMatrix();
                return _shape.ReplaceWith(matrix.Transpose());
            }

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphSequenceContext context)
            {
                var matrix = errorSignal.GetMatrix();
                return _shape.ReplaceWith(matrix.Transpose());
            }
        }

        public TransposeFrom4DTensorToMatrix(string? name = null) : base(name)
        {
        }

        public override void ExecuteForward(IGraphSequenceContext context)
        {
            var output = context.Data.GetMatrix().Transpose();
            AddNextGraphAction(context, new MatrixGraphData(output), () => new Backpropagation(this, context.Data));
        }
    }
}
