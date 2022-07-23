using System;
using System.Diagnostics;
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

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                var es = errorSignal.GetMatrix();
                using var columnSums = es.ColumnSums();
                using var initialDelta = columnSums.Multiply(1f / es.RowCount);
                var learningRate = context.LearningContext!.LearningRate;
                for (uint j = 0; j < _source.Data.Length; j++) {
                    _source.Data[j] += initialDelta[j] * learningRate;
                }
                #if DEBUG
                foreach(var item in _source.Data)
                    Debug.Assert(!float.IsNaN(item));
                #endif

                if(_source._contextName != null)
                    context.SetData(_source._contextName, "hidden-backward", errorSignal);
                return GraphData.Null;
            }
        }

        readonly BrightDataContext _context;
        readonly SetMemory _setMemory;
        readonly string? _contextName;

        public MemoryFeeder(BrightDataContext context, float[] data, string? contextName, string? name = null, string? id = null) : base(name, id)
        {
            _context = context;
            Data = data;
            _contextName = contextName;
            _setMemory = new SetMemory(Id, contextName);
        }

        public IAction SetMemoryAction => _setMemory;
        public float[] Data { get; set; }

        public bool LoadNextFromMemory { get; set; } = false;

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            IMatrix memory;
            if (!LoadNextFromMemory && context.BatchSequence.Type == MiniBatchSequenceType.SequenceStart) {
                memory = context.GetLinearAlgebraProvider().CreateMatrix(context.BatchSequence.MiniBatch.BatchSize, (uint)Data.Length, (_, y) => Data[y]);
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
            using var temp = _context.LinearAlgebraProvider.CreateVector(Data);
            temp.WriteTo(writer);
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            Data = _context.LoadReadOnlyVectorAndThenGetArrayFrom(reader);
        }
    }
}
