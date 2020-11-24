using System.IO;
using BrightData;
using BrightData.Distributions;
using BrightData.Helper;

namespace BrightWire.ExecutionGraph.Node.Filter
{
    /// <summary>
    /// Drop out (inverted) regularisation
    /// https://en.wikipedia.org/wiki/Dropout_(neural_networks)
    /// </summary>
    class DropOut : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<DropOut>
        {
            readonly IFloatMatrix _filter;

            public Backpropagation(DropOut source, IFloatMatrix filter) : base(source)
            {
                _filter = filter;
            }

            protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IGraphContext context, INode[] parents)
            {
                var output = errorSignal.GetMatrix().PointwiseMultiply(_filter);
                return errorSignal.ReplaceWith(output);
            }
        }
        float _dropOutPercentage;
        BernoulliDistribution _probabilityToDrop;

        public DropOut(IBrightDataContext context, float dropOutPercentage, string name = null) : base(name)
        {
            _dropOutPercentage = dropOutPercentage;
            _probabilityToDrop = new BernoulliDistribution(context, _dropOutPercentage);
        }

        public override void ExecuteForward(IGraphContext context)
        {
            if (context.IsTraining) {
                // drop out random neurons during training
                var lap = context.LinearAlgebraProvider;
                var matrix = context.Data.GetMatrix();
                var filter = lap.CreateMatrix(matrix.RowCount, matrix.ColumnCount, (i, j) => FloatMath.IsZero(_dropOutPercentage) ? 1f : _probabilityToDrop.Sample() == 1 ? 0f : 1f / _dropOutPercentage);
                var output = matrix.PointwiseMultiply(filter);
                _AddNextGraphAction(context, context.Data.ReplaceWith(output), () => new Backpropagation(this, filter));
            } else
                _AddNextGraphAction(context, context.Data, null);
        }

        protected override (string Description, byte[] Data) _GetInfo()
        {
            return ("DO", _WriteData(WriteTo));
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            _dropOutPercentage = reader.ReadSingle();
            _probabilityToDrop ??= new BernoulliDistribution(factory.Context, _dropOutPercentage);
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(_dropOutPercentage);
        }
    }
}
