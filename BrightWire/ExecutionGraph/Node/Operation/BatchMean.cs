using BrightData;

namespace BrightWire.ExecutionGraph.Node.Operation
{
    /// <summary>
    /// Calculates the mean across the batch 
    /// </summary>
    internal class BatchMean : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<BatchMean>
        {
            readonly uint _rowCount;

            public Backpropagation(BatchMean source, uint rowCount) : base(source)
            {
                _rowCount = rowCount;
            }

            protected override IGraphData Backpropagate(INode? fromNode, IGraphData errorSignal, IGraphSequenceContext context, INode[] parents)
            {
                var es = errorSignal.GetMatrix();
                using var ones = context.LinearAlgebraProvider.CreateMatrix(es.RowCount, es.ColumnCount, 1f / _rowCount);
                return errorSignal.ReplaceWith(ones.PointwiseMultiply(es));
            }
        }

        public BatchMean(string? name = null) : base(name)
        {
        }

        public override void ExecuteForward(IGraphSequenceContext context)
        {
            var input = context.Data.GetMatrix();
            using var columnSums = input.ColumnSums();
            columnSums.Multiply(1f / input.RowCount);
            var mean = columnSums.AsIndexable();

            var output = context.LinearAlgebraProvider.CreateMatrix(input.RowCount, input.ColumnCount, (i, j) => mean[j]);
            AddNextGraphAction(context, context.Data.ReplaceWith(output), () => new Backpropagation(this, input.RowCount));
        }
    }
}
