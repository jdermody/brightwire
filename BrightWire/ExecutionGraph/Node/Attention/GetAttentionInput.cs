using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BrightData;
using BrightData.Helper;
using BrightData.LinearAlgebra;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node.Input;

namespace BrightWire.ExecutionGraph.Node.Attention
{
    internal class GetAttentionInput(
        LinearAlgebraProvider lap,
        uint inputSize,
        uint encoderSize,
        uint decoderSize,
        string? encoderName,
        string? decoderName,
        string? name,
        string? id = null)
        : NodeBase(name, id)
    {
        class Backpropagation(GetAttentionInput source) : SingleBackpropagationBase<GetAttentionInput>(source)
        {
            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                return GraphData.Null;
            }
        }

        public uint BlockSize => inputSize + encoderSize + decoderSize;

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            var currentIndex = context.BatchSequence.SequenceIndex;
            var batchSize = context.BatchSequence.MiniBatch.BatchSize;

            // get the previous decoder state
            IMatrix? decoderHiddenState = null;
            if (decoderSize > 0 && decoderName is not null) {
                if (currentIndex == 0) {
                    if ((FindByName(decoderName) as IHaveMemoryNode)?.Memory is MemoryFeeder decoderMemory)
                        decoderHiddenState = context.ExecutionContext.GetMemory(decoderMemory.Id);
                }
                else {
                    var previousContext = context.BatchSequence.MiniBatch.GetSequenceAtIndex(currentIndex - 1).GraphContext;
                    decoderHiddenState = previousContext?.GetData("hidden-forward").Single(d => d.Name == decoderName).Data.GetMatrix();
                }

                if (decoderHiddenState == null)
                    throw new Exception("Not able to find the decoder hidden state");
                if (decoderHiddenState.ColumnCount != decoderSize)
                    throw new Exception($"Expected decoder size to be {decoderSize} but found {decoderHiddenState.ColumnCount}");
            }

            // find each encoder hidden state and sequence input
            var previousBatch = context.BatchSequence.MiniBatch.PreviousMiniBatch ?? throw new Exception("No previous mini batch");
            Debug.Assert(batchSize == previousBatch.BatchSize);
            IMatrix[]? encoderStates = null;
            var inputs = new IMatrix[previousBatch.SequenceCount];
            for (uint i = 0, len = previousBatch.SequenceCount; i < len; i++) {
                var sequence = previousBatch.GetSequenceAtIndex(i);
                if (encoderSize > 0 && encoderName is not null) {
                    if (i == 0)
                        encoderStates = new IMatrix[len];
                    var encoderState = sequence.GraphContext!.GetData("hidden-forward").Single(d => d.Name == encoderName).Data.GetMatrix()
                        ?? throw new Exception("Not able to find the encoder hidden state");
                    if (encoderState.ColumnCount != encoderSize)
                        throw new Exception($"Expected encoder size to be {encoderSize} but found {encoderState.ColumnCount}");
                    encoderStates![i] = encoderState;
                }

                var input = sequence.Input?.GetMatrix() ?? throw new Exception("Not able to find the input matrix");
                if (input.ColumnCount != inputSize)
                    throw new Exception($"Expected input size to be {inputSize} but found {input.ColumnCount}");
                inputs[i] = input;
            }

            var sequenceSize = previousBatch.SequenceCount;
            var numInputRows = batchSize * sequenceSize;

            // create the input tensor
            var inputTensor = lap.CreateTensor3D(sequenceSize, batchSize, BlockSize, false);
            var inputMatrices = sequenceSize.AsRange().Select(inputTensor.GetMatrix).ToArray();
            for (uint i = 0; i < numInputRows; i++) {
                var sequenceIndex = i / batchSize;
                var batchIndex = i % batchSize;
                var inputRow = inputMatrices[sequenceIndex].GetRow(batchIndex);
                inputs[sequenceIndex].GetRow(batchIndex).CopyTo(inputRow, sourceOffset:0, targetOffset:0);
                encoderStates?[sequenceIndex].GetRow(batchIndex).CopyTo(inputRow, 0, inputSize);
                decoderHiddenState?.GetRow(batchIndex).CopyTo(inputRow, 0, inputSize + encoderSize);
            }
            inputMatrices.DisposeAll();

            return (this, inputTensor.AsGraphData(), () => new Backpropagation(this));
        }

        protected override (string Description, byte[] Data) GetInfo()
        {
            return ("GAI", WriteData(WriteTo));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            encoderName.WriteTo(writer);
            decoderName.WriteTo(writer);
            inputSize.WriteTo(writer);
            encoderSize.WriteTo(writer);
            decoderSize.WriteTo(writer);
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            lap = factory.LinearAlgebraProvider;
            encoderName = reader.ReadString();
            decoderName = reader.ReadString();
            inputSize = reader.ReadUInt32();
            encoderSize = reader.ReadUInt32();
            decoderSize = reader.ReadUInt32();
        }
    }
}
