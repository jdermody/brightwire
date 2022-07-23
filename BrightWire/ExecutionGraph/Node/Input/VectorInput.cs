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

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                var es = errorSignal.GetMatrix();

                using var columnSums = es.ColumnSums();
                columnSums.MultiplyInPlace(1f / es.RowCount);

                // store the updates
                var learningContext = context.LearningContext!;
                learningContext.AddError(ErrorType.Default, _source, columnSums);
                return errorSignal;
            }
        }

        readonly BrightDataContext _context;
        readonly float[] _data;

		public VectorInput(BrightDataContext context, float[] data, string? name = null, string? id = null) : base(name, id)
        {
            _context = context;
            _data = data;
        }

		public float[] Data => _data;

        public override void ApplyError(ErrorType type, ITensor delta, ILearningContext context)
        {
            var array = delta.Segment.GetLocalOrNewArray();
            for (uint j = 0; j < _data.Length; j++)
                _data[j] += array[j] * context.LearningRate;
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            var data = context.GetLinearAlgebraProvider().CreateMatrix(context.BatchSequence.MiniBatch.BatchSize, (uint)_data.Length, (_, y) => _data[y]);
            return (this, data.AsGraphData(), () => new Backpropagation(this));
        }

        protected override (string Description, byte[] Data) GetInfo()
		{
			return ("VI", WriteData(WriteTo));
		}

		public override void WriteTo(BinaryWriter writer)
		{
            _context.CreateReadOnlyVector(_data).WriteTo(writer);
		}

		public override void ReadFrom(GraphFactory factory, BinaryReader reader)
		{
            var temp = _context.LoadReadOnlyVectorFrom(reader);
            temp.Segment.CopyTo(_data);
		}
	}
}
