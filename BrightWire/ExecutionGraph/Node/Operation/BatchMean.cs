using System;

namespace BrightWire.ExecutionGraph.Node.Operation
{
    /// <summary>
    /// Calculates the mean across the batch 
    /// </summary>
    internal class BatchMean(string? name = null) : NodeBase(name)
    {
        class Backpropagation(BatchMean source, uint rowCount) : SingleBackpropagationBase<BatchMean>(source)
        {
            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                var es = errorSignal.GetMatrix();
                using var ones = context.GetLinearAlgebraProvider().CreateMatrix(es.RowCount, es.ColumnCount, (_, _) => 1f / rowCount);
                return errorSignal.ReplaceWith(ones.PointwiseMultiply(es));
            }
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            var input = signal.GetMatrix();
            using var columnSums = input.ColumnSums();
            columnSums.MultiplyInPlace(1f / input.RowCount);
            var output = context.GetLinearAlgebraProvider().CreateMatrix(input.RowCount, input.ColumnCount, columnSums.Segment);
            return (this, signal.ReplaceWith(output), () => new Backpropagation(this, input.RowCount));
        }
    }
}
