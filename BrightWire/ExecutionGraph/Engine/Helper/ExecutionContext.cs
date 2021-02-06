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
        readonly ConcurrentQueue<IGraphOperation> _operationList = new ConcurrentQueue<IGraphOperation>();
        readonly ConcurrentDictionary<string, IFloatMatrix> _memory = new ConcurrentDictionary<string, IFloatMatrix>();
        readonly ConcurrentDictionary<IMiniBatchSequence, System.Action<IGraphContext>> _continuationTable = new ConcurrentDictionary<IMiniBatchSequence, System.Action<IGraphContext>>();

	    public ExecutionContext(ILinearAlgebraProvider lap)
        {
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
        public void Add(IGraphOperation operation) => _operationList.Enqueue(operation);
        public void RegisterContinuation(IMiniBatchSequence sequence, System.Action<IGraphContext> callback) => _continuationTable[sequence] = callback;
        public int RemainingOperationCount => _operationList.Count;
        public bool HasContinuations => _continuationTable.Any();

        public void Continue(IGraphContext context)
        {
            if(_continuationTable.TryRemove(context.BatchSequence, out var callback))
                callback(context);
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
