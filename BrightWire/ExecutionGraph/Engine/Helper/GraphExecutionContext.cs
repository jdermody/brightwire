using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightData.LinearAlgebra;

namespace BrightWire.ExecutionGraph.Engine.Helper
{
    /// <summary>
    /// Graph engine execution context
    /// </summary>
    public class GraphExecutionContext : IHaveLinearAlgebraProvider, IDisposable
    {
        class AdditionalBatch
        {
            public AdditionalBatch(IMiniBatch batch, IGraphData data, Action<IGraphContext, IGraphData> start, Action<IGraphContext[]> end)
            {
                Batch = batch;
                Data = data;
                Start = start;
                End = end;
            }

            public IMiniBatch Batch { get; } 
            public IGraphData Data { get; } 
            public Action<IGraphContext, IGraphData> Start { get; }  
            public Action<IGraphContext[]> End { get; }  
        }
        readonly ICreateGraphContext _createGraphContext;
        readonly ConcurrentQueue<IGraphOperation> _operationList = new();
        readonly ConcurrentDictionary<string, IMatrix> _memory = new();
        readonly ConcurrentDictionary<IMiniBatchSequence, Action<IGraphContext>> _continuationTable = new();
        readonly ConcurrentStack<AdditionalBatch> _additionalBatches = new();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphEngine">Graph engine</param>
        /// <param name="wantInputInExecutionResults">True to save the graph input in the execution results</param>
	    public GraphExecutionContext(IGraphEngine graphEngine, bool wantInputInExecutionResults = false)
        {
            WantInputInExecutionResults = wantInputInExecutionResults;
            _createGraphContext = graphEngine;
            LinearAlgebraProvider = graphEngine.LinearAlgebraProvider;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (var item in _memory)
                item.Value.Dispose();
            _memory.Clear();
        }

        /// <inheritdoc />
        public LinearAlgebraProvider LinearAlgebraProvider { get; }

        /// <summary>
        /// Adds graph operations to the queue
        /// </summary>
        /// <param name="operations"></param>
        public void Add(IEnumerable<IGraphOperation> operations)
        {
            foreach (var item in operations)
                _operationList.Enqueue(item);
        }

        /// <summary>
        /// Registers a continuation that will be executed after the current sequence has been processed in full
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="callback"></param>
        public void RegisterContinuation(IMiniBatchSequence sequence, Action<IGraphContext> callback) => _continuationTable[sequence] = callback;

        /// <summary>
        /// Registers an additional mini batch to execute after the current mini batch has completed
        /// </summary>
        /// <param name="miniBatch">Mini batch to execute</param>
        /// <param name="data">Initial data</param>
        /// <param name="start">Callback when starting the batch</param>
        /// <param name="end">Callback when ending the batch</param>
        public void RegisterAdditionalMiniBatch(IMiniBatch miniBatch, IGraphData data, Action<IGraphContext, IGraphData> start, Action<IGraphContext[]> end) => _additionalBatches.Push(new(miniBatch, data, start, end));

        /// <summary>
        /// Count of remaining operations in queue
        /// </summary>
        public int RemainingOperationCount => _operationList.Count;

        /// <summary>
        /// True if there are registered continuations or additional mini batches that still need to execute
        /// </summary>
        public bool HasContinuations => _continuationTable.Any() || _additionalBatches.Any();

        /// <summary>
        /// True if the input to the graph will be stored in the execution results
        /// </summary>
        public bool WantInputInExecutionResults { get; }

        /// <summary>
        /// Checks if execution should continue from a remaining continuation
        /// </summary>
        /// <param name="context"></param>
        public void Continue(IGraphContext context)
        {
            if(_continuationTable.TryRemove(context.BatchSequence, out var callback))
                callback(context);
        }

        /// <summary>
        /// Executes any additional mini batches
        /// </summary>
        /// <param name="learningContext"></param>
        /// <returns></returns>
        public IEnumerable<(IGraphContext Context, Action<IGraphContext[]> Callback)> ExecuteAdditionalMiniBatch(ILearningContext? learningContext)
        {
            while (_additionalBatches.TryPop(out var item)) {
                IMiniBatchSequence? prev = null;

                while (item.Batch.GetNextSequence() is { } sequence) {
                    var context = _createGraphContext.Create(this, sequence, learningContext);
                    IGraphData? state;

                    // otherwise take the hidden state of the last encoder
                    if (prev?.GraphContext != null) {
                        var previousState = prev.GraphContext.GetData("hidden-forward");
                        state = previousState.Last().Data;
                    }

                    // otherwise take the context
                    else
                        state = item.Data;

                    item.Start(context, state);
                    yield return (context, item.End);
                    prev = sequence;
                }
            }
        }

        /// <summary>
        /// Checks if a named memory slot is in use
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool HasMemory(string index) => _memory.ContainsKey(index);

        /// <summary>
        /// Returns a named memory slot
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public IMatrix GetMemory(string index)
        {
            if (_memory.TryGetValue(index, out var output))
                return output;
            throw new Exception($"Memory not found: {index}");
        }

        /// <summary>
        /// Pops the next pending graph operation (if any)
        /// </summary>
        /// <returns></returns>
        public IGraphOperation? GetNextOperation()
        {
            if (_operationList.TryDequeue(out var ret))
                return ret;
            return null;
        }

        /// <summary>
        /// Sets a named memory slot
        /// </summary>
        /// <param name="index"></param>
        /// <param name="memory"></param>
        public void SetMemory(string index, IMatrix? memory)
        {
            if (memory == null) {
                if (_memory.TryRemove(index, out var temp))
                    temp.Dispose();
            } else {
                _memory[index] = memory;
            }
        }
    }
}
