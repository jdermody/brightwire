using System;
using BrightWire.ExecutionGraph.Helper;
using System.IO;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    /// <summary>
    /// Max pooling convolutional neural network
    /// </summary>
    internal class MaxPool(uint filterWidth, uint filterHeight, uint xStride, uint yStride, string? name = null)
        : NodeBase(name)
    {
        class Backpropagation(MaxPool source, ITensor4D indices, uint inputColumns, uint inputRows, uint outputColumns, uint outputRows, uint depth, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
            : SingleBackpropagationBase<MaxPool>(source)
        {
            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                var errorMatrix = errorSignal.GetMatrix();
                var tensor = errorMatrix.Reshape(null, depth, outputRows, outputColumns);
                var output = tensor.ReverseMaxPool(indices, inputRows, inputColumns, filterWidth, filterHeight, xStride, yStride);
                return new Tensor4DGraphData(output.ReshapeAsMatrix(), output.RowCount, output.ColumnCount, output.Depth);
            }
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            var tensor = signal.GetMatrix().Reshape(null, signal.Depth, signal.Rows, signal.Columns);
            var (output, index) = tensor.MaxPool(filterWidth, filterHeight, xStride, yStride, true);

            var graphData = new Tensor4DGraphData(output);
            return (this, graphData, () => new Backpropagation(this, index!, tensor.ColumnCount, tensor.RowCount, output.ColumnCount, output.RowCount, output.Depth, filterWidth, filterHeight, xStride, yStride));
        }

        protected override (string Description, byte[] Data) GetInfo()
        {
            return ("MAX", WriteData(WriteTo));
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            filterWidth = (uint)reader.ReadInt32();
            filterHeight = (uint)reader.ReadInt32();
            xStride = (uint)reader.ReadInt32();
            yStride = (uint)reader.ReadInt32();
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write((int)filterWidth);
            writer.Write((int)filterHeight);
            writer.Write((int)xStride);
            writer.Write((int)yStride);
        }
    }
}
