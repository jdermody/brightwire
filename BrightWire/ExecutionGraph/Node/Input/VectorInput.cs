using System;
using System.IO;
using BrightData;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightWire.ExecutionGraph.Node.Input
{
    internal class VectorInput(BrightDataContext context, float[] data, string? name = null, string? id = null)
        : NodeBase(name, id)
    {
		class Backpropagation(VectorInput source) : SingleBackpropagationBase<VectorInput>(source)
        {
            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                var es = errorSignal.GetMatrix();

                using var columnSums = es.ColumnSums();
                columnSums.MultiplyInPlace(1f / es.RowCount);

                // store the updates
                var learningContext = context.LearningContext!;
                learningContext.AddError(NodeErrorType.Default, _source, columnSums);
                return errorSignal;
            }
        }

        public float[] Data => data;

        public override void ApplyError(NodeErrorType type, ITensor<float> delta, ILearningContext learningContext)
        {
            var temp = SpanOwner<float>.Empty;
            var array = delta.Segment.GetSpan(ref temp, out var wasTempUsed);
            try {
                for (var j = 0; j < data.Length; j++)
                    data[j] += array[j] * learningContext.LearningRate;
            }
            finally {
                if(wasTempUsed)
                    temp.Dispose();
            }
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext graphContext, NodeBase? source)
        {
            var data1 = graphContext.GetLinearAlgebraProvider().CreateMatrix(graphContext.BatchSequence.MiniBatch.BatchSize, (uint)data.Length, (_, y) => data[y]);
            return (this, data1.AsGraphData(), () => new Backpropagation(this));
        }

        protected override (string Description, byte[] Data) GetInfo()
		{
			return ("VI", WriteData(WriteTo));
		}

		public override void WriteTo(BinaryWriter writer)
		{
            context.CreateReadOnlyVector(data).WriteTo(writer);
		}

		public override void ReadFrom(GraphFactory factory, BinaryReader reader)
		{
            var temp = context.LoadReadOnlyVectorFrom(reader);
            temp.ReadOnlySegment.CopyTo(data);
		}
	}
}
