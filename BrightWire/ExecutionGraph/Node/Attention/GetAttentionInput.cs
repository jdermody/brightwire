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
    internal class GetAttentionInput : NodeBase
    {
        LinearAlgebraProvider _lap;
        string? _encoderName, _decoderName;
        uint _inputSize, _encoderSize, _decoderSize;

        class Backpropagation : SingleBackpropagationBase<GetAttentionInput>
        {
            public Backpropagation(GetAttentionInput source) : base(source)
            {
            }

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                return GraphData.Null;
            }
        }

        public GetAttentionInput(
            LinearAlgebraProvider lap, 
            uint inputSize, 
            uint encoderSize, 
            uint decoderSize,
            string? encoderName, 
            string? decoderName, 
            string? name, 
            string? id = null
        ) : base(name, id)
        {
            _lap         = lap;
            _encoderName = encoderName;
            _decoderName = decoderName;
            _inputSize   = inputSize;
            _encoderSize = encoderSize;
            _decoderSize = decoderSize;
        }

        public uint BlockSize => _inputSize + _encoderSize + _decoderSize;

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            var currentIndex = context.BatchSequence.SequenceIndex;
            var batchSize = context.BatchSequence.MiniBatch.BatchSize;

            // get the previous decoder state
            IMatrix? decoderHiddenState = null;
            if (_decoderSize > 0 && _decoderName is not null) {
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
                if (decoderHiddenState.ColumnCount != _decoderSize)
                    throw new Exception($"Expected decoder size to be {_decoderSize} but found {decoderHiddenState.ColumnCount}");
            }

            // find each encoder hidden state and sequence input
            var previousBatch = context.BatchSequence.MiniBatch.PreviousMiniBatch ?? throw new Exception("No previous mini batch");
            Debug.Assert(batchSize == previousBatch.BatchSize);
            IMatrix[]? encoderStates = null;
            var inputs = new IMatrix[previousBatch.SequenceCount];
            for (uint i = 0, len = previousBatch.SequenceCount; i < len; i++) {
                var sequence = previousBatch.GetSequenceAtIndex(i);
                if (_encoderSize > 0 && _encoderName is not null) {
                    if (i == 0)
                        encoderStates = new IMatrix[len];
                    var encoderState = sequence.GraphContext!.GetData("hidden-forward").Single(d => d.Name == _encoderName).Data.GetMatrix()
                        ?? throw new Exception("Not able to find the encoder hidden state");
                    if (encoderState.ColumnCount != _encoderSize)
                        throw new Exception($"Expected encoder size to be {_encoderSize} but found {encoderState.ColumnCount}");
                    encoderStates![i] = encoderState;
                }

                var input = sequence.Input?.GetMatrix() ?? throw new Exception("Not able to find the input matrix");
                if (input.ColumnCount != _inputSize)
                    throw new Exception($"Expected input size to be {_inputSize} but found {input.ColumnCount}");
                inputs[i] = input;
            }

            var sequenceSize = previousBatch.SequenceCount;
            var numInputRows = batchSize * sequenceSize;

            // create the input tensor
            var inputTensor = _lap.CreateTensor3D(sequenceSize, batchSize, BlockSize, false);
            var inputMatrices = sequenceSize.AsRange().Select(inputTensor.GetMatrix).ToArray();
            for (uint i = 0; i < numInputRows; i++) {
                var sequenceIndex = i / batchSize;
                var batchIndex = i % batchSize;
                var inputRow = inputMatrices[sequenceIndex].Row(batchIndex);
                inputs[sequenceIndex].Row(batchIndex).CopyTo(inputRow, 0, 0);
                encoderStates?[sequenceIndex].Row(batchIndex).CopyTo(inputRow, 0, _inputSize);
                decoderHiddenState?.Row(batchIndex).CopyTo(inputRow, 0, _inputSize + _encoderSize);
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
            _encoderName.WriteTo(writer);
            _decoderName.WriteTo(writer);
            _inputSize.WriteTo(writer);
            _encoderSize.WriteTo(writer);
            _decoderSize.WriteTo(writer);
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            _lap = factory.LinearAlgebraProvider;
            _encoderName = reader.ReadString();
            _decoderName = reader.ReadString();
            _inputSize = reader.ReadUInt32();
            _encoderSize = reader.ReadUInt32();
            _decoderSize = reader.ReadUInt32();
        }
    }
}
