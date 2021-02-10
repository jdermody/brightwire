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

            protected override IGraphData Backpropagate(INode? fromNode, IGraphData errorSignal, IGraphSequenceContext context, INode[] parents)
            {
                var matrix = errorSignal.GetMatrix();
                var parts = matrix.SplitAtColumn(matrix.RowCount - _memorySize);
                return errorSignal.ReplaceWith(parts.Left);
            }

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphSequenceContext context)
            {
                var matrix = errorSignal.GetMatrix();
                var parts = matrix.SplitAtColumn(matrix.RowCount - _memorySize);
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
            AddNextGraphAction(context, output, () => new Backpropagation(this, memory.RowCount));
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
