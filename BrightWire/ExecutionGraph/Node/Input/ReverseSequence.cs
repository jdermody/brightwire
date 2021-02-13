using System;
using BrightWire.ExecutionGraph.Helper;
using System.IO;

namespace BrightWire.ExecutionGraph.Node.Input
{
    /// <summary>
    /// Inputs the opposite sequential item from the input index (for bidirectional recurrent neural networks)
    /// https://en.wikipedia.org/wiki/Bidirectional_recurrent_neural_networks
    /// </summary>
    internal class ReverseSequence : NodeBase
    {
        int _inputIndex;

        public ReverseSequence(int inputIndex = 0, string? name = null) : base(name)
        {
            _inputIndex = inputIndex;
        }

        public override void ExecuteForward(IGraphSequenceContext context)
        {
            if (_inputIndex == 0) {
                var curr = context.BatchSequence;
                var batch = curr.MiniBatch;
                var reversed = batch.GetSequenceAtIndex(batch.SequenceCount - curr.SequenceIndex - 1).Input;
                if (reversed == null)
                    throw new Exception("Input data was null");

                //context.AddForward(new ExecutionHistory(this, reversed, context.Source), null);
            }
            else 
                throw new NotImplementedException();
        }

        public override (IGraphData Next, Func<IBackpropagate>? BackProp) Forward(IGraphData signal, uint channel, IGraphSequenceContext context, INode? source)
        {
            if (_inputIndex == 0) {
                var curr = context.BatchSequence;
                var batch = curr.MiniBatch;
                var reversed = batch.GetSequenceAtIndex(batch.SequenceCount - curr.SequenceIndex - 1).Input;
                if (reversed == null)
                    throw new Exception("Input data was null");

                //context.AddForward(new ExecutionHistory(this, reversed, context.Source), null);
                return (reversed, null);
            }
            else 
                throw new NotImplementedException();
        }

        protected override (string Description, byte[] Data) GetInfo()
        {
            return ("RS", WriteData(WriteTo));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(_inputIndex);
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            _inputIndex = reader.ReadInt32();
        }
    }
}
