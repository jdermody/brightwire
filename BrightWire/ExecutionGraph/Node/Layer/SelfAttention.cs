using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightData;
using BrightData.Helper;
using BrightData.LinearAlgebra;
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
        LinearAlgebraProvider _lap;
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

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                var (left, right) = errorSignal.GetMatrix().SplitAtColumn(_position);

                // train the attention layer
                var learningContext = context.LearningContext!;
                foreach (var (encoderState, combinedState) in _weights) {
                    using var err = encoderState.PointwiseMultiply(right);
                    using var errRows = err.RowSums();
                    errRows.MultiplyInPlace(1f / err.ColumnCount);

                    using var feedForwardError = errRows.SoftmaxDerivative();
                    using var feedForwardError2 = feedForwardError.ColumnSums();
                    using var collapsed = combinedState.TransposeThisAndMultiply(feedForwardError);
                    using var rowSums = collapsed.RowSums();
                    rowSums.MultiplyInPlace(1f / feedForwardError.ColumnCount);
                    var weightUpdate = rowSums.Reshape(null, 1);

                    learningContext.AddError(ErrorType.Bias, _source, feedForwardError2.Reshape(null, 1));
                    learningContext.AddError(ErrorType.Weight, _source, weightUpdate);
                }

                return left.AsGraphData();
            }
        }

        public SelfAttention(LinearAlgebraProvider lap, string encoderName, string decoderName, FeedForward layer, string? name, string? id = null) : base(name, id)
        {
            _lap = lap;
            _encoderName = encoderName;
            _decoderName = decoderName;
            _layer = layer;
        }

        public override void ApplyError(ErrorType type, ITensor delta, ILearningContext context) => _layer.ApplyError(type, delta, context);

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
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
            using var combinedAttention = _lap.CreateMatrix(signal.Rows, encoderStates[0].ColumnCount);
            var backward = new List<(IMatrix EncoderState, IMatrix CombinedState)>();
            var index = 0;
            foreach (var (first, second) in softmax.AllColumns(false).Zip(encoderStates)) {
                // save the average weight across the batch for diagnostics
                var multiplyWeight = first.Segment.Average();
                if(!String.IsNullOrWhiteSpace(Name))
                    context.SetData($"{Name}:{context.BatchSequence.SequenceIndex}:{index}", "self-attention", new SingleGraphData(multiplyWeight));

                using var stretched = _lap.CreateMatrixFromColumns(Enumerable.Repeat(first, (int)second.ColumnCount).ToArray());
                using var result = second.PointwiseMultiply(stretched);
                combinedAttention.AddInPlace(result);
                backward.Add((second, inputs[index++]));
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
            _lap = factory.LinearAlgebraProvider;
            _layer ??= GenericActivator.CreateUninitialized<FeedForward>();
            _layer.ReadFrom(factory, reader);
            _encoderName = reader.ReadString();
            _decoderName = reader.ReadString();
        }
    }
}
