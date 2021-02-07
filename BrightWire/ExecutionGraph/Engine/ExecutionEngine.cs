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
    internal class ExecutionEngine : EngineBase<ExecutionEngineContext>, IGraphEngine
	{
        public ExecutionEngine(ILinearAlgebraProvider lap, ExecutionGraphModel graph, INode start) : base(lap)
		{
			Graph = graph;
			Start = start;
		}

        public INode Start { get; }
        public ExecutionGraphModel Graph { get; }
		public IDataSource? DataSource => _dataSource;
		public ILinearAlgebraProvider LinearAlgebraProvider => _lap;

        //public void AddExecutionResult(IGraphSequenceContext context)
        //{
        //    var output = context.Output;
        //    _executionResults.Add((context, output.Any()
        //            ? output.Select(o => o.GetMatrix()).ToArray()
        //            : new[] { context.Data.GetMatrix() }
        //        ));
        //}

		//protected override IEnumerable<ExecutionResult> GetResults()
		//{
  //          foreach (var (context, data) in _executionResults) {
		//		uint outputIndex = 0;
		//		foreach (var output in data) {
		//			yield return new ExecutionResult(context.BatchSequence, output.AsIndexable().Rows.Select(r => r.Data).ToArray(), outputIndex);
		//			++outputIndex;
		//		}
		//		context.Dispose();
		//		foreach (var matrix in data)
		//			matrix.Dispose();
		//	}
		//	_executionResults.Clear();
  //      }

        public override ExecutionEngineContext CreateContext(IGraphExecutionContext executionContext, IMiniBatchSequence sequence) => new ExecutionEngineContext(executionContext, sequence);
        public override IGraphEngine GraphEngine => this;

        public IEnumerable<ExecutionResult> Execute(IDataSource dataSource, uint batchSize = 128, Action<float>? batchCompleteCallback = null)
		{
			_lap.PushLayer();
			_dataSource = dataSource;
            var provider = new MiniBatchProvider(dataSource, null);
            using var executionContext = new ExecutionContext(_lap, this);
            // ReSharper disable once AccessToDisposedClosure
            executionContext.Add(provider.GetMiniBatches(batchSize, mb => Execute(executionContext, mb)));
            float operationCount = executionContext.RemainingOperationCount;
            float index = 0f;

            IGraphOperation? operation;
            while ((operation = executionContext.GetNextOperation()) != null) {
                _lap.PushLayer();
                foreach (var result in operation.Execute(executionContext))
                    yield return result;
                //foreach (var (context, data) in _executionResults) {
                //    uint outputIndex = 0;
                //    foreach (var output in data) {
                //        ret.Add(new ExecutionResult(context.BatchSequence, output.AsIndexable().Rows.Select(r => r.Data).ToArray(), outputIndex));
                //        ++outputIndex;
                //    }
                //    context.Dispose();
                //    foreach (var matrix in data)
                //        matrix.Dispose();
                //}
                //_executionResults.Clear();
                _lap.PopLayer();

                if (batchCompleteCallback != null) {
                    var percentage = (++index) / operationCount;
                    batchCompleteCallback(percentage);
                }
            }

            _lap.PopLayer();
			_dataSource = null;
        }

		protected override IEnumerable<ExecutionResult> Execute(IGraphExecutionContext executionContext, IMiniBatch batch)
		{
            var table = new Dictionary<IMiniBatchSequence, IGraphSequenceContext>();

			if (batch.IsSequential) {
				IMiniBatchSequence? curr;
				while ((curr = batch.GetNextSequence()) != null) {
					var context = CreateContext(executionContext, curr);
					Start.ExecuteForward(context, 0);
					while (context.HasNext)
						context.ExecuteNext();
                    foreach (var result in context.Results)
                        yield return result;
					table.Add(curr, context);
				}
			} else {
				var context = CreateContext(executionContext, batch.CurrentSequence);
				Start.ExecuteForward(context, 0);

				while (context.HasNext)
					context.ExecuteNext();

                foreach (var result in context.Results)
                    yield return result;

				table.Add(batch.CurrentSequence, context);
			}

            foreach (var result in Continue(batch, executionContext, sequence => table[sequence]))
                yield return result;
        }
	}
}
