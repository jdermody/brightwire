using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BrightData;
using BrightData.LinearAlgebra;

namespace BrightWire.ExecutionGraph.Engine.Helper
{
    /// <summary>
    /// Graph engine execution context
    /// </summary>
    public class GraphExecutionContext : IDisposable
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

            public IMiniBatch Batch { get; set; } 
            public IGraphData Data { get; set; } 
            public Action<IGraphContext, IGraphData> Start { get; set; }  
            public Action<IGraphContext[]> End { get; set; }  
        }
        readonly ICreateGraphContext _createGraphContext;
        readonly ConcurrentQueue<IGraphOperation> _operationList = new();
        readonly ConcurrentDictionary<string, IMatrix> _memory = new();
        readonly ConcurrentDictionary<IMiniBatchSequence, Action<IGraphContext>> _continuationTable = new();
        readonly ConcurrentStack<AdditionalBatch> _additionalBatches = new();

	    public GraphExecutionContext(IGraphEngine graphEngine, bool wantInputInExecutionResults = false)
        {
            WantInputInExecutionResults = wantInputInExecutionResults;
            _createGraphContext = graphEngine;
            LinearAlgebraProvider = graphEngine.LinearAlgebraProvider;
        }

        public void Dispose()
        {
            foreach (var item in _memory)
                item.Value.Dispose();
            _memory.Clear();
        }

        public LinearAlgebraProvider LinearAlgebraProvider { get; }

        public void Add(IEnumerable<IGraphOperation> operations)
        {
            foreach (var item in operations)
                _operationList.Enqueue(item);
        }
        public void RegisterContinuation(IMiniBatchSequence sequence, Action<IGraphContext> callback) => _continuationTable[sequence] = callback;
        public void RegisterAdditionalMiniBatch(IMiniBatch miniBatch, IGraphData data, Action<IGraphContext, IGraphData> start, Action<IGraphContext[]> end) => _additionalBatches.Push(new(miniBatch, data, start, end));

        public int RemainingOperationCount => _operationList.Count;
        public bool HasContinuations => _continuationTable.Any() || _additionalBatches.Any();
        public bool WantInputInExecutionResults { get; }

        public void Continue(IGraphContext context)
        {
            if(_continuationTable.TryRemove(context.BatchSequence, out var callback))
                callback(context);
        }

        public IEnumerable<(IGraphContext Context, Action<IGraphContext[]> Callback)> ExecuteAdditionalMiniBatch(ILearningContext? learningContext)
        {
            while (_additionalBatches.TryPop(out var item)) {
                IMiniBatchSequence? sequence, prev = null;

                while ((sequence = item.Batch.GetNextSequence()) != null) {
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

        public bool HasMemory(string index) => _memory.ContainsKey(index);

        public IMatrix GetMemory(string index)
        {
            if (_memory.TryGetValue(index, out var output))
                return output;
            throw new Exception($"Memory not found: {index}");
        }

        public IGraphOperation? GetNextOperation()
        {
            if (_operationList.TryDequeue(out var ret))
                return ret;
            return null;
        }

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
