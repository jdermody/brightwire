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

            public override void BackwardInternal(INode? fromNode, IGraphData errorSignal, IGraphSequenceContext context, INode[] parents)
            {
                if (context.BatchSequence.Type == MiniBatchSequenceType.SequenceStart) {
                    var es = errorSignal.GetMatrix();

                    using var columnSums = es.ColumnSums();
                    columnSums.Multiply(1f / es.RowCount);
                    var initialDelta = columnSums.AsIndexable();
                    for (uint j = 0; j < _source._data.Length; j++)
                        _source._data[j] += initialDelta[j] * context.LearningContext!.BatchLearningRate;
                }
                SendErrorTo(null, context, parents);
            }
        }

        readonly IBrightDataContext _context;
        readonly float[] _data;
	    readonly SetMemory _setMemory;

        public MemoryFeeder(IBrightDataContext context, float[] data, string? name = null, string? id = null) : base(name, id)
        {
            _context = context;
            _data = data;
            _setMemory = new SetMemory(Id);
        }

        public IAction SetMemoryAction => _setMemory;
        public Vector<float> Data
        {
            get => _context.CreateVector(_data);
	        set => value.Segment.CopyTo(_data);
        }

        public override void ExecuteForward(IGraphSequenceContext context)
        {
            if (context.BatchSequence.Type == MiniBatchSequenceType.SequenceStart) {
                var memory = context.LinearAlgebraProvider.CreateMatrix(context.BatchSequence.MiniBatch.BatchSize, (uint)_data.Length, (x, y) => _data[y]);
                context.ExecutionContext.SetMemory(Id, memory);
                AddNextGraphAction(context, new MatrixGraphData(memory), () => new Backpropagation(this));
            } 
            else {
                var memory = context.ExecutionContext.GetMemory(Id);
                AddNextGraphAction(context, new MatrixGraphData(memory), null);
            }
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
