﻿using System;
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
        class Backpropagation(DropOut source, IMatrix<float> filter) : SingleBackpropagationBase<DropOut>(source)
        {
            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                var output = errorSignal.GetMatrix().PointwiseMultiply(filter);
                return errorSignal.ReplaceWith(output);
            }
        }
        float _dropOutPercentage;
        INonNegativeDiscreteDistribution? _probabilityToDrop;

        public DropOut(BrightDataContext context, float dropOutPercentage, string? name = null) : base(name)
        {
            _dropOutPercentage = dropOutPercentage;
            _probabilityToDrop = context.CreateBernoulliDistribution(_dropOutPercentage);
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            if (context.LearningContext != null) {
                // drop out random neurons during training
                var lap = context.GetLinearAlgebraProvider();
                var matrix = signal.GetMatrix();
                Func<uint, uint, float> sample = Math<float>.IsZero(_dropOutPercentage)
                    ? (_, _) => 1f
                    : (_, _) => _probabilityToDrop!.Sample() == 1 ? 0f : 1f / _dropOutPercentage
                ;
                var filter = lap.CreateMatrix(matrix.RowCount, matrix.ColumnCount, sample);
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
