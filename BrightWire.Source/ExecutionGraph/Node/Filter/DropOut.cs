using BrightWire.ExecutionGraph.Helper;
using MathNet.Numerics.Distributions;
using System.Collections.Generic;
using System.IO;

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
            readonly IMatrix _filter;

            public Backpropagation(DropOut source, IMatrix filter) : base(source)
            {
                _filter = filter;
            }

            protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                var output = errorSignal.GetMatrix().PointwiseMultiply(_filter);
                return errorSignal.ReplaceWith(output);
            }
        }
        float _dropOutPercentage;
        Bernoulli _probabilityToDrop;

        public DropOut(float dropOutPercentage, bool stochastic, string name = null) : base(name)
        {
            _dropOutPercentage = dropOutPercentage;
            _probabilityToDrop = stochastic ? new Bernoulli(_dropOutPercentage) : new Bernoulli(_dropOutPercentage, new System.Random(0));
        }

        public override void ExecuteForward(IContext context)
        {
            if (context.IsTraining) {
                // drop out random neurons during training
                var lap = context.LinearAlgebraProvider;
                var matrix = context.Data.GetMatrix();
                var filter = lap.CreateMatrix(matrix.RowCount, matrix.ColumnCount, (i, j) => _probabilityToDrop.Sample() == 1 ? 0f : 1f / _dropOutPercentage);
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
            if(_probabilityToDrop == null)
                _probabilityToDrop = new Bernoulli(_dropOutPercentage);
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(_dropOutPercentage);
        }
    }
}
