using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    class MaxPool : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<MaxPool>
        {
            readonly IReadOnlyList<(int[] X, int[] Y)> _indexList;
            readonly int _columns, _rows;

            public Backpropagation(MaxPool source, IReadOnlyList<(int[] X, int[] Y)> indexList, int columns, int rows)
                : base(source)
            {
                _indexList = indexList;
                _columns = columns;
                _rows = rows;
            }

            protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                return errorSignal.GetTensor().ReverseMaxPool(_rows, _columns, _indexList).ToGraphData();
            }
        }
        readonly int _width, _height, _stride;

        public MaxPool(int width, int height, int stride, string name = null) : base(name)
        {
            _width = width;
            _height = height;
            _stride = stride;
        }

        public override void ExecuteForward(IContext context)
        {
            var tensor = context.Data.GetTensor();
            (var output, var index) = tensor.MaxPool(_width, _height, _stride);

            var graphData = new TensorGraphData(output);
            _AddNextGraphAction(context, graphData, () => new Backpropagation(this, index, tensor.ColumnCount, tensor.RowCount));
        }
    }
}
