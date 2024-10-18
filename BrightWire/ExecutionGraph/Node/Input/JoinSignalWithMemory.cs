using System;
using System.IO;

namespace BrightWire.ExecutionGraph.Node.Input
{
    /// <summary>
    /// Joins the graph signal with a saved signal stored in named memory
    /// </summary>
    internal class JoinSignalWithMemory(string slotName, string? name) : NodeBase(name)
    {
        string _slotName = slotName;

        class Backpropagation(JoinSignalWithMemory source, uint memorySize) : SingleBackpropagationBase<JoinSignalWithMemory>(source)
        {
            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                var matrix = errorSignal.GetMatrix();
                var (left, right) = matrix.SplitAtColumn(matrix.ColumnCount - memorySize);
                right.Dispose();
                return errorSignal.ReplaceWith(left);
            }
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
