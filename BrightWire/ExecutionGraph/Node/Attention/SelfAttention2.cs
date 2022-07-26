using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData;
using BrightData.LinearAlgebra;
using BrightWire.ExecutionGraph.Node.Input;
using BrightData.Serialisation;

namespace BrightWire.ExecutionGraph.Node.Attention
{
    internal class SelfAttention2 : NodeBase, IHaveFeedForward
    {
        LinearAlgebraProvider _lap;
        string _encoderName, _decoderName;
        uint _inputSize, _encoderSize, _decoderSize;

        public SelfAttention2(
            LinearAlgebraProvider lap, 
            string encoderName, 
            string decoderName,
            uint inputSize, 
            uint encoderSize, 
            uint decoderSize,
            string? name, 
            string? id = null
        ) : base(name, id)
        {
            _lap = lap;
            _encoderName = encoderName;
            _decoderName = decoderName;
            _inputSize = inputSize;
            _encoderSize = encoderSize;
            _decoderSize = decoderSize;
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            var currentIndex = context.BatchSequence.SequenceIndex;

            // get the previous decoder state
            IMatrix? decoderHiddenState = null;
            uint numInputRows = 0;
            if (_decoderSize > 0) {
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
                numInputRows += decoderHiddenState.RowCount;
            }

            // find each encoder hidden state and sequence input
            var previousBatch = context.BatchSequence.MiniBatch.PreviousMiniBatch;
            if(previousBatch == null)
                throw new Exception("No previous mini batch");

            var encoderStates = new List<IMatrix>();
            var inputs = new List<IMatrix>();
            for (uint i = 0, len = previousBatch.SequenceCount; i < len; i++) {
                var sequence = previousBatch.GetSequenceAtIndex(i);
                if (_encoderSize > 0) {
                    var encoderState = sequence.GraphContext!.GetData("hidden-forward").Single(d => d.Name == _encoderName).Data.GetMatrix();
                    if(encoderState == null)
                        throw new Exception("Not able to find the encoder hidden state");
                    if (encoderState.ColumnCount != _encoderSize)
                        throw new Exception($"Expected encoder size to be {_encoderSize} but found {encoderState.ColumnCount}");
                    encoderStates.Add(encoderState);
                    numInputRows += encoderState.RowCount;
                }

                var input = sequence.Input?.GetMatrix();
                if (input == null)
                    throw new Exception("Not able to find the input matrix");
                if (input.ColumnCount != _inputSize)
                    throw new Exception($"Expected input size to be {_inputSize} but found {input.ColumnCount}");
                inputs.Add(input);
                numInputRows += input.RowCount;
            }

            // create the big input matrix
            using var inputMatrix = _lap.CreateMatrix(numInputRows, _inputSize + _encoderSize + _decoderSize, false);

            return (this, signal, null);
        }

        public IFeedForward FeedForward { get; }

        protected override (string Description, byte[] Data) GetInfo()
        {
            return ("SA", WriteData(WriteTo));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            //_layer.WriteTo(writer);
            _encoderName.WriteTo(writer);
            _decoderName.WriteTo(writer);
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            _lap = factory.LinearAlgebraProvider;
            //_layer ??= GenericActivator.CreateUninitialized<FeedForward>();
            //_layer.ReadFrom(factory, reader);
            _encoderName = reader.ReadString();
            _decoderName = reader.ReadString();
        }
    }
}
