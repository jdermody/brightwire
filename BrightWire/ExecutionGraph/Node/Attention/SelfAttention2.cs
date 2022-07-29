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
using BrightWire.ExecutionGraph.Node.Layer;

namespace BrightWire.ExecutionGraph.Node.Attention
{
    internal class SelfAttention2 : NodeBase
    {
        LinearAlgebraProvider _lap;
        string _encoderName, _decoderName;
        uint _inputSize, _encoderSize, _decoderSize, _blockSize;
        IMatrix _attention;

        public SelfAttention2(
            LinearAlgebraProvider lap, 
            string encoderName, 
            string decoderName,
            uint inputSize, 
            uint encoderSize, 
            uint decoderSize,
            IWeightInitialisation weightInit,
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
            _attention = weightInit.CreateWeight(1, _blockSize = _inputSize + _encoderSize + _decoderSize);
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            var currentIndex = context.BatchSequence.SequenceIndex;

            // get the previous decoder state
            IMatrix? decoderHiddenState = null;
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
            }

            // find each encoder hidden state and sequence input
            var previousBatch = context.BatchSequence.MiniBatch.PreviousMiniBatch;
            if(previousBatch == null)
                throw new Exception("No previous mini batch");

            IMatrix[]? encoderStates = null;
            var inputs = new IMatrix[previousBatch.SequenceCount];
            for (uint i = 0, len = previousBatch.SequenceCount; i < len; i++) {
                var sequence = previousBatch.GetSequenceAtIndex(i);
                if (_encoderSize > 0) {
                    if (i == 0)
                        encoderStates = new IMatrix[len];
                    var encoderState = sequence.GraphContext!.GetData("hidden-forward").Single(d => d.Name == _encoderName).Data.GetMatrix();
                    if(encoderState == null)
                        throw new Exception("Not able to find the encoder hidden state");
                    if (encoderState.ColumnCount != _encoderSize)
                        throw new Exception($"Expected encoder size to be {_encoderSize} but found {encoderState.ColumnCount}");
                    encoderStates![i] = encoderState;
                }

                var input = sequence.Input?.GetMatrix();
                if (input == null)
                    throw new Exception("Not able to find the input matrix");
                if (input.ColumnCount != _inputSize)
                    throw new Exception($"Expected input size to be {_inputSize} but found {input.ColumnCount}");
                inputs[i] = input;
            }

            var numInputRows = previousBatch.BatchSize * previousBatch.SequenceCount;

            // create a big input matrix
            using var inputMatrix = _lap.CreateMatrix(numInputRows, _blockSize, false);
            var inputMatrixSegments = (IMatrixSegments)inputMatrix;
            for (uint i = 0; i < numInputRows; i++) {
                var row = inputMatrixSegments.Row(i);
                var sequenceIndex = i / previousBatch.BatchSize;
                inputs[sequenceIndex].Segment.CopyTo(row);
            }

            // find the per batch and input softmax
            using var output = inputMatrix.TransposeThisAndMultiply(_attention);
            var numInputs = (uint)inputs.Length;
            var numPointers = numInputRows / numInputs;
            var pointers = new TensorSegmentWrapper[numPointers];
            var outputSegment = output.Segment;
            for (uint i = 0; i < numPointers; i++)
                pointers[i] = new TensorSegmentWrapper(outputSegment, i, numPointers, numInputs);
            var softmax = _lap.MultiSoftmax(pointers);

            // construct the attention weights 
            using var inputWeight = _lap.CreateMatrix(inputMatrix.RowCount, inputMatrix.ColumnCount, (i, j) => 0f);

            return (this, signal, null);
        }

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
