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

        public override (IGraphData Next, Func<IBackpropagate>? BackProp) Forward(IGraphData signal, uint channel, IGraphSequenceContext context, INode? source)
        {
            var input = signal.GetMatrix();
            using var columnSums = input.ColumnSums();
            columnSums.Multiply(1f / input.RowCount);
            var mean = columnSums.AsIndexable();

            var output = context.LinearAlgebraProvider.CreateMatrix(input.RowCount, input.ColumnCount, (i, j) => mean[j]);
            return (signal.ReplaceWith(output), () => new Backpropagation(this, input.RowCount));
        }
    }
}
