using BrightWire.ExecutionGraph.Helper;
using BrightWire.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    /// <summary>
    /// A feed forward layer with tied weights (from a previous feed forward layer)
    /// </summary>
    class TiedFeedForward : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<TiedFeedForward>
        {
            readonly IMatrix _input = null;

            public Backpropagation(TiedFeedForward source, IMatrix input) : base(source)
            {
                _input = input;
            }

            protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                var es = errorSignal.GetMatrix();

                // work out the next error signal
                IMatrix ret = es.Multiply(_source._layer.Weight);

                // calculate the update to the weights
                var weightUpdate = _input.TransposeThisAndMultiply(es).Transpose();

                // store the updates
                var learningContext = context.LearningContext;
                learningContext.StoreUpdate(_source, es, err => _source.UpdateBias(err, learningContext));
                learningContext.StoreUpdate(_source, weightUpdate, err => _source._layer.UpdateWeights(err, learningContext));

                return errorSignal.ReplaceWith(ret);
            }
        }
        IFeedForward _layer;
        IVector _bias;
        string _layerId;

        public TiedFeedForward(IFeedForward layer, IWeightInitialisation weightInit, string name = null) : base(name)
        {
            _layer = layer;
            _layerId = layer.Id;
            _bias = weightInit.CreateBias(layer.InputSize);
        }

        public void UpdateBias(IMatrix delta, ILearningContext context)
        {
            using (var columnSums = delta.ColumnSums())
                _bias.AddInPlace(columnSums, 1f / columnSums.Count, context.BatchLearningRate);
        }

        public override void ExecuteForward(IContext context)
        {
            var input = context.Data.GetMatrix();

            // feed forward
            var output = input.TransposeAndMultiply(_layer.Weight);
            output.AddToEachRow(_bias);

            // set output
            _AddNextGraphAction(context, context.Data.ReplaceWith(output), () => new Backpropagation(this, input));
        }

        protected override (string Description, byte[] Data) _GetInfo()
        {
            return ("TFF", _WriteData(WriteTo));
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            _layerId = reader.ReadString();

            var lap = factory?.LinearAlgebraProvider;
            var bias = FloatVector.ReadFrom(reader);
            if (_bias == null)
                _bias = lap.CreateVector(bias);
            else
                _bias.Data = bias;
        }

        public override void OnDeserialise(IReadOnlyDictionary<string, INode> graph)
        {
            _layer = graph[_layerId] as IFeedForward;
            Debug.Assert(_layer != null);
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(_layerId);
            _bias.Data.WriteTo(writer);
        }
    }
}
