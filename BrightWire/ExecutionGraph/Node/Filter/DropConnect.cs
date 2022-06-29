using System;
using BrightWire.ExecutionGraph.Node.Layer;
using System.IO;
using BrightData;
using BrightData.Helper;

namespace BrightWire.ExecutionGraph.Node.Filter
{
    /// <summary>
    /// Drop connect (inverted) regularisation
    /// http://cs.nyu.edu/~wanli/dropc/
    /// </summary>
    internal class DropConnect : FeedForward
    {
        new class Backpropagation : SingleBackpropagationBase<DropConnect>
        {
            readonly IMatrix _input, _filter, _filteredWeights;

            public Backpropagation(DropConnect source, IMatrix input, IMatrix filter, IMatrix filteredWeights) : base(source)
            {
                _input = input;
                _filter = filter;
                _filteredWeights = filteredWeights;
            }

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphSequenceContext context)
            {
                var es = errorSignal.GetMatrix();

                // work out the next error signal against the filtered weights
                IMatrix ret = es.TransposeAndMultiply(_filteredWeights);

                // calculate the update to the weights and filter out the dropped connections
                var weightUpdate = _input.TransposeThisAndMultiply(es).PointwiseMultiply(_filter);

                // store the updates
                var learningContext = context.LearningContext!;
                learningContext.StoreUpdate(_source, es, err => _source.UpdateBias(err, learningContext));
                learningContext.StoreUpdate(_source, weightUpdate, err => _source.UpdateWeights(err, learningContext));

                return errorSignal.ReplaceWith(ret);
            }
        }
        float _dropOutPercentage;
        INonNegativeDiscreteDistribution? _probabilityToDrop;

        public DropConnect(BrightDataContext context, float dropOutPercentage, uint inputSize, uint outputSize, IVector bias, IMatrix weight, IGradientDescentOptimisation updater, string? name = null) 
            : base(inputSize, outputSize, bias, weight, updater, name)
        {
            _dropOutPercentage = dropOutPercentage;
            _probabilityToDrop = context.CreateBernoulliDistribution(_dropOutPercentage);
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphSequenceContext context, NodeBase? source)
        {
            if (context.LearningContext != null) {
                var lap = context.LinearAlgebraProvider;
                var input = signal;
                var inputMatrix = input.GetMatrix();
                var filter = lap.CreateMatrix(Weight.RowCount, Weight.ColumnCount, (_, _) => FloatMath.IsZero(_dropOutPercentage) ? 1f : _probabilityToDrop!.Sample() == 1 ? 0f : 1f / _dropOutPercentage);
                var filteredWeights = Weight.PointwiseMultiply(filter);
                var output = FeedForwardInternal(inputMatrix, filteredWeights);
                return (this, input.ReplaceWith(output), () => new Backpropagation(this, inputMatrix, filter, filteredWeights));
            }
            return base.ForwardSingleStep(signal, channel, context, source);
        }

        protected override (string Description, byte[] Data) GetInfo()
        {
            return ("DC", WriteData(WriteTo));
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            base.ReadFrom(factory, reader);
            _dropOutPercentage = reader.ReadSingle();
            _probabilityToDrop ??= factory.Context.CreateBernoulliDistribution(_dropOutPercentage);
        }

        public override void WriteTo(BinaryWriter writer)
        {
            base.WriteTo(writer);
            writer.Write(_dropOutPercentage);
        }
    }
}
