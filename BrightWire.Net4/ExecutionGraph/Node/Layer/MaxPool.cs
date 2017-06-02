using BrightWire.ExecutionGraph.Helper;
using System.Collections.Generic;
using System.IO;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    /// <summary>
    /// Max pooling convolutional neural network
    /// </summary>
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
                var tensor = errorSignal.GetMatrix().ConvertTo4DTensor(_outputRows, _outputColumns, _depth);
                var output = tensor.ReverseMaxPool(_inputRows, _inputColumns, _indexList);
                return new Tensor4DGraphData(output.ConvertToMatrix(), output.RowCount, output.ColumnCount, output.Depth);
            }
        }
        int _filterWidth, _filterHeight, _stride;

        public MaxPool(int filterWidth, int filterHeight, int stride, string name = null) : base(name)
        {
            _filterWidth = filterWidth;
            _filterHeight = filterHeight;
            _stride = stride;
        }

        public override void ExecuteForward(IContext context)
        {
            var input = context.Data;
            var tensor = input.GetMatrix().ConvertTo4DTensor(input.Rows, input.Columns, input.Depth);
            (var output, var index) = tensor.MaxPool(_filterWidth, _filterHeight, _stride, context.IsTraining);

            var graphData = new Tensor4DGraphData(output);
            _AddNextGraphAction(context, graphData, () => new Backpropagation(this, index, tensor.ColumnCount, tensor.RowCount, output.ColumnCount, output.RowCount, output.Depth));
        }

        protected override void _Initalise(GraphFactory factory, string description, byte[] data)
        {
            _ReadFrom(data, reader => ReadFrom(factory, reader));
        }

        protected override (string Description, byte[] Data) _GetInfo()
        {
            return ("MAX", _WriteData(WriteTo));
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            _filterWidth = reader.ReadInt32();
            _filterHeight = reader.ReadInt32();
            _stride = reader.ReadInt32();
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(_filterWidth);
            writer.Write(_filterHeight);
            writer.Write(_stride);
        }
    }
}
