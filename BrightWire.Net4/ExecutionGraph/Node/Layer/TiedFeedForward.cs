using BrightWire.ExecutionGraph.Helper;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    class TiedFeedForward : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<TiedFeedForward>
        {
            readonly IMatrix _input = null;

            public Backpropagation(TiedFeedForward source, IMatrix input) : base(source)
            {
                _input = input;
            }

            protected override void _Dispose(bool isDisposing)
            {
                //_input.Dispose();
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
                learningContext.Store(es, err => _source.UpdateBias(err, learningContext));
                learningContext.Store(weightUpdate, err => _source._layer.UpdateWeights(err, learningContext));

                return ret.ToGraphData();
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

        protected override void _Initalise(GraphFactory factory, string description, byte[] data)
        {
            _ReadFrom(data, reader => ReadFrom(factory, reader));
        }

        public void UpdateBias(IMatrix delta, ILearningContext context)
        {
            using (var columnSums = delta.ColumnSums())
                _bias.AddInPlace(columnSums, 1f / columnSums.Count, context.LearningRate);
        }

        public override void ExecuteForward(IContext context)
        {
            var input = context.Data.GetMatrix();

            // feed forward
            var output = input.TransposeAndMultiply(_layer.Weight);
            output.AddToEachRow(_bias);

            // set output
            _AddNextGraphAction(context, new MatrixGraphData(output), () => new Backpropagation(this, input));
        }

        protected override (string Description, byte[] Data) _GetInfo()
        {
            return ("TFF", _WriteData(WriteTo));
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            _layerId = reader.ReadString();

            var lap = factory.LinearAlgebraProvider;
            _bias = lap.CreateVector(FloatVector.ReadFrom(reader));
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
            //writer.Write(_updater);
        }
    }
}
