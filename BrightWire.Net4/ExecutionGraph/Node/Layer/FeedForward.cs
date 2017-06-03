using BrightWire.ExecutionGraph.Helper;
using BrightWire.Models;
using System.Collections.Generic;
using System.IO;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    /// <summary>
    /// Feed forward neural network
    /// https://en.wikipedia.org/wiki/Feedforward_neural_network
    /// </summary>
    class FeedForward : NodeBase, IFeedForward
    {
        protected class Backpropagation : SingleBackpropagationBase<FeedForward>
        {
            readonly IMatrix _input = null;

            public Backpropagation(FeedForward source, IMatrix input) : base(source)
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
                IMatrix ret = es.TransposeAndMultiply(_source._weight);

                // calculate the update to the weights
                var weightUpdate = _input.TransposeThisAndMultiply(es);

                // store the updates
                var learningContext = context.LearningContext;
                learningContext.StoreUpdate(_source, es, err => _source.UpdateBias(err, learningContext));
                learningContext.StoreUpdate(_source, weightUpdate, err => _source.UpdateWeights(err, learningContext));

                return errorSignal.ReplaceWith(ret);
            }
        }

        IVector _bias;
        IMatrix _weight;
        IGradientDescentOptimisation _updater;
        int _inputSize, _outputSize;

        public FeedForward(int inputSize, int outputSize, IVector bias, IMatrix weight, IGradientDescentOptimisation updater, string name = null) : base(name)
        {
            _bias = bias;
            _weight = weight;
            _updater = updater;
            _inputSize = inputSize;
            _outputSize = outputSize;
        }

        public IVector Bias => _bias;
        public IMatrix Weight => _weight;
        public int InputSize => _inputSize;
        public int OutputSize => _outputSize;

        protected override void _Initalise(GraphFactory factory, string description, byte[] data)
        {
            _ReadFrom(data, reader => ReadFrom(factory, reader));
        }

        protected override void _Dispose(bool isDisposing)
        {
            _bias.Dispose();
            _weight.Dispose();
        }

        public void UpdateWeights(IMatrix delta, ILearningContext context)
        {
            _updater.Update(_weight, delta, context);
        }

        public void UpdateBias(IMatrix delta, ILearningContext context)
        {
            using (var columnSums = delta.ColumnSums())
                _bias.AddInPlace(columnSums, 1f / columnSums.Count, context.LearningRate);
        }

        protected IMatrix _FeedForward(IMatrix input, IMatrix weight)
        {
            var output = input.Multiply(weight);
            output.AddToEachRow(_bias);
            return output;
        }

        public override void ExecuteForward(IContext context)
        {
            var input = context.Data.GetMatrix();
            var output = _FeedForward(input, _weight);

            // set output
            _AddNextGraphAction(context, context.Data.ReplaceWith(output), () => new Backpropagation(this, input));
        }

        protected override (string Description, byte[] Data) _GetInfo()
        {
            return ("FF", _WriteData(WriteTo));
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            var lap = factory.LinearAlgebraProvider;

            _inputSize = reader.ReadInt32();
            _outputSize = reader.ReadInt32();

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

            if (_updater == null)
                _updater = factory.CreateWeightUpdater(_weight);
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(_inputSize);
            writer.Write(_outputSize);
            _bias.Data.WriteTo(writer);
            _weight.Data.WriteTo(writer);
        }
    }
}
