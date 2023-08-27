using System;
using System.Collections.Generic;
using System.IO;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    /// <summary>
    /// A feed forward layer with tied weights (from a previous feed forward layer)
    /// </summary>
    internal class TiedFeedForward : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<TiedFeedForward>
        {
            readonly IMatrix _input;

            public Backpropagation(TiedFeedForward source, IMatrix input) : base(source)
            {
                _input = input;
            }

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                var es = errorSignal.GetMatrix();

                // work out the next error signal
                var ret = es.Multiply(_source._layer.Weight);

                // calculate the update to the weights
                var weightUpdate = _input.TransposeThisAndMultiply(es).Transpose();

                // store the updates
                var learningContext = context.LearningContext!;
                learningContext.AddError(NodeErrorType.Bias, _source, es);
                learningContext.AddError(NodeErrorType.Weight, _source, weightUpdate);

                return errorSignal.ReplaceWith(ret);
            }
        }
        IFeedForward _layer;
        IVector _bias;
        string _layerId;

        public TiedFeedForward(IFeedForward layer, IWeightInitialisation weightInit, string? name = null) : base(name)
        {
            _layer = layer;
            _layerId = layer.Id;
            _bias = weightInit.CreateBias(layer.InputSize);
        }

        public override void ApplyError(NodeErrorType type, ITensor delta, ILearningContext context)
        {
            if (type == NodeErrorType.Bias)
                UpdateBias((IMatrix)delta, context);
            else if (type == NodeErrorType.Weight)
                _layer.UpdateWeights((IMatrix)delta, context);
            else {
                throw new NotImplementedException();
            }
        }

        public void UpdateBias(IMatrix delta, ILearningContext context)
        {
            using var columnSums = delta.ColumnSums();
            columnSums.MultiplyInPlace(1f / columnSums.Size);
            _bias.AddInPlace(columnSums, 1f, context.LearningRate);
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            var input = signal.GetMatrix();

            // feed forward
            var output = input.TransposeAndMultiply(_layer.Weight);
            output.AddToEachRow(_bias.Segment);

            // set output
            return (this, signal.ReplaceWith(output), () => new Backpropagation(this, input));
        }

        protected override (string Description, byte[] Data) GetInfo()
        {
            return ("TFF", WriteData(WriteTo));
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            var lap = factory.LinearAlgebraProvider;
            _layerId = reader.ReadString();

            var bias = factory.Context.LoadReadOnlyVectorFrom(reader);
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (_bias == null)
                _bias = bias.Create(lap);
            else
                bias.CopyTo(_bias);
        }

        public override void OnDeserialise(IReadOnlyDictionary<string, NodeBase> graph)
        {
            _layer = (IFeedForward)graph[_layerId];
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(_layerId);
            _bias.WriteTo(writer);
        }
    }
}
