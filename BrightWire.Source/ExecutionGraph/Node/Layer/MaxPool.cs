using BrightWire.ExecutionGraph.Helper;
using System.Collections.Generic;
using System.Diagnostics;
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
            readonly I4DTensor _indices;
            readonly int _inputColumns, _inputRows, _outputColumns, _outputRows, _depth, _filterWidth, _filterHeight, _stride;

            public Backpropagation(MaxPool source, I4DTensor indices, int inputColumns, int inputRows, int outputColumns, int outputRows, int depth, int filterWidth, int filterHeight, int stride)
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
				_stride = stride;
            }

            protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
	            var errorMatrix = errorSignal.GetMatrix();
                var tensor = errorMatrix.As4DTensor(_outputRows, _outputColumns, _depth);
                var output = tensor.ReverseMaxPool(_indices, _inputRows, _inputColumns, _filterWidth, _filterHeight, _stride);

	            //output.AsMatrix().Constrain(-1f, 1f);

//#if DEBUG
//	            Debug.Assert(output.AsVector().IsEntirelyFinite());
//#endif

                return new Tensor4DGraphData(output.AsMatrix(), output.RowCount, output.ColumnCount, output.Depth);
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
            var tensor = input.GetMatrix().As4DTensor(input.Rows, input.Columns, input.Depth);
            var (output, index) = tensor.MaxPool(_filterWidth, _filterHeight, _stride, true);

			//#if DEBUG
	  //      Debug.Assert(output.AsVector().IsEntirelyFinite());
	  //      Debug.Assert(index.AsVector().IsEntirelyFinite());
			//#endif

            var graphData = new Tensor4DGraphData(output);
            _AddNextGraphAction(context, graphData, () => new Backpropagation(this, index, tensor.ColumnCount, tensor.RowCount, output.ColumnCount, output.RowCount, output.Depth, _filterWidth, _filterHeight, _stride));
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
