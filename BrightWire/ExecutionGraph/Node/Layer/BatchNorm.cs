using System;
using BrightWire.ExecutionGraph.Node.Input;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightData;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node.Operation;
using BrightWire.Helper;

namespace BrightWire.ExecutionGraph.Node.Layer
{
	/// <summary>
	/// Batch normalisation layer
	/// https://arxiv.org/abs/1502.03167
	/// </summary>
    internal class BatchNorm : NodeBase
	{
		uint _inputSize;
		NodeBase _input, _output;
		VectorInput _gamma, _beta;
		VectorBasedStatistics _statistics;
		OneToMany _start;
		IMatrix? _gammaCached, _betaCached, _meanCached, _stdDevCached;

#pragma warning disable 8618
        public BatchNorm(GraphFactory graph, uint inputSize, string? name = null) : base(name)
#pragma warning restore 8618
        {
			Create(graph, inputSize, null, null, null, null, 0);
		}

		void Create(GraphFactory graph, uint inputSize, float[]? gamma, float[]? beta, float[]? derivedMean, float[]? derivedM2, uint count)
        {
            _inputSize = inputSize;
	        _statistics = new VectorBasedStatistics(graph.LinearAlgebraProvider, inputSize, derivedMean, derivedM2, count);

	        if (gamma == null) {
		        gamma = new float[inputSize];
		        for (var i = 0; i < inputSize; i++)
			        gamma[i] = 1f;
	        }
	        beta ??= new float[inputSize];

	        _input = new FlowThrough("input");
	        _gamma = new VectorInput(graph.Context, gamma, "gamma");
	        _beta = new VectorInput(graph.Context, beta, "beta");

	        var mean = graph.Connect(inputSize, _input).Add(new BatchMean());
	        var subtractMean = graph.Subtract(inputSize, _input, mean.LastNode!);
			var subtractMeanOutput = subtractMean.LastNode!;

	        var stdDev = subtractMean
		        .Add(new InputSquared())
		        .Add(new BatchMean())
		        .Add(new SquareRootOfInput())
		        .Add(new OneDividedByInput());

            var normalised = graph.Multiply(_inputSize, subtractMeanOutput, stdDev.LastNode!);
	        var adjusted = graph.Multiply(_inputSize, _gamma, normalised.LastNode!);
	        _output = graph.Add(_inputSize, _beta, adjusted.LastNode!, Name != null ? $"{Name}_last" : null).LastNode!;

	        _start = new OneToMany(SubNodes, Name != null ? $"{Name}_start" : null);
        }

		public override IEnumerable<NodeBase> SubNodes
		{
			get
			{
				yield return _input;
				yield return _gamma;
				yield return _beta;
			}
		}

		public override List<WireToNode> Output => _output.Output;

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            IMatrix input;
            IReadOnlyVector[]? samples;
            var lap = context.GetLinearAlgebraProvider();
            var shouldDispose = false;
            (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ret = (this, GraphData.Null, null);

			if (signal.Depth > 1) {
                throw new NotImplementedException();
				// reshape the tensor by depth slice
				//var tensor4D = data.Get4DTensor();
				//var slots = new List<IFloatVector>[data.Depth];
				//for (var i = 0; i < data.Depth; i++)
				//	slots[i] = new List<IFloatVector>();

				//foreach (var tensor in tensor4D.ReshapeAsMatrix().ColumnVectors()) {
				//	var depthSlices = tensor.ReshapeAsColumnMatrix().ReshapeAsVector().Split(data.Depth);
				//	for (var i = 0; i < data.Depth; i++)
				//		slots[i].Add(depthSlices[i]);
				//}

				//// TODO: create row vectors from slots and matrix from rows

				////input = lap.CreateMatrixFromRows(list);
				////samples = list;
				//shouldDispose = true;
            }
			else {
				input = signal.GetMatrix();
				samples = input.AllRows(false);
			}

			// collect statistics
			if (context.LearningContext != null) {
				foreach (var vector in samples) {
                    using var temp = vector.Create(lap);
					_statistics.Update(temp);
                }
			}

			if (context.LearningContext != null) {
				ret = _start.ForwardSingleStep(signal, channel, context, source);

				// invalidate the cache
				if (_gammaCached != null) {
					_gammaCached.Dispose();
					_gammaCached = null;
					_betaCached?.Dispose();
					_betaCached = null;
					_meanCached?.Dispose();
					_meanCached = null;
					_stdDevCached?.Dispose();
					_stdDevCached = null;
				}
			}
			else if(_statistics != null) {
                if (_gammaCached?.Segment.IsValid != true || _gammaCached?.RowCount != input.RowCount || _gammaCached?.ColumnCount != input.ColumnCount)
					_gammaCached = lap.CreateMatrix(input.RowCount, input.ColumnCount, (_, y) => _gamma.Data[y]);
				if(_betaCached?.Segment.IsValid != true || _betaCached?.RowCount != input.RowCount || _betaCached?.ColumnCount != input.ColumnCount)
					_betaCached = lap.CreateMatrix(input.RowCount, input.ColumnCount, (_, y) => _beta.Data[y]);
				if (_meanCached?.Segment.IsValid != true || _meanCached?.RowCount != input.RowCount || _meanCached?.ColumnCount != input.ColumnCount) {
					var mean = _statistics.Mean;
					_meanCached = lap.CreateMatrixFromRows(Enumerable.Repeat(mean, (int)input.RowCount).ToArray());
				}
				if (_stdDevCached?.Segment.IsValid != true || _stdDevCached?.RowCount != input.RowCount || _stdDevCached?.ColumnCount != input.ColumnCount) {
                    using var variance = _statistics.GetSampleVariance();
                    using var stdDev = variance.Sqrt();
                    _stdDevCached = lap.CreateMatrixFromRows(Enumerable.Repeat(stdDev, (int)input.RowCount).ToArray());
                }
				
				input.SubtractInPlace(_meanCached);
                using var xHat = input.PointwiseDivide(_stdDevCached);
                var output = xHat.PointwiseMultiply(_gammaCached);
                output.AddInPlace(_betaCached);
                ret = (this, signal.ReplaceWith(output), null);
            }

			if (shouldDispose)
				input.Dispose();
            return ret;
        }

        protected override (string Description, byte[] Data) GetInfo()
		{
			return ("BN", WriteData(WriteTo));
		}

		public override void WriteTo(BinaryWriter writer)
		{
			writer.Write(_inputSize);
			_gamma.WriteTo(writer);
			_beta.WriteTo(writer);

			writer.Write(_statistics.Count);
			_statistics.Mean.WriteTo(writer);
			_statistics.M2.WriteTo(writer);
		}

		public override void ReadFrom(GraphFactory factory, BinaryReader reader)
		{
			var inputSize = (uint)reader.ReadInt32();
            var context = factory.Context;
			var gamma = context.LoadReadOnlyVectorAndThenGetArrayFrom(reader);
			var beta = context.LoadReadOnlyVectorAndThenGetArrayFrom(reader);

			var count = (uint)reader.ReadInt32();
			var mean = context.LoadReadOnlyVectorAndThenGetArrayFrom(reader);
			var m2 = context.LoadReadOnlyVectorAndThenGetArrayFrom(reader);

			Create(factory, inputSize, gamma, beta, mean, m2, count);
		}
	}
}
