using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using BrightData.Serialisation;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node.Input;

namespace BrightWire.ExecutionGraph.Node.Gate
{
    class SequenceToSequenceGate : NodeBase
    {
        string _encoderName, _decoderName;
        ConcurrentStack<IGraphContext>? _encoderContext;

        public SequenceToSequenceGate(string encoderName, string decoderName, string? name, string? id = null) : base(name, id)
        {
            _encoderName = encoderName;
            _decoderName = decoderName;
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            _encoderContext ??= new ConcurrentStack<IGraphContext>();
            _encoderContext.Push(context);

            if (context.BatchSequence.Type == MiniBatchSequenceType.SequenceEnd) {
                var nextBatch = context.BatchSequence.MiniBatch.NextMiniBatch;
                if (nextBatch == null)
                    throw new Exception("No following mini batch was found");

                // connect the hidden states of the encoder to the decoder
                var hiddenForward = context.GetData("hidden-forward").Single(d => d.Name == _encoderName);
                MemoryFeeder memoryFeeder;
                if (FindByName(_decoderName) is IHaveMemoryNode node)
                    memoryFeeder = (MemoryFeeder)node.Memory;
                else
                    throw new Exception($"{_decoderName} was not found or does not have a memory node");
                context.ExecutionContext.SetMemory(memoryFeeder.Id, hiddenForward.Data.GetMatrix());
                memoryFeeder.LoadNextFromMemory = true;

                context.ExecutionContext.RegisterAdditionalMiniBatch(nextBatch, signal, OnStartEncoder, OnEndDecoder);
            }

            return (this, GraphData.Null, null);
        }

        void OnStartEncoder(IGraphContext context, IGraphData data, CancellationToken ct)
        {
            foreach (var wire in Output)
                wire.SendTo.Forward(ct, data, context, wire.Channel);
        }

        void OnEndDecoder(IGraphContext[] context)
        {
            var firstContext = context.FirstOrDefault();
            var learningContext = firstContext?.LearningContext;
            if (learningContext != null) {
                var gradient = learningContext.BackpropagateThroughTime(null);

                if (gradient != null) {
                    // add the memory signal to the current 
                    var hiddenBackward = firstContext!.GetData("hidden-backward").Single(d => d.Name == _decoderName);
                    gradient.GetMatrix().AddInPlace(hiddenBackward.Data.GetMatrix());

                    // backpropagate the encoder
                    foreach (var item in _encoderContext!.Reverse())
                        learningContext.DeferBackpropagation(null, delta => item.Backpropagate(delta));
                    learningContext.BackpropagateThroughTime(gradient);
                }
            }

            _encoderContext?.Clear();
        }

        protected override (string Description, byte[] Data) GetInfo()
        {
            return ("S2S", WriteData(WriteTo));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            _encoderName.WriteTo(writer);
            _decoderName.WriteTo(writer);
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            _encoderName = reader.ReadString();
            _decoderName = reader.ReadString();
        }
    }
}
