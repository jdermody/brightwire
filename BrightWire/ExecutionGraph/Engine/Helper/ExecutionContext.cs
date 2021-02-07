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
        readonly ConcurrentQueue<IGraphOperation> _operationList = new ConcurrentQueue<IGraphOperation>();
        readonly ConcurrentDictionary<string, IFloatMatrix> _memory = new ConcurrentDictionary<string, IFloatMatrix>();
        readonly ConcurrentDictionary<IMiniBatchSequence, Action<IGraphSequenceContext>> _continuationTable = new ConcurrentDictionary<IMiniBatchSequence, Action<IGraphSequenceContext>>();
        readonly ConcurrentStack<(IMiniBatch Batch, IGraphData Data, Action<IGraphSequenceContext, IGraphData> Start, Action<IGraphSequenceContext[]> End)> _additionalBatches = new ConcurrentStack<(IMiniBatch, IGraphData, Action<IGraphSequenceContext, IGraphData>, Action<IGraphSequenceContext[]>)>();

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
        public void RegisterAdditional(IMiniBatch miniBatch, IGraphData data, Action<IGraphSequenceContext, IGraphData> start, Action<IGraphSequenceContext[]> end) => _additionalBatches.Push((miniBatch, data, start, end));

        public int RemainingOperationCount => _operationList.Count;
        public bool HasContinuations => _continuationTable.Any() || _additionalBatches.Any();

        public void Continue(IGraphSequenceContext context)
        {
            if(_continuationTable.TryRemove(context.BatchSequence, out var callback))
                callback(context);
        }

        public IEnumerable<(IGraphSequenceContext Context, Action<IGraphSequenceContext[]> Callback)> ExecuteAdditional()
        {
            while (_additionalBatches.TryPop(out var item)) {
                IMiniBatchSequence? sequence;
                while ((sequence = item.Batch.GetNextSequence()) != null) {
                    var context = _createGraphContext.Create(this, sequence);
                    item.Start(context, item.Data);
                    while (context.HasNext)
                        context.ExecuteNext();
                    yield return (context, item.End);
                }
            }
        }

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
