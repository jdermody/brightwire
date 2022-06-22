using System;
using BrightWire.ExecutionGraph.Helper;
using System.IO;
using BrightData;
using BrightData.LinearAlgebra;
using BrightWire.ExecutionGraph.Action;

namespace BrightWire.ExecutionGraph.Node.Input
{
    /// <summary>
    /// Feeds memory into the graph from a named memory slot
    /// </summary>
    internal class MemoryFeeder : NodeBase, IMemoryNode
    {
        class Backpropagation : SingleBackpropagationBase<MemoryFeeder>
        {
            public Backpropagation(MemoryFeeder source) : base(source)
            {
            }

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphSequenceContext context)
            {
                var es = errorSignal.GetMatrix();
                using var columnSums = es.ColumnSums();
                columnSums.Multiply(1f / es.RowCount);
                var initialDelta = columnSums;
                for (uint j = 0; j < _source.Data.Length; j++)
                    _source.Data[j] += initialDelta[j] * context.LearningContext!.BatchLearningRate;

                if(_source._contextName != null)
                    context.SetData(_source._contextName, "hidden-backward", errorSignal);
                return GraphData.Null;
            }
        }

        readonly IBrightDataContext _context;
        readonly SetMemory _setMemory;
        readonly string? _contextName;

        public MemoryFeeder(IBrightDataContext context, float[] data, string? contextName, string? name = null, string? id = null) : base(name, id)
        {
            _context = context;
            Data = data;
            _contextName = contextName;
            _setMemory = new SetMemory(Id, contextName);
        }

        public IAction SetMemoryAction => _setMemory;
        public float[] Data { get; set; }

        public bool LoadNextFromMemory { get; set; }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphSequenceContext context, NodeBase? source)
        {
            IMatrix memory;
            if (!LoadNextFromMemory && context.BatchSequence.Type == MiniBatchSequenceType.SequenceStart) {
                memory = context.LinearAlgebraProvider.CreateMatrix(context.BatchSequence.MiniBatch.BatchSize, (uint)Data.Length, (_, y) => Data[y]);
                context.ExecutionContext.SetMemory(Id, memory);
            }
            else {
                memory = context.ExecutionContext.GetMemory(Id);
                LoadNextFromMemory = false;
            }

            return (this, memory.AsGraphData(), () => new Backpropagation(this));
        }

        protected override (string Description, byte[] Data) GetInfo()
        {
            return ("MF", WriteData(WriteTo));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            using var temp = _context.LinearAlgebraProvider2.CreateVector(Data);
            temp.WriteTo(writer);
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            Data = _context.ReadVectorAndThenGetArrayFrom(reader);
        }
    }
}
