using System;
using System.IO;
using BrightWire.ExecutionGraph.Node;

namespace BrightWire.ExecutionGraph.Action
{
    /// <summary>
    /// Joins the graph signal with a saved signal stored in named memory
    /// </summary>
    internal class JoinSignalWithMemory : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<JoinSignalWithMemory>
        {
            readonly uint _memorySize;

            public Backpropagation(JoinSignalWithMemory source, uint memorySize) : base(source)
            {
                _memorySize = memorySize;
            }

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphSequenceContext context)
            {
                var matrix = errorSignal.GetMatrix();
                var parts = matrix.SplitAtColumn(matrix.ColumnCount - _memorySize);
                parts.Right.Dispose();
                return errorSignal.ReplaceWith(parts.Left);
            }
        }
        string _slotName;

        public JoinSignalWithMemory(string slotName, string? name) : base(name)
        {
            _slotName = slotName;
        }

        public override void ExecuteForward(IGraphSequenceContext context)
        {
            var memory = context.ExecutionContext.GetMemory(_slotName);
            var data = context.Data;
            var output = data.ReplaceWith(data.GetMatrix().ConcatRows(memory));
            AddNextGraphAction(context, output, () => new Backpropagation(this, memory.ColumnCount));
        }

        public override (IGraphData Next, Func<IBackpropagate>? BackProp) Forward(IGraphData signal, uint channel, IGraphSequenceContext context, INode? source)
        {
            var memory = context.ExecutionContext.GetMemory(_slotName);
            var output = signal.ReplaceWith(signal.GetMatrix().ConcatRows(memory));
            return (output, () => new Backpropagation(this, memory.ColumnCount));
        }

        protected override (string Description, byte[] Data) GetInfo()
        {
            return ("JSWM", WriteData(WriteTo));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(_slotName);
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            _slotName = reader.ReadString();
        }
    }
}
