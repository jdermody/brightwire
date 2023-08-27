using System;
using System.IO;

namespace BrightWire.ExecutionGraph.Node.Input
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

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                var matrix = errorSignal.GetMatrix();
                var (left, right) = matrix.SplitAtColumn(matrix.ColumnCount - _memorySize);
                right.Dispose();
                return errorSignal.ReplaceWith(left);
            }
        }
        string _slotName;

        public JoinSignalWithMemory(string slotName, string? name) : base(name)
        {
            _slotName = slotName;
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            var memory = context.ExecutionContext.GetMemory(_slotName);
            var output = signal.ReplaceWith(signal.GetMatrix().ConcatRight(memory));
            return (this, output, () => new Backpropagation(this, memory.ColumnCount));
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
