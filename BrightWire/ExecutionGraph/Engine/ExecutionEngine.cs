using System.Collections.Generic;
using System.Linq;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Engine.Helper;
using BrightWire.Models;
using System;
using BrightData;
using BrightData.LinearAlegbra2;
using BrightWire.ExecutionGraph.Node;

namespace BrightWire.ExecutionGraph.Engine
{
	/// <summary>
	/// Executes (without training) graphs
	/// </summary>
    internal class ExecutionEngine : IGraphExecutionEngine
	{
        readonly BrightDataContext _context;

        public ExecutionEngine(BrightDataContext context, LinearAlgebraProvider lap, ExecutionGraphModel graph, NodeBase start)
        {
            _context = context;
            LinearAlgebraProvider = lap;
			Graph = graph;
			Start = start;
        }

        public NodeBase Start { get; }
        public ExecutionGraphModel Graph { get; }
        public IDataSource? DataSource { get; private set; } = null;
		public LinearAlgebraProvider LinearAlgebraProvider { get; }

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

        public ExecutionGraphSequenceContext CreateContext(GraphExecutionContext executionContext, IMiniBatchSequence sequence) => new(executionContext, sequence);

        IEnumerable<IGraphContext> Execute(GraphExecutionContext executionContext, IGraphOperation operation)
        {
            var batch = operation.GetMiniBatch();
            var table = new Dictionary<IMiniBatchSequence, IGraphContext>();
            var ct = _context.CancellationToken;

            if (batch.IsSequential) {
				IMiniBatchSequence? curr;
				while ((curr = batch.GetNextSequence()) != null && !ct.IsCancellationRequested) {
					var context = CreateContext(executionContext, curr);
                    Start.Forward(ct, GraphData.Null, context);
                    table.Add(curr, context);
                }
			} else {
				var context = CreateContext(executionContext, batch.CurrentSequence);
                Start.Forward(ct, GraphData.Null, context);
                yield return context;
            }

            if (executionContext.HasContinuations) {
                while (executionContext.HasContinuations) {
                    var additionalContext = new List<(IGraphContext Context, Action<IGraphContext[]> OnEnd)>();
                    foreach (var item in executionContext.ExecuteAdditionalMiniBatch(null))
                        additionalContext.Add(item);

                    // after all have executed...
                    if (additionalContext.Any()) {
                        var groups = additionalContext.GroupBy(d => d.OnEnd);
                        foreach (var group in groups)
                            group.Key(group.Select(d => d.Context).ToArray());

                        foreach (var (context, _) in additionalContext)
                            yield return context;
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

        public GraphExecutionContext CreateExecutionContext() => new(_context, LinearAlgebraProvider, this);

        public IEnumerable<ExecutionResult> Execute(IDataSource dataSource, uint batchSize = 128, Action<float>? batchCompleteCallback = null)
        {
            LinearAlgebraProvider.PushScope();
            DataSource = dataSource;
            var provider = new MiniBatchProvider(dataSource, null);
            using var executionContext = new GraphExecutionContext(_context, LinearAlgebraProvider, this);
            // ReSharper disable once AccessToDisposedClosure
            executionContext.Add(provider.GetMiniBatches(batchSize));
            float operationCount = executionContext.RemainingOperationCount;
            var index = 0f;

            IGraphOperation? operation;
            while ((operation = executionContext.GetNextOperation()) != null) {
                LinearAlgebraProvider.PushScope();
                foreach (var context in Execute(executionContext, operation)) {
                    foreach (var result in context.Results)
                        yield return result;
                    context.Dispose();
                }
                LinearAlgebraProvider.PopScope();

                if (batchCompleteCallback != null) {
                    var percentage = (++index) / operationCount;
                    batchCompleteCallback(percentage);
                }
            }

            LinearAlgebraProvider.PopScope();
        }

        public ExecutionResult? Execute(float[] input)
        {
            LinearAlgebraProvider.PushScope();
            DataSource = new SingleRowDataSource(input, LinearAlgebraProvider, false, MiniBatchSequenceType.Standard, 0);
            var provider = new MiniBatchProvider(DataSource, null);
            using var executionContext = new GraphExecutionContext(_context, LinearAlgebraProvider, this);
            // ReSharper disable once AccessToDisposedClosure
            executionContext.Add(provider.GetMiniBatches(1/*, mb => Execute(executionContext, mb)*/));

            IGraphOperation? operation;
            IGraphContext? context = null;
            // TODO: check that there is a single operation?
            while ((operation = executionContext.GetNextOperation()) != null) {
                LinearAlgebraProvider.PushScope();
                context = Execute(executionContext, operation).Single();
                LinearAlgebraProvider.PopScope();
            }
            var ret = context?.Results.SingleOrDefault();
            context?.Dispose();
            LinearAlgebraProvider.PopScope();
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
            LinearAlgebraProvider.PushScope();
            DataSource = new SequentialRowDataSource(input, LinearAlgebraProvider);
            var provider = new MiniBatchProvider(DataSource, null);
            using var executionContext = new GraphExecutionContext(_context, LinearAlgebraProvider, this);
            // ReSharper disable once AccessToDisposedClosure
            executionContext.Add(provider.GetMiniBatches(1));

            IGraphOperation? operation;
            while ((operation = executionContext.GetNextOperation()) != null) {
                LinearAlgebraProvider.PushScope();
                foreach (var context in Execute(executionContext, operation)) {
                    yield return context.Results.Single();
                    context.Dispose();
                }

                LinearAlgebraProvider.PopScope();
            }

            LinearAlgebraProvider.PopScope();
            DataSource = null;
        }

        public ExecutionResult? ExecuteSingleSequentialStep(GraphExecutionContext executionContext, uint sequenceIndex, float[] input, MiniBatchSequenceType sequenceType)
        {
            LinearAlgebraProvider.PushScope();
            DataSource = new SingleRowDataSource(input, LinearAlgebraProvider, true, sequenceType, sequenceIndex);
            var provider = new MiniBatchProvider(DataSource, LinearAlgebraProvider.Context.Random);
            // ReSharper disable once AccessToDisposedClosure
            executionContext.Add(provider.GetMiniBatches(1));

            IGraphOperation? operation;
            IGraphContext? context = null;
            // TODO: check that there is a single operation?
            while ((operation = executionContext.GetNextOperation()) != null) {
                context = Execute(executionContext, operation).Single();
            }

            var ret = context?.Results.SingleOrDefault();
            context?.Dispose();
            LinearAlgebraProvider.PopScope();
            DataSource = null;
            return ret;
        }

        IGraphContext ICreateGraphContext.Create(GraphExecutionContext executionContext, IMiniBatchSequence sequence, ILearningContext? learningContext) => CreateContext(executionContext, sequence);
    }
}
