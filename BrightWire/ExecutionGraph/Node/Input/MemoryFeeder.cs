using System;
using System.Collections.Generic;
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
        class Backpropagation : BackpropagationBase<MemoryFeeder>
        {
            public Backpropagation(MemoryFeeder source) : base(source)
            {
            }

            public override IEnumerable<(IGraphData Signal, IGraphSequenceContext Context, NodeBase ToNode)> Backward(IGraphData errorSignal, IGraphSequenceContext context, NodeBase[] parents)
            {
                var es = errorSignal.GetMatrix();
                using var columnSums = es.ColumnSums();
                columnSums.Multiply(1f / es.RowCount);
                var initialDelta = columnSums.AsIndexable();
                for (uint j = 0; j < _source._data.Length; j++)
                    _source._data[j] += initialDelta[j] * context.LearningContext!.BatchLearningRate;

                if(_source._contextName != null)
                    context.SetData(_source._contextName, "hidden-backward", errorSignal);
                return ErrorTo(GraphData.Null, context, parents);
            }
        }

        readonly IBrightDataContext _context;
        readonly float[] _data;
	    readonly SetMemory _setMemory;
        readonly string? _contextName;

        public MemoryFeeder(IBrightDataContext context, float[] data, string? contextName, string? name = null, string? id = null) : base(name, id)
        {
            _context = context;
            _data = data;
            _contextName = contextName;
            _setMemory = new SetMemory(Id, contextName);
        }

        public IAction SetMemoryAction => _setMemory;
        public Vector<float> Data
        {
            get => _context.CreateVector(_data);
	        set => value.Segment.CopyTo(_data);
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphSequenceContext context, NodeBase? source)
        {
            IFloatMatrix memory;
            if (context.BatchSequence.Type == MiniBatchSequenceType.SequenceStart) {
                memory = context.LinearAlgebraProvider.CreateMatrix(context.BatchSequence.MiniBatch.BatchSize, (uint)_data.Length, (x, y) => _data[y]);
                context.ExecutionContext.SetMemory(Id, memory);
            }
            else
                memory = context.ExecutionContext.GetMemory(Id);

            var next = new MatrixGraphData(memory);
            return (this, next, () => new Backpropagation(this));
        }

        protected override (string Description, byte[] Data) GetInfo()
        {
            return ("MF", WriteData(WriteTo));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            Data.WriteTo(writer);
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            Data = _context.ReadVectorFrom(reader);
        }
    }
}
