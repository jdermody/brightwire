using System;
using System.IO;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Input
{
    internal class VectorInput : NodeBase
	{
		class Backpropagation : SingleBackpropagationBase<VectorInput>
		{
			public Backpropagation(VectorInput source) : base(source)
			{
			}

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphSequenceContext context)
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
                return errorSignal;
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

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphSequenceContext context, NodeBase? source)
        {
            var data = context.LinearAlgebraProvider.CreateMatrix(context.BatchSequence.MiniBatch.BatchSize, (uint)_data.Length, (_, y) => _data[y]);
            return (this, data.AsGraphData(), () => new Backpropagation(this));
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
