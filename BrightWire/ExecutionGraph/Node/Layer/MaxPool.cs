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
            readonly I4DFloatTensor _indices;
            readonly uint _inputColumns, _inputRows, _outputColumns, _outputRows, _depth, _filterWidth, _filterHeight, _xStride, _yStride;

            public Backpropagation(MaxPool source, I4DFloatTensor indices, uint inputColumns, uint inputRows, uint outputColumns, uint outputRows, uint depth, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
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

            protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IGraphContext context, INode[] parents)
            {
	            var errorMatrix = errorSignal.GetMatrix();
                var tensor = errorMatrix.ReshapeAs4DTensor(_outputRows, _outputColumns, _depth);
                var output = tensor.ReverseMaxPool(_indices, _inputRows, _inputColumns, _filterWidth, _filterHeight, _xStride, _yStride);

				//output.AsMatrix().Constrain(-1f, 1f);

//#if DEBUG
//				Debug.Assert(output.ReshapeAsVector().IsEntirelyFinite());
//#endif

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

        public override void ExecuteForward(IGraphContext context)
        {
            var input = context.Data;
            var tensor = input.GetMatrix().ReshapeAs4DTensor(input.Rows, input.Columns, input.Depth);
            var (output, index) = tensor.MaxPool(_filterWidth, _filterHeight, _xStride, _yStride, true);

//#if DEBUG
//			Debug.Assert(output.ReshapeAsVector().IsEntirelyFinite());
//			Debug.Assert(index.ReshapeAsVector().IsEntirelyFinite());
//#endif

			var graphData = new Tensor4DGraphData(output);
            _AddNextGraphAction(context, graphData, () => new Backpropagation(this, index, tensor.ColumnCount, tensor.RowCount, output.ColumnCount, output.RowCount, output.Depth, _filterWidth, _filterHeight, _xStride, _yStride));
        }

        protected override (string Description, byte[] Data) _GetInfo()
        {
            return ("MAX", _WriteData(WriteTo));
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
