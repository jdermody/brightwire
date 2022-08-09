using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        IGradientDescentOptimisation _updater;
        IMatrix _attention;

        class Backpropagation : SingleBackpropagationBase<SelfAttention2>
        {
            readonly uint _position;
            readonly ITensor3D _inputTensor;
            readonly IMatrix _softmaxInputMatrix;
            readonly IMatrix _savedInputMatrix;

            public Backpropagation(SelfAttention2 source, uint position, ITensor3D inputTensor, IMatrix softmaxInputMatrix, IMatrix savedInputMatrix) : base(source)
            {
                _position = position;
                _inputTensor = inputTensor;
                _softmaxInputMatrix = softmaxInputMatrix;
                _savedInputMatrix = savedInputMatrix;

                Debug.Assert(_savedInputMatrix.IsEntirelyFinite());
                Debug.Assert(_softmaxInputMatrix.IsEntirelyFinite());
                Debug.Assert(_inputTensor.IsEntirelyFinite());
            }

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                var (left, right) = errorSignal.GetMatrix().SplitAtColumn(_position);
                Debug.Assert(right.IsEntirelyFinite());

                // train the attention layer
                var learningContext = context.LearningContext!;
                var lap = context.GetLinearAlgebraProvider();
                var errorVectors = new ITensorSegment[_inputTensor.Depth];
                for (uint i = 0, len = _inputTensor.Depth; i < len; i++) {
                    using var slice = _inputTensor.GetMatrix(i);
                    slice.PointwiseMultiplyInPlace(right);
                    var vector = slice.RowSums();
                    //vector.MultiplyInPlace(1f / slice.ColumnCount);
                    vector.ConstrainInPlace(-1f, 1f);
                    errorVectors[i] = vector.Segment;
                }

                var softmaxDerivative = lap.SoftmaxDerivativePerRow(_softmaxInputMatrix, errorVectors);
                var errorMatrix = lap.CreateMatrixFromRows(softmaxDerivative);
                _savedInputMatrix.MultiplyEachColumnWith(errorMatrix.Segment);
                var attentionError = _savedInputMatrix.ColumnSums();
                attentionError.MultiplyInPlace(1f / _savedInputMatrix.RowCount);

                Debug.Assert(attentionError.IsEntirelyFinite());
                learningContext.AddError(ErrorType.Default, _source, attentionError.Reshape(_source._attention.RowCount, _source._attention.ColumnCount));
                errorVectors.DisposeAll();
                return left.AsGraphData();
            }
        }

        public SelfAttention2(
            LinearAlgebraProvider lap, 
            string encoderName, 
            string decoderName,
            uint inputSize, 
            uint encoderSize, 
            uint decoderSize,
            IWeightInitialisation weightInit,
            Func<IMatrix, IGradientDescentOptimisation> updater,
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
            _updater = updater(_attention);
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            var currentIndex = context.BatchSequence.SequenceIndex;
            var batchSize = context.BatchSequence.MiniBatch.BatchSize;

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
            Debug.Assert(batchSize == previousBatch.BatchSize);
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

            var numInputRows = batchSize * previousBatch.SequenceCount;

            // create a big input matrix
            using var inputMatrix = _lap.CreateMatrix(numInputRows, _blockSize, false);
            uint offset = 0;
            for (uint i = 0; i < numInputRows; i++) {
                var sequenceIndex = i / batchSize;
                var batchIndex = i % batchSize;
                var inputRow = inputMatrix.Row(i);
                inputs[sequenceIndex].Row(batchIndex).CopyTo(inputRow, 0, 0);
                encoderStates?[sequenceIndex].Row(batchIndex).CopyTo(inputRow, 0, _inputSize);
                decoderHiddenState?.Row(batchIndex).CopyTo(inputRow, 0, _inputSize + _encoderSize);
            }

            // find the per batch softmax
            using var output = inputMatrix.TransposeAndMultiply(_attention);
            var numInputs = (uint)inputs.Length;
            var pointers = new TensorSegmentWrapper[batchSize];
            var outputSegment = output.Segment;
            for (uint i = 0; i < batchSize; i++)
                pointers[i] = new TensorSegmentWrapper(outputSegment, i, batchSize, numInputs);
            var softmax = _lap.MultiSoftmax(pointers);
            var softmaxMatrix = _lap.CreateMatrixFromColumns(pointers.Cast<ITensorSegment>().ToArray());

            // construct the final attention output by multiplying attention weights
            var savedInputMatrix = inputMatrix.Clone();
            using var weightMatrix = _lap.CreateMatrixFromRows(softmax);
            var inputTensor = inputMatrix.Reshape(null, batchSize, inputMatrix.ColumnCount);
            for (uint i = 0, len = inputTensor.Depth; i < len; i++) {
                using var slice = inputTensor.GetMatrix(i);
                var column = weightMatrix.Column(i);
                slice.MultiplyEachColumnWith(column);
            }
            using var combined = inputTensor.AddMatrices();
            combined.MultiplyInPlace(1f / inputTensor.Depth);
            combined.ConstrainInPlace(-1f, 1f);

            // append the attention output to the existing input signal
            var final = signal.GetMatrix().ConcatRight(combined);
            return (this, final.AsGraphData(), () => new Backpropagation(this, signal.Columns, inputTensor, softmaxMatrix, savedInputMatrix));
        }

        public override void ApplyError(ErrorType type, ITensor delta, ILearningContext context)
        {
            _updater.Update(_attention, (IMatrix)delta, context);
        }

        protected override (string Description, byte[] Data) GetInfo()
        {
            return ("SA", WriteData(WriteTo));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            _encoderName.WriteTo(writer);
            _decoderName.WriteTo(writer);
            _inputSize.WriteTo(writer);
            _encoderSize.WriteTo(writer);
            _decoderSize.WriteTo(writer);
            _attention.WriteTo(writer);
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            _lap = factory.LinearAlgebraProvider;
            _encoderName = reader.ReadString();
            _decoderName = reader.ReadString();
            _inputSize = reader.ReadUInt32();
            _encoderSize = reader.ReadUInt32();
            _decoderSize = reader.ReadUInt32();
            _blockSize = _inputSize + _encoderSize + _decoderSize;

            var attention = factory.Context.ReadMatrixFrom(reader);
            if (_attention == null)
                _attention = attention.Create(_lap);
            else
                attention.CopyTo(_attention);
            _updater ??= factory.CreateWeightUpdater(_attention);
        }
    }
}
