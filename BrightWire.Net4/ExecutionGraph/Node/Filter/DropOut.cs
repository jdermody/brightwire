using BrightWire.ExecutionGraph.Helper;
using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Filter
{
    class DropOut : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<DropOut>
        {
            readonly IReadOnlyList<IMatrix> _filter;

            public Backpropagation(DropOut source, IReadOnlyList<IMatrix> filter) : base(source)
            {
                _filter = filter;
            }

            protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                return context.ToGraphData(errorSignal.Decompose().Zip(_filter, (e, f) => e.PointwiseMultiply(f)));
            }
        }
        float _dropOutPercentage;
        Bernoulli _probabilityToDrop;

        public DropOut(float dropOutPercentage, string name = null) : base(name)
        {
            _dropOutPercentage = dropOutPercentage;
            _probabilityToDrop = new Bernoulli(_dropOutPercentage);
        }

        public override void ExecuteForward(IContext context)
        {
            if (context.IsTraining) {
                // drop out random neurons during training
                var lap = context.LinearAlgebraProvider;
                var filterList = new List<IMatrix>();
                var outputList = new List<IMatrix>();
                foreach (var matrix in context.Data.Decompose()) {
                    var filter = lap.CreateMatrix(matrix.RowCount, matrix.ColumnCount, (i, j) => _probabilityToDrop.Sample() == 1 ? 0f : 1f);
                    outputList.Add(matrix.PointwiseMultiply(filter));
                    filterList.Add(filter);
                }
                _AddNextGraphAction(context, context.ToGraphData(outputList), () => new Backpropagation(this, filterList));
            } else {
                // otherwise scale by the drop out percentage
                var scaleFactor = 1 - _dropOutPercentage;
                var matrixList = context.Data.Decompose();
                foreach (var matrix in matrixList)
                    matrix.Multiply(scaleFactor);
                var scaled = context.ToGraphData(matrixList);
                _AddNextGraphAction(context, scaled, null);
            }
        }

        protected override void _Initalise(GraphFactory factory, string description, byte[] data)
        {
            _ReadFrom(data, reader => ReadFrom(factory, reader));
        }

        protected override (string Description, byte[] Data) _GetInfo()
        {
            return ("DO", _WriteData(WriteTo));
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            _dropOutPercentage = reader.ReadSingle();
            _probabilityToDrop = new Bernoulli(_dropOutPercentage);
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(_dropOutPercentage);
        }
    }
}
