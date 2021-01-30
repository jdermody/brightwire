using System.IO;
using BrightData;
using BrightData.FloatTensor;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    /// <summary>
    /// Feed forward neural network
    /// https://en.wikipedia.org/wiki/Feedforward_neural_network
    /// </summary>
    internal class FeedForward : NodeBase, IFeedForward
    {
        protected class Backpropagation : SingleBackpropagationBase<FeedForward>
        {
            readonly IFloatMatrix _input = null;

            public Backpropagation(FeedForward source, IFloatMatrix input) : base(source)
            {
                _input = input;
            }

            protected override void _Dispose(bool isDisposing)
            {
                //_input.Dispose();
            }

            protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IGraphContext context, INode[] parents)
            {
                var es = errorSignal.GetMatrix();

                // work out the next error signal
                var ret = es.TransposeAndMultiply(_source._weight);

                // calculate the update to the weights
                var weightUpdate = _input.TransposeThisAndMultiply(es);

                // store the updates
                var learningContext = context.LearningContext;
                learningContext.StoreUpdate(_source, es, err => _source.UpdateBias(err, learningContext));
                learningContext.StoreUpdate(_source, weightUpdate, err => _source.UpdateWeights(err, learningContext));

                return errorSignal.ReplaceWith(ret);
            }
        }

        IFloatVector _bias;
        IFloatMatrix _weight;
        IGradientDescentOptimisation _updater;
        uint _inputSize, _outputSize;

        public FeedForward(uint inputSize, uint outputSize, IFloatVector bias, IFloatMatrix weight, IGradientDescentOptimisation updater, string name = null) : base(name)
        {
            _bias = bias;
            _weight = weight;
            _updater = updater;
            _inputSize = inputSize;
            _outputSize = outputSize;
        }

        public IFloatVector Bias => _bias;
        public IFloatMatrix Weight => _weight;
        public uint InputSize => _inputSize;
        public uint OutputSize => _outputSize;

        protected override void _Dispose(bool isDisposing)
        {
            _bias.Dispose();
            _weight.Dispose();
        }

        public void UpdateWeights(IFloatMatrix delta, ILearningContext context)
        {
            _updater.Update(_weight, delta, context);
        }

        public void UpdateBias(IFloatMatrix delta, ILearningContext context)
        {
            using var columnSums = delta.ColumnSums();
            _bias.AddInPlace(columnSums, 1f / delta.RowCount, context.BatchLearningRate);
        }

        protected IFloatMatrix _FeedForward(IFloatMatrix input, IFloatMatrix weight)
        {
            var output = input.Multiply(weight);
            output.AddToEachRow(_bias);
            return output;
        }

        public override void ExecuteForward(IGraphContext context)
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

            _inputSize = (uint)reader.ReadInt32();
            _outputSize = (uint)reader.ReadInt32();

            // read the bias parameters
            var bias = factory.Context.ReadVectorFrom(reader);
            if (_bias == null)
                _bias = lap.CreateVector(bias);
            else
                _bias.Data = bias;

            // read the weight parameters
            var weight = factory.Context.ReadMatrixFrom(reader);
            if (_weight == null)
                _weight = lap.CreateMatrix(weight);
            else
                _weight.Data = weight;

            _updater ??= factory.CreateWeightUpdater(_weight);
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write((int)_inputSize);
            writer.Write((int)_outputSize);
            _bias.Data.WriteTo(writer);
            _weight.Data.WriteTo(writer);
        }
    }
}
