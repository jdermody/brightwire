using BrightWire.ExecutionGraph.Helper;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    class FeedForward : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase
        {
            readonly FeedForward _layer;
            readonly IMatrix _input = null;

            public Backpropagation(FeedForward layer, IMatrix input)
            {
                _layer = layer;
                _input = input;
            }

            protected override void _Dispose(bool isDisposing)
            {
                _input.Dispose();
            }

            protected override IGraphData _Backward(IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                var es = errorSignal.GetMatrix();

                // work out the next error signal
                IMatrix ret = es.TransposeAndMultiply(_layer._weight);

                // calculate the update to the weights
                var weightUpdate = _input.TransposeThisAndMultiply(es);

                // store the updates
                var learningContext = context.LearningContext;
                learningContext.Store(es, err => _UpdateBias(err, learningContext));
                learningContext.Store(weightUpdate, err => _layer.Update(err, learningContext));

                // log the backpropagation
                //learningContext.Log("feed-forward-backpropagation", channel, _layer.GetHashCode(), errorSignal, ret, writer => {
                //    if (learningContext.LogMatrixValues) {
                //        writer.WriteStartElement("bias-update");
                //        writer.WriteRaw(errorSignal.AsIndexable().AsXml);
                //        writer.WriteEndElement();

                //        writer.WriteStartElement("weight-update");
                //        writer.WriteRaw(weightUpdate.AsIndexable().AsXml);
                //        writer.WriteEndElement();
                //    }
                //});

                return ret.ToGraphData();
            }

            void _UpdateBias(IMatrix delta, ILearningContext context)
            {
                using (var columnSums = delta.ColumnSums())
                    _layer._bias.AddInPlace(columnSums, 1f / columnSums.Count, context.LearningRate);
            }
        }

        IVector _bias;
        IMatrix _weight;
        readonly IGradientDescentOptimisation _updater;

        public FeedForward(IVector bias, IMatrix weight, IGradientDescentOptimisation updater, string name = null) : base(name)
        {
            _bias = bias;
            _weight = weight;
            _updater = updater;
        }

        protected override void _Initalise(GraphFactory factory, string description, byte[] data)
        {
            _ReadFrom(data, reader => ReadFrom(factory, reader));
        }

        protected override void _Dispose(bool isDisposing)
        {
            _bias.Dispose();
            _weight.Dispose();
        }

        public void Update(IMatrix delta, ILearningContext context)
        {
            _updater.Update(_weight, delta, context);
        }

        public override void ExecuteForward(IContext context)
        {
            var input = context.Data.GetMatrix();

            // feed forward
            var output = input.Multiply(_weight);
            output.AddToEachRow(_bias);

            // set output
            _AddNextGraphAction(context, new MatrixGraphData(output), () => new Backpropagation(this, input));
        }

        protected override (string Description, byte[] Data) _GetInfo()
        {
            return ("FF", _WriteData(WriteTo));
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            var lap = factory.LinearAlgebraProvider;

            // read the bias parameters
            var bias = FloatVector.ReadFrom(reader);
            if (_bias == null)
                _bias = lap.CreateVector(bias);
            else
                _bias.Data = bias;

            // read the weight parameters
            var weight = FloatMatrix.ReadFrom(reader);
            if (_weight == null)
                _weight = lap.CreateMatrix(weight);
            else
                _weight.Data = weight;
        }

        public override void WriteTo(BinaryWriter writer)
        {
            _bias.Data.WriteTo(writer);
            _weight.Data.WriteTo(writer);
        }
    }
}
