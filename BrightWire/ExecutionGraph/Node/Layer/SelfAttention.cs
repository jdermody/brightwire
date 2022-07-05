using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightData;
using BrightData.Helper;
using BrightData.Serialisation;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node.Input;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    /// <summary>
    /// Implementation of self attention from: https://arxiv.org/abs/1409.0473
    /// </summary>
    class SelfAttention : NodeBase, IHaveFeedForward
    {
        FeedForward _layer;
        string _encoderName, _decoderName;

        class Backpropagation : SingleBackpropagationBase<SelfAttention>
        {
            readonly uint _position;
            readonly List<(IMatrix EncoderState,  IMatrix CombinedState)> _weights;

            public Backpropagation(SelfAttention source, uint position, List<(IMatrix EncoderState, IMatrix CombinedState)> weights) : base(source)
            {
                _position = position;
                _weights = weights;
            }

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphSequenceContext context)
            {
                var (left, right) = errorSignal.GetMatrix().SplitAtColumn(_position);

                // train the attention layer
                var learningContext = context.LearningContext!;
                foreach (var (encoderState, combinedState) in _weights) {
                    using var err = encoderState.PointwiseMultiply(right);
                    using var errRows = err.RowSums();
                    errRows.MultiplyInPlace(1f / err.ColumnCount);

                    var feedForwardError = errRows.SoftmaxDerivative();
                    using var collapsed = combinedState.TransposeThisAndMultiply(feedForwardError).RowSums();
                    collapsed.MultiplyInPlace(1f / feedForwardError.ColumnCount);
                    var weightUpdate = collapsed.Reshape(null, 1);

                    learningContext.StoreUpdate(_source, feedForwardError, e => _source._layer.UpdateBias(e, learningContext));
                    learningContext.StoreUpdate(_source, weightUpdate, e => _source._layer.UpdateWeights(e, learningContext));
                }

                return left.AsGraphData();
            }
        }

        public SelfAttention(string encoderName, string decoderName, FeedForward layer, string? name, string? id = null) : base(name, id)
        {
            _encoderName = encoderName;
            _decoderName = decoderName;
            _layer = layer;
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphSequenceContext context, NodeBase? source)
        {
            var currentIndex = context.BatchSequence.SequenceIndex;

            // get the previous decoder state
            IMatrix? decoderHiddenState = null;
            if (currentIndex == 0) {
                if ((FindByName(_decoderName) as IHaveMemoryNode)?.Memory is MemoryFeeder decoderMemory)
                    decoderHiddenState = context.ExecutionContext.GetMemory(decoderMemory.Id);
            }
            else {
                var previousContext = context.BatchSequence.MiniBatch.GetSequenceAtIndex(currentIndex - 1).GraphContext;
                decoderHiddenState = previousContext?.GetData("hidden-forward").Single(d => d.Name == _decoderName).Data.GetMatrix();
            }
            if (decoderHiddenState == null)
                throw new Exception("Not able to find the decoder hidden state");

            // attend to each encoder hidden state
            var previous = context.BatchSequence.MiniBatch.PreviousMiniBatch;
            if(previous == null)
                throw new Exception("No previous mini batch");

            IMatrix? weights = null;
            var encoderStates = new List<IMatrix>();
            var inputs = new List<IMatrix>();
            for (uint i = 0, len = previous.SequenceCount; i < len; i++) {
                var encoderState = previous.GetSequenceAtIndex(i).GraphContext!.GetData("hidden-forward").Single(d => d.Name == _encoderName).Data.GetMatrix();
                var combined = decoderHiddenState.ConcatRows(encoderState);
                var output = _layer.Forward(combined);

                if (weights == null)
                    weights = output;
                else {
                    var next = weights.ConcatRows(output);
                    weights.Dispose();
                    weights = next;
                }
                encoderStates.Add(encoderState);
                inputs.Add(combined);
            }

            // form the new attention as a product of the weights
            using var softmax = weights!.Softmax();
            var lap = context.GetLinearAlgebraProvider();
            var combinedAttention = lap.CreateMatrix(signal.Rows, encoderStates[0].ColumnCount);
            var backward = new List<(IMatrix EncoderState, IMatrix CombinedState)>();
            var index = 0;
            foreach (var (first, second) in softmax.ColumnVectors().Zip(encoderStates)) {
                var multiplyWeight = first.Average();
                if(!String.IsNullOrWhiteSpace(Name))
                    context.SetData($"{Name}:{context.BatchSequence.SequenceIndex}:{index}", "self-attention", new SingleGraphData(multiplyWeight));

                var saved = second.Clone();
                using var stretched = lap.CreateMatrix(second.RowCount, second.ColumnCount, (i, j) => first[i]);
                second.PointwiseMultiplyInPlace(stretched);
                //second.Multiply(multiplyWeight);
                combinedAttention.AddInPlace(second);
                backward.Add((saved, inputs[index++]));
            }
            combinedAttention.MultiplyInPlace(1f / index);

            // concatenate signal with combined attention
            var final = signal.GetMatrix().ConcatRows(combinedAttention);
            return (this, final.AsGraphData(), () => new Backpropagation(this, signal.Columns, backward));
        }

        public IFeedForward FeedForward => _layer;

        protected override (string Description, byte[] Data) GetInfo()
        {
            return ("SA", WriteData(WriteTo));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            _layer.WriteTo(writer);
            _encoderName.WriteTo(writer);
            _decoderName.WriteTo(writer);
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            // ReSharper disable once ConstantNullCoalescingCondition
            _layer ??= GenericActivator.CreateUninitialized<FeedForward>();
            _layer.ReadFrom(factory, reader);
            _encoderName = reader.ReadString();
            _decoderName = reader.ReadString();
        }
    }
}
