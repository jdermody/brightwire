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
            readonly IReadOnlyList<IReadOnlyList<(object X, object Y)>> _indexList;
            readonly int _inputColumns, _inputRows, _outputColumns, _outputRows, _depth;

            public Backpropagation(MaxPool source, IReadOnlyList<IReadOnlyList<(object X, object Y)>> indexList, int inputColumns, int inputRows, int outputColumns, int outputRows, int depth)
                : base(source)
            {
                _indexList = indexList;
                _inputColumns = inputColumns;
                _inputRows = inputRows;
                _outputColumns = outputColumns;
                _outputRows = outputRows;
                _depth = depth;
            }

            protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                var lap = context.LinearAlgebraProvider;
                var tensor = lap.Create4DTensor(errorSignal.GetMatrix(), _outputRows, _outputColumns, _depth);
                var output = tensor.ReverseMaxPool(_inputRows, _inputColumns, _indexList);
                return new Tensor4DGraphData(output.ConvertToMatrix(), output.RowCount, output.ColumnCount, output.Depth);
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
            var input = context.Data;
            var lap = context.LinearAlgebraProvider;
            var tensor = lap.Create4DTensor(input.GetMatrix(), input.Rows, input.Columns, input.Depth);

            (var output, var index) = tensor.MaxPool(_width, _height, _stride, context.IsTraining);

            var graphData = new Tensor4DGraphData(output);
            _AddNextGraphAction(context, graphData, () => new Backpropagation(this, index, tensor.ColumnCount, tensor.RowCount, output.ColumnCount, output.RowCount, output.Depth));
        }
    }
}
