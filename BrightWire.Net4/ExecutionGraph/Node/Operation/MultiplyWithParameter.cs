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
                var delta = error.ColumnSums();
                delta.Multiply(1f / error.RowCount);
                var delta2 = delta.Average();
                _source._param += delta2 * context.LearningContext.LearningRate;
                return errorSignal;
            }
        }
        float _param;

        public MultiplyWithParameter(float param, string name = null) : base(name)
        {
            _param = param;
        }

        public float Parameter
        {
            get => _param;
            set => _param = value;
        }

        public override void ExecuteForward(IContext context)
        {
            var input = context.Data.GetMatrix();
            var output = context.LinearAlgebraProvider.CreateMatrix(input.RowCount, input.ColumnCount, _param);
            _AddNextGraphAction(context, context.Data.ReplaceWith(output), () => new Backpropagation(this));
        }
    }
}
