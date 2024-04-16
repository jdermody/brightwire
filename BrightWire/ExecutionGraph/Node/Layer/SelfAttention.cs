using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightData;
using BrightData.Helper;
using BrightData.LinearAlgebra;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node.Input;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    /// <summary>
    /// Implementation of self attention from: https://arxiv.org/abs/1409.0473
    /// </summary>
    class SelfAttention(LinearAlgebraProvider<float> lap, string encoderName, string decoderName, FeedForward layer, string? name, string? id = null)
        : NodeBase(name, id), IHaveFeedForward
    {
        class Backpropagation(SelfAttention source, uint position, List<(IMatrix<float> EncoderState, IMatrix<float> CombinedState)> weights)
            : SingleBackpropagationBase<SelfAttention>(source)
        {
            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                var (left, right) = errorSignal.GetMatrix().SplitAtColumn(position);

                // train the attention layer
                var learningContext = context.LearningContext!;
                foreach (var (encoderState, combinedState) in weights) {
                    using var err = encoderState.PointwiseMultiply(right);
                    using var errRows = err.RowSums();
                    errRows.MultiplyInPlace(1f / err.ColumnCount);

                    using var feedForwardError = errRows.SoftmaxDerivative();
                    using var feedForwardError2 = feedForwardError.ColumnSums();
                    using var collapsed = combinedState.TransposeThisAndMultiply(feedForwardError);
                    using var rowSums = collapsed.RowSums();
                    rowSums.MultiplyInPlace(1f / feedForwardError.ColumnCount);
                    var weightUpdate = rowSums.Reshape(null, 1);

                    learningContext.AddError(NodeErrorType.Bias, _source, feedForwardError2.Reshape(null, 1));
                    learningContext.AddError(NodeErrorType.Weight, _source, weightUpdate);
                }

                return left.AsGraphData();
            }
        }

        public override void ApplyError(NodeErrorType type, ITensor<float> delta, ILearningContext context) => layer.ApplyError(type, delta, context);

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            var currentIndex = context.BatchSequence.SequenceIndex;

            // get the previous decoder state
            IMatrix<float>? decoderHiddenState = null;
            if (currentIndex == 0) {
                if (FindByName(decoderName) is IHaveMemoryNode { Memory: MemoryFeeder decoderMemory })
                    decoderHiddenState = context.ExecutionContext.GetMemory(decoderMemory.Id);
            }
            else {
                var previousContext = context.BatchSequence.MiniBatch.GetSequenceAtIndex(currentIndex - 1).GraphContext;
                decoderHiddenState = previousContext?.GetData("hidden-forward").Single(d => d.Name == decoderName).Data.GetMatrix();
            }
            if (decoderHiddenState == null)
                throw new Exception("Not able to find the decoder hidden state");

            // attend to each encoder hidden state
            var previous = context.BatchSequence.MiniBatch.PreviousMiniBatch ?? throw new Exception("No previous mini batch");

            IMatrix<float>? weights = null;
            var encoderStates = new List<IMatrix<float>>();
            var inputs = new List<IMatrix<float>>();
            for (uint i = 0, len = previous.SequenceCount; i < len; i++) {
                var sequence = previous.GetSequenceAtIndex(i);
                var encoderState = sequence.GraphContext!.GetData("hidden-forward").Single(d => d.Name == encoderName).Data.GetMatrix();
                var combined = decoderHiddenState.ConcatRight(encoderState);
                var output = layer.Forward(combined);

                if (weights == null)
                    weights = output;
                else {
                    var next = weights.ConcatRight(output);
                    weights.Dispose();
                    weights = next;
                }
                encoderStates.Add(encoderState);
                inputs.Add(combined);
            }

            // form the new attention as a product of the weights
            using var softMax = weights!.Softmax();
            using var combinedAttention = lap.CreateMatrix(signal.Rows, encoderStates[0].ColumnCount, false);
            var backward = new List<(IMatrix<float> EncoderState, IMatrix<float> CombinedState)>();
            var index = 0;
            foreach (var (first, second) in softMax.AllColumnsAsReadOnly(false).Zip(encoderStates)) {
                // save the average weight across the batch for diagnostics
                var multiplyWeight = first.ReadOnlySegment.ApplyReadOnlySpan(x => x.Average());
                if(!String.IsNullOrWhiteSpace(Name))
                    context.SetData($"{Name}:{context.BatchSequence.SequenceIndex}:{index}", "self-attention", new SingleGraphData(multiplyWeight));

                using var stretched = lap.CreateMatrixFromColumns(Enumerable.Repeat(first, (int)second.ColumnCount).ToArray());
                using var result = second.PointwiseMultiply(stretched);
                combinedAttention.AddInPlace(result);
                backward.Add((second, inputs[index++]));
            }
            combinedAttention.MultiplyInPlace(1f / index);

            // concatenate signal with combined attention
            var final = signal.GetMatrix().ConcatRight(combinedAttention);
            return (this, final.AsGraphData(), () => new Backpropagation(this, signal.Columns, backward));
        }

        public IFeedForward FeedForward => layer;

        protected override (string Description, byte[] Data) GetInfo()
        {
            return ("SA", WriteData(WriteTo));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            layer.WriteTo(writer);
            encoderName.WriteTo(writer);
            decoderName.WriteTo(writer);
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            lap = factory.LinearAlgebraProvider;
            layer ??= GenericActivator.CreateUninitialized<FeedForward>();
            layer.ReadFrom(factory, reader);
            encoderName = reader.ReadString();
            decoderName = reader.ReadString();
        }
    }
}
