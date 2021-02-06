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
            readonly IFloatMatrix _input;

            public Backpropagation(TiedFeedForward source, IFloatMatrix input) : base(source)
            {
                _input = input;
            }

            protected override IGraphData Backpropagate(INode? fromNode, IGraphData errorSignal, IGraphContext context, INode[] parents)
            {
                var es = errorSignal.GetMatrix();

                // work out the next error signal
                var ret = es.Multiply(_source._layer.Weight);

                // calculate the update to the weights
                var weightUpdate = _input.TransposeThisAndMultiply(es).Transpose();

                // store the updates
                var learningContext = context.LearningContext!;
                learningContext.StoreUpdate(_source, es, err => _source.UpdateBias(err, learningContext));
                learningContext.StoreUpdate(_source, weightUpdate, err => _source._layer.UpdateWeights(err, learningContext));

                return errorSignal.ReplaceWith(ret);
            }
        }
        IFeedForward _layer;
        IFloatVector _bias;
        string _layerId;

        public TiedFeedForward(IFeedForward layer, IWeightInitialisation weightInit, string? name = null) : base(name)
        {
            _layer = layer;
            _layerId = layer.Id;
            _bias = weightInit.CreateBias(layer.InputSize);
        }

        public void UpdateBias(IFloatMatrix delta, ILearningContext context)
        {
            using var columnSums = delta.ColumnSums();
            _bias.AddInPlace(columnSums, 1f / columnSums.Count, context.BatchLearningRate);
        }

        public override void ExecuteForward(IGraphContext context)
        {
            var input = context.Data.GetMatrix();

            // feed forward
            var output = input.TransposeAndMultiply(_layer.Weight);
            output.AddToEachRow(_bias);

            // set output
            AddNextGraphAction(context, context.Data.ReplaceWith(output), () => new Backpropagation(this, input));
        }

        protected override (string Description, byte[] Data) GetInfo()
        {
            return ("TFF", WriteData(WriteTo));
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            _layerId = reader.ReadString();

            var lap = factory.LinearAlgebraProvider;
            var bias = factory.Context.ReadVectorFrom(reader);
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (_bias == null)
                _bias = lap.CreateVector(bias);
            else
                _bias.Data = bias;
        }

        public override void OnDeserialise(IReadOnlyDictionary<string, INode> graph)
        {
            _layer = (IFeedForward)graph[_layerId];
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(_layerId);
            _bias.Data.WriteTo(writer);
        }
    }
}
