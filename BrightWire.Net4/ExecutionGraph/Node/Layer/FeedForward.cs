using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    public class FeedForward : NodeBase
    {
        class Backpropagation : IBackpropagation
        {
            readonly FeedForward _layer;
            readonly IMatrix _input = null;

            public Backpropagation(FeedForward layer, IMatrix input)
            {
                _layer = layer;
                _input = input;
            }

            public void Dispose()
            {
                _input.Dispose();
            }

            public IMatrix Backward(IMatrix errorSignal, IContext context, bool calculateOutput)
            {
                // work out the next error signal
                IMatrix ret = null;
                if (calculateOutput)
                    ret = errorSignal.TransposeAndMultiply(_layer._weight);

                // calculate the update to the weights
                var weightUpdate = _input.TransposeThisAndMultiply(errorSignal);

                // store the updates
                var learningContext = context.LearningContext;
                learningContext.Store(errorSignal, err => _UpdateBias(err, learningContext));
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

                return ret;
            }

            void _UpdateBias(IMatrix delta, ILearningContext context)
            {
                using (var columnSums = delta.ColumnSums())
                    _layer._bias.AddInPlace(columnSums, 1f / columnSums.Count, context.LearningRate);
            }
        }

        protected readonly IVector _bias;
        protected readonly IMatrix _weight;
        protected readonly IGradientDescentOptimisation _updater;

        public FeedForward(IVector bias, IMatrix weight, IGradientDescentOptimisation updater, string name = null) : base(name)
        {
            _bias = bias;
            _weight = weight;
            _updater = updater;
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

        public override void SetPrimaryInput(IContext context)
        {
            var input = context.Data.GetAsMatrix();

            // feed forward
            var output = input.Multiply(_weight);
            output.AddToEachRow(_bias);

            // set output
            context.Add(new GraphAction(this, new MatrixGraphData(output)), () => new Backpropagation(this, input));
        }
    }
}
