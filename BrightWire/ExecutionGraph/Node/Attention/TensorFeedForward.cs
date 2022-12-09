using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Attention
{
    /// <summary>
    /// Applies a feed forward node to each matrix within a tensor
    /// </summary>
    internal class TensorFeedForward : NodeBase
    {
        readonly IFeedForward _feedForward;

        class Backpropagation : SingleBackpropagationBase<TensorFeedForward>
        {
            readonly Func<IBackpropagate>?[] _backProp;

            public Backpropagation(Func<IBackpropagate>?[] backProp, TensorFeedForward source) : base(source)
            {
                _backProp = backProp;
            }

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                var errorTensor = errorSignal.Get3DTensor();
                var depth = errorTensor!.Depth;
                var output = new IMatrix[depth];

                for (uint i = 0; i < depth; i++) {
                    var backProp = _backProp[i]!();
                    using var matrix = errorTensor.GetMatrix(i);
                    var (signal, _, toNode) = backProp.Backward(matrix.AsGraphData(), context, new NodeBase[] { _source }).Single();
                    output[i] = signal.GetMatrix();
                }

                var lap = context.GetLinearAlgebraProvider();
                var outputTensor = lap.CreateTensor3D(output);
                return outputTensor.AsGraphData();
            }
        }

        public TensorFeedForward(IFeedForward feedForward, string? name, string? id = null) : base(name, id)
        {
            _feedForward = feedForward;
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            var tensor = signal.Get3DTensor();
            var depth = tensor!.Depth;
            var output = new IMatrix[depth];
            Func<IBackpropagate>?[]? backProp = null;

            for (uint i = 0; i < depth; i++) {
                using var matrix = tensor.GetMatrix(i);
                var (_, graphData, bp) = _feedForward.ForwardSingleStep(matrix.AsGraphData(), 0, context, this);
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
