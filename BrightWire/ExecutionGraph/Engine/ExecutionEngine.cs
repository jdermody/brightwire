using System.Collections.Generic;
using System.Linq;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Engine.Helper;
using BrightWire.Models;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using BrightData;
using BrightData.LinearAlgebra;
using BrightWire.ExecutionGraph.Node;

namespace BrightWire.ExecutionGraph.Engine
{
	/// <summary>
	/// Executes (without training) graphs
	/// </summary>
    internal class ExecutionEngine : IGraphExecutionEngine
    {
        readonly GraphBatchProcessor.Hooks _hooks;

        public ExecutionEngine(LinearAlgebraProvider<float> lap, ExecutionGraphModel graph, NodeBase start)
        {
            LinearAlgebraProvider = lap;
            Graph = graph;
            Start = start;
            _hooks = new GraphBatchProcessor.Hooks(
                (ctx, seq, _) => new ExecutionGraphSequenceContext(ctx, seq),
                (node, seq, context) => node.Forward(GraphData.Null, context));
        }

        public NodeBase Start { get; }
        public ExecutionGraphModel Graph { get; }
        public IDataSource? DataSource { get; private set; } = null;
		public LinearAlgebraProvider<float> LinearAlgebraProvider { get; }

        public ExecutionGraphSequenceContext CreateContext(GraphExecutionContext executionContext, MiniBatch.Sequence sequence) => new(executionContext, sequence);

        public GraphExecutionContext CreateExecutionContext() => new(this);

        public async IAsyncEnumerable<ExecutionResult> Execute(IDataSource dataSource, uint batchSize = 128, Action<float>? batchCompleteCallback = null, bool wantInputInExecutionResults = false, [EnumeratorCancellation] CancellationToken ct = default)
        {
            LinearAlgebraProvider.PushScope();
            DataSource = dataSource;
            var provider = new MiniBatchProvider(dataSource, null);
            using var executionContext = new GraphExecutionContext(this, wantInputInExecutionResults);
            // ReSharper disable once AccessToDisposedClosure
            executionContext.Add(provider.GetMiniBatches(batchSize));

            await foreach (var result in GraphBatchProcessor.ExecuteBatchLoop(LinearAlgebraProvider, executionContext, _hooks, Start, batchCompleteCallback, ct))
                yield return result;

            LinearAlgebraProvider.PopScope();
        }

        public async IAsyncEnumerable<ExecutionResult> Execute(float[] input, [EnumeratorCancellation] CancellationToken ct = default)
        {
            LinearAlgebraProvider.PushScope();
            DataSource = new SingleRowDataSource(input, LinearAlgebraProvider, false, MiniBatchSequenceType.Standard, 0);
            var provider = new MiniBatchProvider(DataSource, null);
            using var executionContext = new GraphExecutionContext(this);
            executionContext.Add(provider.GetMiniBatches(1));

            while (executionContext.GetNextOperation() is { } operation && !ct.IsCancellationRequested) {
                LinearAlgebraProvider.PushScope();
                using var context = (await ProcessOperation(executionContext, operation, ct).First());
                yield return context.Results.Single();
                LinearAlgebraProvider.PopScope();
            }
            LinearAlgebraProvider.PopScope();
            DataSource = null;
        }

        public async IAsyncEnumerable<ExecutionResult> ExecuteSequential(float[][] input, [EnumeratorCancellation] CancellationToken ct = default)
        {
            LinearAlgebraProvider.PushScope();
            DataSource = new SequentialRowDataSource(input, LinearAlgebraProvider);
            var provider = new MiniBatchProvider(DataSource, null);
            using var executionContext = new GraphExecutionContext(this);
            // ReSharper disable once AccessToDisposedClosure
            executionContext.Add(provider.GetMiniBatches(1));

            while (executionContext.GetNextOperation() is { } operation && !ct.IsCancellationRequested) {
                LinearAlgebraProvider.PushScope();
                await foreach (var context in ProcessOperation(executionContext, operation, ct)) {
                    yield return context.Results.Single();
                    context.Dispose();
                }
                LinearAlgebraProvider.PopScope();
            }

            LinearAlgebraProvider.PopScope();
            DataSource = null;
        }

        public async IAsyncEnumerable<ExecutionResult> ExecuteSingleSequentialStep(GraphExecutionContext executionContext, uint sequenceIndex, float[] input, MiniBatchSequenceType sequenceType, [EnumeratorCancellation] CancellationToken ct = default)
        {
            LinearAlgebraProvider.PushScope();
            DataSource = new SingleRowDataSource(input, LinearAlgebraProvider, true, sequenceType, sequenceIndex);
            var provider = new MiniBatchProvider(DataSource, LinearAlgebraProvider.Context.Random);
            // ReSharper disable once AccessToDisposedClosure
            executionContext.Add(provider.GetMiniBatches(1));

            while (executionContext.GetNextOperation() is { } operation && !ct.IsCancellationRequested) {
                using var context = (await ProcessOperation(executionContext, operation, ct).First());
                yield return context.Results.Single();
            }
            LinearAlgebraProvider.PopScope();
            DataSource = null;
        }

        async IAsyncEnumerable<IGraphContext> ProcessOperation(GraphExecutionContext executionContext, IGraphOperation operation, [EnumeratorCancellation] CancellationToken ct = default)
        {
            await foreach (var context in GraphBatchProcessor.ProcessOperation(executionContext, operation, _hooks, Start, ct))
                yield return context;
        }

        IGraphContext ICreateGraphContext.Create(GraphExecutionContext executionContext, MiniBatch.Sequence sequence, ILearningContext? learningContext) => CreateContext(executionContext, sequence);
    }
}
