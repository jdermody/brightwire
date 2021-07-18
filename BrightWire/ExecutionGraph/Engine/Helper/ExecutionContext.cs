using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BrightData;

namespace BrightWire.ExecutionGraph.Engine.Helper
{
    /// <summary>
    /// Graph engine execution context
    /// </summary>
    internal class ExecutionContext : IGraphExecutionContext
    {
        readonly ICreateGraphContext _createGraphContext;
        readonly ConcurrentQueue<IGraphOperation> _operationList = new();
        readonly ConcurrentDictionary<string, IFloatMatrix> _memory = new();
        readonly ConcurrentDictionary<IMiniBatchSequence, Action<IGraphSequenceContext>> _continuationTable = new();
        readonly ConcurrentStack<(IMiniBatch Batch, IGraphData Data, Action<IGraphSequenceContext, IGraphData> Start, Action<IGraphSequenceContext[]> End)> _additionalBatches = new();

	    public ExecutionContext(ILinearAlgebraProvider lap, ICreateGraphContext createGraphContext)
        {
            _createGraphContext = createGraphContext;
            LinearAlgebraProvider = lap;
        }

        public void Dispose()
        {
            foreach (var item in _memory)
                item.Value.Dispose();
            _memory.Clear();
        }

        public ILinearAlgebraProvider LinearAlgebraProvider { get; }

        public void Add(IEnumerable<IGraphOperation> operations)
        {
            foreach (var item in operations)
                _operationList.Enqueue(item);
        }
        public void RegisterContinuation(IMiniBatchSequence sequence, Action<IGraphSequenceContext> callback) => _continuationTable[sequence] = callback;
        public void RegisterAdditionalMiniBatch(IMiniBatch miniBatch, IGraphData data, Action<IGraphSequenceContext, IGraphData> start, Action<IGraphSequenceContext[]> end) => _additionalBatches.Push((miniBatch, data, start, end));

        public int RemainingOperationCount => _operationList.Count;
        public bool HasContinuations => _continuationTable.Any() || _additionalBatches.Any();

        public void Continue(IGraphSequenceContext context)
        {
            if(_continuationTable.TryRemove(context.BatchSequence, out var callback))
                callback(context);
        }

        public IEnumerable<(IGraphSequenceContext Context, Action<IGraphSequenceContext[]> Callback)> ExecuteAdditionalMiniBatch(ILearningContext? learningContext)
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

        public IFloatMatrix GetMemory(string index)
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

        public void SetMemory(string index, IFloatMatrix? memory)
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
