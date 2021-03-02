using System.Collections.Generic;
using System.Linq;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Engine.Helper;
using BrightWire.Models;
using System;
using BrightData;
using BrightWire.ExecutionGraph.Node;

namespace BrightWire.ExecutionGraph.Engine
{
	/// <summary>
	/// Executes (without training) graphs
	/// </summary>
    internal class ExecutionEngine : IGraphExecutionEngine
	{
        public ExecutionEngine(ILinearAlgebraProvider lap, ExecutionGraphModel graph, NodeBase start)
        {
            LinearAlgebraProvider = lap;
			Graph = graph;
			Start = start;
        }

        public NodeBase Start { get; }
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

        public ExecutionGraphSequenceContext CreateContext(IGraphExecutionContext executionContext, IMiniBatchSequence sequence) => new ExecutionGraphSequenceContext(executionContext, sequence);

        IEnumerable<IGraphSequenceContext> Execute(IGraphExecutionContext executionContext, IMiniBatch batch)
		{
            var table = new Dictionary<IMiniBatchSequence, IGraphSequenceContext>();

            if (batch.IsSequential) {
				IMiniBatchSequence? curr;
				while ((curr = batch.GetNextSequence()) != null) {
					var context = CreateContext(executionContext, curr);
                    Start.Forward(GraphData.Null, context);
                    table.Add(curr, context);
                }
			} else {
				var context = CreateContext(executionContext, batch.CurrentSequence);
                Start.Forward(GraphData.Null, context);
                yield return context;
            }

            if (executionContext.HasContinuations) {
                while (executionContext.HasContinuations) {
                    var additionalContext = new List<(IGraphSequenceContext Context, Action<IGraphSequenceContext[]> OnEnd)>();
                    foreach (var item in executionContext.ExecuteAdditionalMiniBatch(null))
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
                        var context = table[currentSequence];
                        executionContext.Continue(context);
                        yield return context;
                    }
                }
            }
            else {
                foreach (var item in table)
                    yield return item.Value;
            }
        }

        public IGraphExecutionContext CreateExecutionContext() => new ExecutionContext(LinearAlgebraProvider, this);

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
                foreach (var context in operation.Execute()) {
                    foreach (var result in context.Results)
                        yield return result;
                    context.Dispose();
                }
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
                context = operation.Execute().Single();
                LinearAlgebraProvider.PopLayer();
            }
            var ret = context?.Results.SingleOrDefault();
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
                foreach (var context in operation.Execute()) {
                    yield return context.Results.Single();
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
                context = operation.Execute().Single();
            }

            var ret = context?.Results.SingleOrDefault();
            context?.Dispose();
            LinearAlgebraProvider.PopLayer();
            DataSource = null;
            return ret;
        }

        IGraphSequenceContext ICreateGraphContext.Create(IGraphExecutionContext executionContext, IMiniBatchSequence sequence, ILearningContext? learningContext) => CreateContext(executionContext, sequence);
    }
}
