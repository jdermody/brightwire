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
    internal class ExecutionEngine : IGraphExecutionEngine
	{
        public ExecutionEngine(ILinearAlgebraProvider lap, ExecutionGraphModel graph, INode start)
        {
            LinearAlgebraProvider = lap;
			Graph = graph;
			Start = start;
        }

        public INode Start { get; }
        public ExecutionGraphModel Graph { get; }
        public IDataSource? DataSource { get; private set; } = null;
		public ILinearAlgebraProvider LinearAlgebraProvider { get; }

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

        public ExecutionEngineContext CreateContext(IGraphExecutionContext executionContext, IMiniBatchSequence sequence) => new ExecutionEngineContext(executionContext, sequence);

        IEnumerable<IGraphSequenceContext> Execute(IGraphExecutionContext executionContext, IMiniBatch batch)
		{
            var table = new Dictionary<IMiniBatchSequence, IGraphSequenceContext>();

			if (batch.IsSequential) {
				IMiniBatchSequence? curr;
				while ((curr = batch.GetNextSequence()) != null) {
					var context = CreateContext(executionContext, curr);
					Start.ExecuteForward(context, 0);
					while (context.HasNext)
						context.ExecuteNext();
                    yield return context;
					table.Add(curr, context);
				}
			} else {
				var context = CreateContext(executionContext, batch.CurrentSequence);
				Start.ExecuteForward(context, 0);

				while (context.HasNext)
					context.ExecuteNext();

                yield return context;
                table.Add(batch.CurrentSequence, context);
			}

            foreach (var result in Continue(batch, executionContext, sequence => table[sequence]))
                yield return result;
        }

        public IGraphExecutionContext CreateExecutionContext() => new ExecutionContext(LinearAlgebraProvider, this);

        protected IEnumerable<IGraphSequenceContext> Continue(IMiniBatch batch, IGraphExecutionContext executionContext, Func<IMiniBatchSequence, IGraphSequenceContext> lookupContext)
        {
            while (executionContext.HasContinuations) {
                var additionalContext = new List<(IGraphSequenceContext Context, Action<IGraphSequenceContext[]> OnEnd)>();
                foreach (var item in executionContext.ExecuteAdditional())
                    additionalContext.Add(item);

                // after all have executed...
                if (additionalContext.Any()) {
                    var groups = additionalContext.GroupBy(d => d.OnEnd);
                    foreach (var group in groups)
                        group.Key(group.Select(d => d.Context).ToArray());

                    foreach (var item in additionalContext)
                        yield return item.Context;
                }

                batch.Reset();
	            IMiniBatchSequence? currentSequence;
	            while ((currentSequence = batch.GetNextSequence()) != null) {
                    var context = lookupContext(currentSequence);
                    executionContext.Continue(context);
                    while (context.HasNext)
                        context.ExecuteNext();
                    yield return context;
                }
            }
        }

        public IEnumerable<ExecutionResult> Execute(IDataSource dataSource, uint batchSize = 128, Action<float>? batchCompleteCallback = null)
        {
            LinearAlgebraProvider.PushLayer();
            DataSource = dataSource;
            var provider = new MiniBatchProvider(dataSource, null);
            using var executionContext = new ExecutionContext(LinearAlgebraProvider, this);
            // ReSharper disable once AccessToDisposedClosure
            executionContext.Add(provider.GetMiniBatches(batchSize, mb => Execute(executionContext, mb)));
            float operationCount = executionContext.RemainingOperationCount;
            float index = 0f;

            IGraphOperation? operation;
            while ((operation = executionContext.GetNextOperation()) != null) {
                LinearAlgebraProvider.PushLayer();
                foreach (var context in operation.Execute(executionContext)) {
                    yield return context.Result;
                    context.Dispose();
                }

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
                LinearAlgebraProvider.PopLayer();

                if (batchCompleteCallback != null) {
                    var percentage = (++index) / operationCount;
                    batchCompleteCallback(percentage);
                }
            }

            LinearAlgebraProvider.PopLayer();
        }

        public ExecutionResult? Execute(float[] input)
        {
            LinearAlgebraProvider.PushLayer();
            DataSource = new SingleRowDataSource(input, LinearAlgebraProvider, false, MiniBatchSequenceType.Standard, 0);
            var provider = new MiniBatchProvider(DataSource, null);
            using var executionContext = new ExecutionContext(LinearAlgebraProvider, this);
            // ReSharper disable once AccessToDisposedClosure
            executionContext.Add(provider.GetMiniBatches(1, mb => Execute(executionContext, mb)));

            IGraphOperation? operation;
            IGraphSequenceContext? context = null;
            // TODO: check that there is a single operation?
            while ((operation = executionContext.GetNextOperation()) != null) {
                LinearAlgebraProvider.PushLayer();
                context = operation.Execute(executionContext).Single();
                LinearAlgebraProvider.PopLayer();
            }
            var ret = context?.Result;
            context?.Dispose();
            LinearAlgebraProvider.PopLayer();
            DataSource = null;
            return ret;
        }

        //protected ExecutionResult _Execute(float[] input)
        //{
        //    _lap.PushLayer();
        //    ExecutionResult ret = null;
        //    _dataSource = new SingleRowDataSource(input, false, MiniBatchSequenceType.Standard, 0);
        //    var provider = new MiniBatchProvider(_dataSource, _lap.Context.Random);
        //    using (var executionContext = new ExecutionContext(_lap)) {
        //        executionContext.Add(provider.GetMiniBatches(1, mb => _Execute(executionContext, mb)));

        //        IGraphOperation operation;
        //        while ((operation = executionContext.GetNextOperation()) != null) {
        //            operation.Execute(executionContext);
        //            _ClearContextList();
        //        }

        //        ret = _GetResults().Single();
        //    }
        //    _lap.PopLayer();
        //    _dataSource = null;
        //    return ret;
        //}

        public IEnumerable<ExecutionResult> ExecuteSequential(float[][] input)
        {
            LinearAlgebraProvider.PushLayer();
            DataSource = new SequentialRowDataSource(input, LinearAlgebraProvider);
            var provider = new MiniBatchProvider(DataSource, null);
            using var executionContext = new ExecutionContext(LinearAlgebraProvider, this);
            // ReSharper disable once AccessToDisposedClosure
            executionContext.Add(provider.GetMiniBatches(1, mb => Execute(executionContext, mb)));

            IGraphOperation? operation;
            while ((operation = executionContext.GetNextOperation()) != null) {
                LinearAlgebraProvider.PushLayer();
                foreach (var context in operation.Execute(executionContext)) {
                    yield return context.Result;
                    context.Dispose();
                }

                LinearAlgebraProvider.PopLayer();
            }

            LinearAlgebraProvider.PopLayer();
            DataSource = null;
        }

        public ExecutionResult? ExecuteSingleSequentialStep(IGraphExecutionContext executionContext, uint sequenceIndex, float[] input, MiniBatchSequenceType sequenceType)
        {
            LinearAlgebraProvider.PushLayer();
            DataSource = new SingleRowDataSource(input, LinearAlgebraProvider, true, sequenceType, sequenceIndex);
            var provider = new MiniBatchProvider(DataSource, LinearAlgebraProvider.Context.Random);
            // ReSharper disable once AccessToDisposedClosure
            executionContext.Add(provider.GetMiniBatches(1, mb => Execute(executionContext, mb)));

            IGraphOperation? operation;
            IGraphSequenceContext? context = null;
            // TODO: check that there is a single operation?
            while ((operation = executionContext.GetNextOperation()) != null) {
                context = operation.Execute(executionContext).Single();
            }

            var ret = context?.Result;
            context?.Dispose();
            LinearAlgebraProvider.PopLayer();
            DataSource = null;
            return ret;
        }

        IGraphSequenceContext ICreateGraphContext.Create(IGraphExecutionContext executionContext, IMiniBatchSequence sequence) => CreateContext(executionContext, sequence);
    }
}
