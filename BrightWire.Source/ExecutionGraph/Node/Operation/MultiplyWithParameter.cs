using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Operation
{
    class MultiplyWithParameter : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<MultiplyWithParameter>
        {
            public Backpropagation(MultiplyWithParameter source) : base(source)
            {
            }

            protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                var error = errorSignal.GetMatrix();
                using (var columnSums = error.ColumnSums()) {
                    columnSums.Multiply(1f / error.RowCount);
                    _source._param.AddInPlace(columnSums, 1f, context.LearningContext.BatchLearningRate);
                }
                return errorSignal;
            }
        }
        IVector _param;

        public MultiplyWithParameter(IVector param, string name = null) : base(name)
        {
            _param = param;
        }

        public IVector Parameter
        {
            get => _param;
            set => _param = value;
        }

        public override void ExecuteForward(IContext context)
        {
            var input = context.Data.GetMatrix();
            var matrix = context.LinearAlgebraProvider.CreateMatrix(Enumerable.Repeat(_param, input.RowCount).ToList());
            _AddNextGraphAction(context, context.Data.ReplaceWith(matrix), () => new Backpropagation(this));
        }
    }
}
