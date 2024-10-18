using System;
using System.IO;
using System.Linq;
using BrightData.Helper;
using BrightWire.ExecutionGraph.Node.Input;

namespace BrightWire.ExecutionGraph.Node.Helper
{
    /// <summary>
    /// Connects the hidden states across two recurrent neural network nodes
    /// </summary>
    class RecurrentBridge(string fromName, string toName, string? name, string? id = null)
        : NodeBase(name, id)
    {
        string _fromName = fromName;
        string _toName = toName;

        class Backpropagation(RecurrentBridge source) : SingleBackpropagationBase<RecurrentBridge>(source)
        {
            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                var hiddenBackward = context.GetData("hidden-backward").Single(d => d.Name == _source._toName);

                // add the memory signal to the current 
                errorSignal.GetMatrix().AddInPlace(hiddenBackward.Data.GetMatrix());

                return errorSignal;
            }
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            // connect the hidden states
            var hiddenForward = context.GetData("hidden-forward").Single(d => d.Name == _fromName);
            MemoryFeeder memoryFeeder;
            if (FindByName(_toName) is IHaveMemoryNode node)
                memoryFeeder = (MemoryFeeder)node.Memory;
            else
                throw new Exception($"{_fromName} was not found or does not have a memory node");

            context.ExecutionContext.SetMemory(memoryFeeder.Id, hiddenForward.Data.GetMatrix());
            memoryFeeder.LoadNextFromMemory = true;

            return (this, signal, () => new Backpropagation(this));
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
