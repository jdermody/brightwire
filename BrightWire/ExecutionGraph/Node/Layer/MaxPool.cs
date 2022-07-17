using System;
using BrightWire.ExecutionGraph.Helper;
using System.IO;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    /// <summary>
    /// Max pooling convolutional neural network
    /// </summary>
    internal class MaxPool : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<MaxPool>
        {
            readonly ITensor4D _indices;
            readonly uint _inputColumns, _inputRows, _outputColumns, _outputRows, _depth, _filterWidth, _filterHeight, _xStride, _yStride;

            public Backpropagation(MaxPool source, ITensor4D indices, uint inputColumns, uint inputRows, uint outputColumns, uint outputRows, uint depth, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
                : base(source)
            {
	            _indices = indices;
                _inputColumns = inputColumns;
                _inputRows = inputRows;
                _outputColumns = outputColumns;
                _outputRows = outputRows;
                _depth = depth;
	            _filterWidth = filterWidth;
				_filterHeight = filterHeight;
				_xStride = xStride;
				_yStride = yStride;
            }

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                var errorMatrix = errorSignal.GetMatrix();
                var tensor = errorMatrix.Reshape(null, _depth, _outputRows, _outputColumns);
                var output = tensor.ReverseMaxPool(_indices, _inputRows, _inputColumns, _filterWidth, _filterHeight, _xStride, _yStride);
                return new Tensor4DGraphData(output.ReshapeAsMatrix(), output.RowCount, output.ColumnCount, output.Depth);
            }
        }
        uint _filterWidth, _filterHeight, _xStride, _yStride;

        public MaxPool(uint filterWidth, uint filterHeight, uint xStride, uint yStride, string? name = null) : base(name)
        {
            _filterWidth = filterWidth;
            _filterHeight = filterHeight;
            _xStride = xStride;
            _yStride = yStride;
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            var tensor = signal.GetMatrix().Reshape(null, signal.Depth, signal.Rows, signal.Columns);
            var (output, index) = tensor.MaxPool(_filterWidth, _filterHeight, _xStride, _yStride, true);

            var graphData = new Tensor4DGraphData(output);
            return (this, graphData, () => new Backpropagation(this, index!, tensor.ColumnCount, tensor.RowCount, output.ColumnCount, output.RowCount, output.Depth, _filterWidth, _filterHeight, _xStride, _yStride));
        }

        protected override (string Description, byte[] Data) GetInfo()
        {
            return ("MAX", WriteData(WriteTo));
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            _filterWidth = (uint)reader.ReadInt32();
            _filterHeight = (uint)reader.ReadInt32();
            _xStride = (uint)reader.ReadInt32();
            _yStride = (uint)reader.ReadInt32();
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write((int)_filterWidth);
            writer.Write((int)_filterHeight);
            writer.Write((int)_xStride);
            writer.Write((int)_yStride);
        }
    }
}
