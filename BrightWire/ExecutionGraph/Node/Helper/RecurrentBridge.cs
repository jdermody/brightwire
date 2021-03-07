using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightData.Serialisation;
using BrightWire.ExecutionGraph.Node.Input;

namespace BrightWire.ExecutionGraph.Node.Helper
{
    class RecurrentBridge : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<RecurrentBridge>
        {
            readonly MemoryFeeder _memoryFeeder;

            public Backpropagation(RecurrentBridge source, MemoryFeeder memoryFeeder) : base(source)
            {
                _memoryFeeder = memoryFeeder;
            }

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphSequenceContext context)
            {
                var hiddenBackward = context.GetData("hidden-backward").Single(d => d.Name == _source._toName);

                // add the memory signal to the current 
                errorSignal.GetMatrix().AddInPlace(hiddenBackward.Data.GetMatrix());

                return errorSignal;
            }
        }
        string _fromName, _toName;

        public RecurrentBridge(string fromName, string toName, string? name, string? id = null) : base(name, id)
        {
            _fromName = fromName;
            _toName = toName;
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphSequenceContext context, NodeBase? source)
        {
            var hiddenForward = context.GetData("hidden-forward").Single(d => d.Name == _fromName);
            MemoryFeeder memoryFeeder;
            if (FindByName(_toName) is IHaveMemoryNode node)
                memoryFeeder = (MemoryFeeder)node.Memory;
            else
                throw new Exception($"{_fromName} was not found or does not have a memory node");

            context.ExecutionContext.SetMemory(memoryFeeder.Id, hiddenForward.Data.GetMatrix());
            memoryFeeder.LoadNextFromMemory = true;

            return (this, signal, () => new Backpropagation(this, memoryFeeder));
        }

        protected override (string Description, byte[] Data) GetInfo()
        {
            return ("RB", WriteData(WriteTo));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            _fromName.WriteTo(writer);
            _toName.WriteTo(writer);
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            _fromName = reader.ReadString();
            _toName = reader.ReadString();
        }
    }
}
