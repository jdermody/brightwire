﻿using System;
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

            public override IEnumerable<(IGraphData Signal, INode ToNode)> Backward(IGraphData errorSignal, IGraphSequenceContext context, INode[] parents)
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

        public override (IGraphData Next, Func<IBackpropagate>? BackProp) Forward(IGraphData signal, uint channel, IGraphSequenceContext context, INode? source)
        {
            var data = context.LinearAlgebraProvider.CreateMatrix(context.BatchSequence.MiniBatch.BatchSize, (uint)_data.Length, (x, y) => _data[y]);
            return (new MatrixGraphData(data), () => new Backpropagation(this));
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
