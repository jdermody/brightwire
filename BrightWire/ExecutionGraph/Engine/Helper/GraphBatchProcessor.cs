using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using BrightData;
using BrightData.LinearAlgebra;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node;
using BrightWire.Models;

namespace BrightWire.ExecutionGraph.Engine.Helper
{
    /// <summary>
    /// Shared batch-processing logic for graph engines.
    /// Provides the common outer batch loop, inner sequence processing, and continuation handling.
    /// Designed for composition — engines delegate to an instance of this class.
    /// </summary>
    internal static class GraphBatchProcessor
    {
        /// <summary>
        /// Delegates for engine-specific behavior.
        /// </summary>
        internal sealed class Hooks
        {
            internal Hooks(
                Func<GraphExecutionContext, MiniBatch.Sequence, ILearningContext?, IGraphContext> createContext,
                Action<NodeBase, MiniBatch.Sequence, IGraphContext> processForward)
            {
                CreateContext = createContext;
                ProcessForward = processForward;
            }

            public Func<GraphExecutionContext, MiniBatch.Sequence, ILearningContext?, IGraphContext> CreateContext { get; }
            public Action<NodeBase, MiniBatch.Sequence, IGraphContext> ProcessForward { get; }
            public Action<GraphExecutionContext>? OnBatchStart { get; internal set; }
            public Action<GraphExecutionContext>? OnLoopEnd { get; internal set; }
        }

        /// <summary>
        /// Executes the standard batch-processing loop: iterate operations, process each mini-batch,
        /// yield results, manage linear algebra scopes, and invoke progress callbacks.
        /// </summary>
        internal static async IAsyncEnumerable<ExecutionResult> ExecuteBatchLoop(
            LinearAlgebraProvider<float> lap,
            GraphExecutionContext executionContext,
            Hooks hooks,
            NodeBase start,
            Action<float>? batchCompleteCallback,
            [EnumeratorCancellation] CancellationToken ct)
        {
            lap.PushScope();
            float operationCount = executionContext.RemainingOperationCount;
            var index = 0f;

            while (executionContext.GetNextOperation() is { } operation && !ct.IsCancellationRequested)
            {
                lap.PushScope();
                await foreach (var context in ProcessOperation(executionContext, operation, hooks, start, ct))
                {
                    foreach (var result in context.Results)
                        yield return result;
                    context.Dispose();
                }
                lap.PopScope();

                if (batchCompleteCallback != null)
                {
                    var percentage = (++index) / operationCount;
                    batchCompleteCallback(percentage);
                }
            }

            lap.PopScope();
        }

        /// <summary>
        /// Processes a single graph operation (mini-batch): handles sequential vs non-sequential
        /// sequences, forward propagation, and continuation execution.
        /// </summary>
        internal static async IAsyncEnumerable<IGraphContext> ProcessOperation(
            GraphExecutionContext executionContext,
            IGraphOperation operation,
            Hooks hooks,
            NodeBase start,
            [EnumeratorCancellation] CancellationToken ct)
        {
            await foreach (var ctx in ProcessOperation(executionContext, operation, hooks, start, null, ct))
                yield return ctx;
        }

        /// <summary>
        /// Like ProcessOperation but passes a learning context for training scenarios.
        /// </summary>
        internal static async IAsyncEnumerable<IGraphContext> ProcessOperation(
            GraphExecutionContext executionContext,
            IGraphOperation operation,
            Hooks hooks,
            NodeBase start,
            ILearningContext? learningContext,
            [EnumeratorCancellation] CancellationToken ct)
        {
            var batch = await operation.GetMiniBatch();
            var table = new Dictionary<MiniBatch.Sequence, IGraphContext>();

            if (batch.IsSequential)
            {
                while (batch.GetNextSequence() is { } curr && !ct.IsCancellationRequested)
                {
                    var context = hooks.CreateContext(executionContext, curr, learningContext);
                    hooks.ProcessForward(start, curr, context);
                    table.Add(curr, context);
                }
            }
            else
            {
                var context = hooks.CreateContext(executionContext, batch.CurrentSequence, learningContext);
                hooks.ProcessForward(start, batch.CurrentSequence, context);
                yield return context;
            }

            if (executionContext.HasContinuations)
            {
                foreach (var c in ProcessContinuations(batch, executionContext, table, hooks, ct))
                    yield return c;
            }
            else
            {
                foreach (var item in table)
                    yield return item.Value;
            }
        }

        /// <summary>
        /// Handles continuation execution: processes additional mini-batches registered during
        /// the forward pass, then continues registered continuation callbacks for each sequence.
        /// </summary>
        internal static IEnumerable<IGraphContext> ProcessContinuations(
            MiniBatch batch,
            GraphExecutionContext executionContext,
            Dictionary<MiniBatch.Sequence, IGraphContext> contextTable,
            Hooks hooks,
            CancellationToken ct)
        {
            while (executionContext.HasContinuations && !ct.IsCancellationRequested)
            {
                var additionalContext = new List<(IGraphContext Context, Action<IGraphContext[]> OnEnd)>();
                foreach (var item in executionContext.ExecuteAdditionalMiniBatch(null))
                    additionalContext.Add(item);

                // after all have executed...
                if (additionalContext.Any())
                {
                    var groups = additionalContext.GroupBy(d => d.OnEnd);
                    foreach (var group in groups)
                        group.Key(group.Select(d => d.Context).ToArray());

                    foreach (var (context, _) in additionalContext)
                        yield return context;
                }

                batch.Reset();
                while (batch.GetNextSequence() is { } currentSequence)
                {
                    var context = contextTable[currentSequence];
                    executionContext.Continue(context);
                    yield return context;
                }
            }
        }
    }
}
