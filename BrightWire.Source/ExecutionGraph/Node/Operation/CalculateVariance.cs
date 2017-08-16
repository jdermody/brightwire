using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Operation
{
    /// <summary>
    /// Calculates the variance 
    /// </summary>
    class CalculateVariance : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<CalculateVariance>
        {
            readonly int _rowCount;

            public Backpropagation(CalculateVariance source, int rowCount) : base(source)
            {
                _rowCount = rowCount;
            }

            protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                var es = errorSignal.GetMatrix();
                using (var ones = context.LinearAlgebraProvider.CreateMatrix(es.RowCount, es.ColumnCount, 1f / _rowCount))
                    return errorSignal.ReplaceWith(ones.PointwiseMultiply(es));
            }
        }
        public CalculateVariance(string name = null) : base(name)
        {
        }

        public override void ExecuteForward(IContext context)
        {
            var input = context.Data.GetMatrix();
            using (var columnSums = input.ColumnSums()) {
                columnSums.Multiply(1f / input.RowCount);
                var mean = columnSums.AsIndexable();

                var output = context.LinearAlgebraProvider.CreateMatrix(input.RowCount, input.ColumnCount, (i, j) => mean[j]);
                _AddNextGraphAction(context, context.Data.ReplaceWith(output), () => new Backpropagation(this, input.RowCount));
            }
        }
    }
}
