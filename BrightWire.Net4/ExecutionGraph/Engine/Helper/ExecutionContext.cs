using BrightWire.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.ExecutionGraph.Engine.Helper
{
    /// <summary>
    /// Graph engine execution context
    /// </summary>
    class ExecutionContext : IExecutionContext
    {
        readonly ConcurrentQueue<IGraphOperation> _operationList = new ConcurrentQueue<IGraphOperation>();
        readonly ConcurrentDictionary<string, IMatrix> _memory = new ConcurrentDictionary<string, IMatrix>();
        readonly ConcurrentDictionary<IMiniBatchSequence, System.Action<IContext>> _continuationTable = new ConcurrentDictionary<IMiniBatchSequence, System.Action<IContext>>();
        readonly ILinearAlgebraProvider _lap;

        public ExecutionContext(ILinearAlgebraProvider lap)
        {
            _lap = lap;
        }

        public void Dispose()
        {
            foreach (var item in _memory)
                item.Value.Dispose();
            _memory.Clear();
        }

        public ILinearAlgebraProvider LinearAlgebraProvider => _lap;

        public void Add(IReadOnlyList<IGraphOperation> operations)
        {
            foreach (var item in operations)
                _operationList.Enqueue(item);
        }
        public void Add(IGraphOperation operation) => _operationList.Enqueue(operation);
        public void RegisterContinuation(IMiniBatchSequence sequence, System.Action<IContext> callback) => _continuationTable[sequence] = callback;
        public int RemainingOperationCount => _operationList.Count;
        public bool HasContinuations => _continuationTable.Any();

        public void Continue(IContext context)
        {
            if(_continuationTable.TryRemove(context.BatchSequence, out System.Action<IContext> callback))
                callback(context);
        }

        public IMatrix GetMemory(string index)
        {
            if (_memory.TryGetValue(index, out IMatrix output))
                return output;
            return null;
        }

        public IGraphOperation GetNextOperation()
        {
            if (_operationList.TryDequeue(out IGraphOperation ret))
                return ret;
            return null;
        }

        public void SetMemory(string index, IMatrix memory)
        {
            if (memory == null) {
                if (_memory.TryRemove(index, out IMatrix temp))
                    temp.Dispose();
            } else {
                _memory[index] = memory;
            }
        }
    }
}
