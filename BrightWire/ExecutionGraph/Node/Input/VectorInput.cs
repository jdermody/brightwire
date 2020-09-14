using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BrightData;
using BrightData.FloatTensors;
using BrightWire.Models;

namespace BrightWire.ExecutionGraph.Node.Input
{
	class VectorInput : NodeBase
	{
		class Backpropagation : BackpropagationBase<VectorInput>
		{
			public Backpropagation(VectorInput source) : base(source)
			{
			}

			public override void _Backward(INode fromNode, IGraphData errorSignal, IContext context, INode[] parents)
			{
				var es = errorSignal.GetMatrix();

                using var columnSums = es.ColumnSums();
                columnSums.Multiply(1f / es.RowCount);

                // store the updates
                var learningContext = context.LearningContext;
                learningContext.StoreUpdate(_source, columnSums, err => {
                    var delta = err.AsIndexable();
                    for (uint j = 0; j < _source._data.Length; j++)
                        _source._data[j] += delta[j] * context.LearningContext.BatchLearningRate;
                });
            }
		}

        private readonly IBrightDataContext _context;
        float[] _data;

		public VectorInput(IBrightDataContext context, float[] data, string name = null, string id = null) : base(name, id)
        {
            _context = context;
            _data = data;
        }

		public float[] Data => _data;

		public override void ExecuteForward(IContext context)
		{
			var data = context.LinearAlgebraProvider.CreateMatrix(context.BatchSequence.MiniBatch.BatchSize, (uint)_data.Length, (x, y) => _data[y]);
			_AddNextGraphAction(context, new MatrixGraphData(data), () => new Backpropagation(this));
		}

		protected override (string Description, byte[] Data) _GetInfo()
		{
			return ("VI", _WriteData(WriteTo));
		}

		public override void WriteTo(BinaryWriter writer)
		{
			FloatVector.Create(_context, _data).WriteTo(writer);
		}

		public override void ReadFrom(GraphFactory factory, BinaryReader reader)
		{
			FloatVector.ReadFrom(_context, reader).Segment.CopyTo(_data);
		}
	}
}
