using System;
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

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphSequenceContext context)
            {
                var es = errorSignal.GetMatrix();
                using var ones = context.GetLinearAlgebraProvider().CreateMatrix(es.RowCount, es.ColumnCount, (_, _) => 1f / _rowCount);
                return errorSignal.ReplaceWith(ones.PointwiseMultiply(es));
            }
        }

        public BatchMean(string? name = null) : base(name)
        {
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphSequenceContext context, NodeBase? source)
        {
            var input = signal.GetMatrix();
            using var columnSums = input.ColumnSums();
            columnSums.Multiply(1f / input.RowCount);
            var mean = columnSums.Segment.GetLocalOrNewArray();

            var output = context.GetLinearAlgebraProvider().CreateMatrix(input.RowCount, input.ColumnCount, (_, j) => mean[j]);
            return (this, signal.ReplaceWith(output), () => new Backpropagation(this, input.RowCount));
        }
    }
}
