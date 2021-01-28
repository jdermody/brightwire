using System.Collections.Generic;
using System.Linq;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Engine.Helper;
using BrightWire.Models;
using System;
using BrightData;

namespace BrightWire.ExecutionGraph.Engine
{
	/// <summary>
	/// Executes (without training) graphs
	/// </summary>
    internal class ExecutionEngine : EngineBase, IGraphEngine
	{
		readonly List<(ExecutionEngineContext Context, IFloatMatrix[] Data)> _executionResults = new List<(ExecutionEngineContext, IFloatMatrix[])>();

		public ExecutionEngine(ILinearAlgebraProvider lap, ExecutionGraphModel graph, INode start) : base(lap)
		{
			Graph = graph;
			Start = start;
		}

		public INode Start { get; }
		public ExecutionGraphModel Graph { get; }
		public IDataSource? DataSource => _dataSource;
		public ILinearAlgebraProvider LinearAlgebraProvider => _lap;

		protected override void _ClearContextList()
		{
			// nop
		}

		protected override IEnumerable<ExecutionResult> _GetResults()
		{
            foreach (var item in _executionResults) {
				uint outputIndex = 0;
				foreach (var output in item.Data) {
					yield return new ExecutionResult(item.Context.BatchSequence, output.AsIndexable().Rows.Select(r => r.Data).ToArray(), outputIndex);
					++outputIndex;
				}
				item.Context.Dispose();
				foreach (var matrix in item.Data)
					matrix.Dispose();
			}
			_executionResults.Clear();
        }

		public IEnumerable<ExecutionResult> Execute(IDataSource dataSource, uint batchSize = 128, Action<float>? batchCompleteCallback = null)
		{
			_lap.PushLayer();
			_dataSource = dataSource;
			var ret = new List<ExecutionResult>();
			var provider = new MiniBatchProvider(dataSource, null);
			using (var executionContext = new ExecutionContext(_lap)) {
				executionContext.Add(provider.GetMiniBatches(batchSize, mb => _Execute(executionContext, mb)));
				float operationCount = executionContext.RemainingOperationCount;
				float index = 0f;

				IGraphOperation? operation;
				while ((operation = executionContext.GetNextOperation()) != null) {
					_lap.PushLayer();
					operation.Execute(executionContext);
					foreach (var item in _executionResults) {
						uint outputIndex = 0;
						foreach (var output in item.Data) {
							ret.Add(new ExecutionResult(item.Context.BatchSequence, output.AsIndexable().Rows.Select(r => r.Data).ToArray(), outputIndex));
							++outputIndex;
						}
						item.Context.Dispose();
						foreach (var matrix in item.Data)
							matrix.Dispose();
					}
					_executionResults.Clear();
					_lap.PopLayer();

					if (batchCompleteCallback != null) {
						var percentage = (++index) / operationCount;
						batchCompleteCallback(percentage);
					}
				}
			}
			_lap.PopLayer();
			_dataSource = null;
			return ret;
		}

		protected override void _Execute(IGraphExecutionContext executionContext, IMiniBatch batch)
		{
			var ret = new List<ExecutionEngineContext>();
			var table = new Dictionary<IMiniBatchSequence, IGraphContext>();

			if (batch.IsSequential) {
				IMiniBatchSequence? curr;
				while ((curr = batch.GetNextSequence()) != null) {
					var context = new ExecutionEngineContext(executionContext, curr);
					Start.ExecuteForward(context, 0);
					while (context.HasNext)
						context.ExecuteNext();
					ret.Add(context);
					table.Add(curr, context);
				}
			} else {
				var context = new ExecutionEngineContext(executionContext, batch.CurrentSequence);
				Start.ExecuteForward(context, 0);

				while (context.HasNext)
					context.ExecuteNext();

				ret.Add(context);
				table.Add(batch.CurrentSequence, context);
			}

			_Continue(batch, executionContext, sequence => table[sequence]);

			foreach (var item in ret) {
				var output = item.Output;
				_executionResults.Add((item, output.Any()
					? output.Select(o => o.GetMatrix()).ToArray()
					: new[] { item.Data.GetMatrix() }
				));
				item.Dispose();
			}
		}
	}
}
