using System;
using System.IO;
using BrightData;

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
            readonly IMatrix _input;

            public Backpropagation(FeedForward source, IMatrix input) : base(source)
            {
                _input = input;
            }

            protected override void DisposeMemory(bool isDisposing)
            {
                //_input.Dispose();
            }

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphSequenceContext context)
            {
                var es = errorSignal.GetMatrix();

                // work out the next error signal
                var ret = es.TransposeAndMultiply(_source._weight);

                // calculate the update to the weights
                var weightUpdate = _input.TransposeThisAndMultiply(es);

                // store the updates
                var learningContext = context.LearningContext!;
                learningContext.StoreUpdate(_source, es, err => _source.UpdateBias(err, learningContext));
                learningContext.StoreUpdate(_source, weightUpdate, err => _source.UpdateWeights(err, learningContext));

                return errorSignal.ReplaceWith(ret);
            }
        }

        IVector _bias;
        IMatrix _weight;
        IGradientDescentOptimisation _updater;
        uint _inputSize, _outputSize;

        public FeedForward(uint inputSize, uint outputSize, IVector bias, IMatrix weight, IGradientDescentOptimisation updater, string? name = null) : base(name)
        {
            _bias = bias;
            _weight = weight;
            _updater = updater;
            _inputSize = inputSize;
            _outputSize = outputSize;
        }

        public IVector Bias => _bias;
        public IMatrix Weight => _weight;
        public uint InputSize => _inputSize;
        public uint OutputSize => _outputSize;

        protected override void DisposeInternal(bool isDisposing)
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
            using var columnSums = delta.ColumnSums();
            _bias.AddInPlace(columnSums, 1f / delta.RowCount, context.BatchLearningRate);
        }

        protected IMatrix FeedForwardInternal(IMatrix input, IMatrix weight)
        {
            var output = input.Multiply(weight);
            output.AddToEachRow(_bias.Segment);
            return output;
        }

        public IMatrix Forward(IMatrix input) => FeedForwardInternal(input, _weight);

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphSequenceContext context, NodeBase? source)
        {
            var input = signal.GetMatrix();
            var output = FeedForwardInternal(input, _weight);

            return (this, signal.ReplaceWith(output), () => new Backpropagation(this, input));
        }

        protected override (string Description, byte[] Data) GetInfo()
        {
            return ("FF", WriteData(WriteTo));
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            _inputSize = (uint)reader.ReadInt32();
            _outputSize = (uint)reader.ReadInt32();

            // read the bias parameters
            var bias = factory.Context.ReadVectorFrom(reader);
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (_bias == null)
                _bias = bias;
            else {
                bias.Segment.CopyTo(_bias.Segment);
                bias.Dispose();
            }

            // read the weight parameters
            var weight = factory.Context.ReadMatrixFrom(reader);
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (_weight == null)
                _weight = weight;
            else {
                weight.Segment.CopyTo(_weight.Segment);
                weight.Dispose();
            }

            // ReSharper disable once ConstantNullCoalescingCondition
            _updater ??= factory.CreateWeightUpdater(_weight);
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write((int)_inputSize);
            writer.Write((int)_outputSize);
            _bias.WriteTo(writer);
            _weight.WriteTo(writer);
        }
    }
}
