using System;
using System.Linq;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Attention
{
    /// <summary>
    /// Applies a feed forward node to each matrix within a tensor
    /// </summary>
    internal class TensorFeedForward(IFeedForward feedForward, string? name, string? id = null) : NodeBase(name, id)
    {
        class Backpropagation(Func<IBackpropagate>?[] prop, TensorFeedForward source) : SingleBackpropagationBase<TensorFeedForward>(source)
        {
            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                var errorTensor = errorSignal.Get3DTensor();
                var depth = errorTensor!.Depth;
                var output = new IMatrix<float>[depth];

                for (uint i = 0; i < depth; i++) {
                    var backProp = prop[i]!();
                    using var matrix = errorTensor.GetMatrix(i);
                    var (signal, _, toNode) = backProp.Backward(matrix.AsGraphData(), context, [_source]).Single();
                    output[i] = signal.GetMatrix();
                }

                var lap = context.GetLinearAlgebraProvider();
                var outputTensor = lap.CreateTensor3D(output);
                return outputTensor.AsGraphData();
            }
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            var tensor = signal.Get3DTensor();
            var depth = tensor!.Depth;
            var output = new IMatrix<float>[depth];
            Func<IBackpropagate>?[]? backProp = null;

            for (uint i = 0; i < depth; i++) {
                using var matrix = tensor.GetMatrix(i);
                var (_, graphData, bp) = feedForward.ForwardSingleStep(matrix.AsGraphData(), 0, context, this);
                output[i] = graphData.GetMatrix();
                if (bp is not null)
                    (backProp ??= new Func<IBackpropagate>?[depth])[i] = bp;
            }
            
            var lap = context.GetLinearAlgebraProvider();
            var outputTensor = lap.CreateTensor3D(output);
            return (this, outputTensor.AsGraphData(), backProp is not null ? () => new Backpropagation(backProp, this) : null);
        }
    }
}
