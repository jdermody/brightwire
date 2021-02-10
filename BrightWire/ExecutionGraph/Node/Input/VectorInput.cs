using System.Collections.Generic;
using BrightWire.ExecutionGraph.Helper;
using System.IO;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Input
{
    internal class VectorInput : NodeBase
	{
		class Backpropagation : BackpropagationBase<VectorInput>
		{
			public Backpropagation(VectorInput source) : base(source)
			{
			}

			public override void BackwardInternal(INode? fromNode, IGraphData errorSignal, IGraphSequenceContext context, INode[] parents)
			{
				var es = errorSignal.GetMatrix();

                using var columnSums = es.ColumnSums();
                columnSums.Multiply(1f / es.RowCount);

                // store the updates
                var learningContext = context.LearningContext!;
                learningContext.StoreUpdate(_source, columnSums, err => {
                    var delta = err.AsIndexable();
                    for (uint j = 0; j < _source._data.Length; j++)
                        _source._data[j] += delta[j] * learningContext.BatchLearningRate;
                });
                SendErrorTo(errorSignal, context, parents);
            }

            public override IEnumerable<(IGraphData signal, INode toNode)> Backward(IGraphData errorSignal, IGraphSequenceContext context, INode[] parents)
            {
                var es = errorSignal.GetMatrix();

                using var columnSums = es.ColumnSums();
                columnSums.Multiply(1f / es.RowCount);

                // store the updates
                var learningContext = context.LearningContext!;
                learningContext.StoreUpdate(_source, columnSums, err => {
                    var delta = err.AsIndexable();
                    for (uint j = 0; j < _source._data.Length; j++)
                        _source._data[j] += delta[j] * learningContext.BatchLearningRate;
                });
                return ErrorTo(errorSignal, parents);
            }
        }

        readonly IBrightDataContext _context;
        readonly float[] _data;

		public VectorInput(IBrightDataContext context, float[] data, string? name = null, string? id = null) : base(name, id)
        {
            _context = context;
            _data = data;
        }

		public float[] Data => _data;

		public override void ExecuteForward(IGraphSequenceContext context)
		{
			var data = context.LinearAlgebraProvider.CreateMatrix(context.BatchSequence.MiniBatch.BatchSize, (uint)_data.Length, (x, y) => _data[y]);
			AddNextGraphAction(context, new MatrixGraphData(data), () => new Backpropagation(this));
		}

		protected override (string Description, byte[] Data) GetInfo()
		{
			return ("VI", WriteData(WriteTo));
		}

		public override void WriteTo(BinaryWriter writer)
		{
            _context.CreateVector(_data).WriteTo(writer);
		}

		public override void ReadFrom(GraphFactory factory, BinaryReader reader)
		{
            _context.ReadVectorFrom(reader).Segment.CopyTo(_data);
		}
	}
}
