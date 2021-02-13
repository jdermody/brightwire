using System;
using System.IO;
using BrightData;
using BrightData.Helper;

namespace BrightWire.ExecutionGraph.Node.Filter
{
    /// <summary>
    /// Drop out (inverted) regularisation
    /// https://en.wikipedia.org/wiki/Dropout_(neural_networks)
    /// </summary>
    internal class DropOut : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<DropOut>
        {
            readonly IFloatMatrix _filter;

            public Backpropagation(DropOut source, IFloatMatrix filter) : base(source)
            {
                _filter = filter;
            }

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphSequenceContext context)
            {
                var output = errorSignal.GetMatrix().PointwiseMultiply(_filter);
                return errorSignal.ReplaceWith(output);
            }
        }
        float _dropOutPercentage;
        INonNegativeDiscreteDistribution? _probabilityToDrop;

        public DropOut(IBrightDataContext context, float dropOutPercentage, string? name = null) : base(name)
        {
            _dropOutPercentage = dropOutPercentage;
            _probabilityToDrop = context.CreateBernoulliDistribution(_dropOutPercentage);
        }

        public override void ExecuteForward(IGraphSequenceContext context)
        {
            if (context.LearningContext != null) {
                // drop out random neurons during training
                var lap = context.LinearAlgebraProvider;
                var matrix = context.Data.GetMatrix();
                var filter = lap.CreateMatrix(matrix.RowCount, matrix.ColumnCount, (i, j) => FloatMath.IsZero(_dropOutPercentage) ? 1f : _probabilityToDrop!.Sample() == 1 ? 0f : 1f / _dropOutPercentage);
                var output = matrix.PointwiseMultiply(filter);
                AddNextGraphAction(context, context.Data.ReplaceWith(output), () => new Backpropagation(this, filter));
            } else
                AddNextGraphAction(context, context.Data, null);
        }

        public override (INode FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) Forward(IGraphData signal, uint channel, IGraphSequenceContext context, INode? source)
        {
            if (context.LearningContext != null) {
                // drop out random neurons during training
                var lap = context.LinearAlgebraProvider;
                var matrix = signal.GetMatrix();
                var filter = lap.CreateMatrix(matrix.RowCount, matrix.ColumnCount, (i, j) => FloatMath.IsZero(_dropOutPercentage) ? 1f : _probabilityToDrop!.Sample() == 1 ? 0f : 1f / _dropOutPercentage);
                var output = matrix.PointwiseMultiply(filter);
                return (this, signal.ReplaceWith(output), () => new Backpropagation(this, filter));
            }
            return (this, signal, null);
        }

        protected override (string Description, byte[] Data) GetInfo()
        {
            return ("DO", WriteData(WriteTo));
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            _dropOutPercentage = reader.ReadSingle();
            _probabilityToDrop ??= factory.Context.CreateBernoulliDistribution(_dropOutPercentage);
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(_dropOutPercentage);
        }
    }
}
