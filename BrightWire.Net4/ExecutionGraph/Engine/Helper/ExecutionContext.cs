using BrightWire.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace BrightWire.ExecutionGraph.Engine.Helper
{
    /// <summary>
    /// Graph engine execution context
    /// </summary>
    class ExecutionContext : IExecutionContext
    {
        readonly ConcurrentQueue<IGraphOperation> _operationList = new ConcurrentQueue<IGraphOperation>();
        readonly ConcurrentDictionary<string, IMatrix> _memory = new ConcurrentDictionary<string, IMatrix>();
        readonly ConcurrentDictionary<int, FloatTensor> _inputTransformationCache;
        readonly ILinearAlgebraProvider _lap;

        public ExecutionContext(ILinearAlgebraProvider lap)
        {
            _lap = lap;
            _inputTransformationCache = new ConcurrentDictionary<int, FloatTensor>();
        }

        public void Dispose()
        {
            foreach (var item in _memory)
                item.Value.Dispose();
            _memory.Clear();

            _inputTransformationCache.Clear();
        }

        public ILinearAlgebraProvider LinearAlgebraProvider => _lap;

        public void Add(IReadOnlyList<IGraphOperation> operations)
        {
            foreach (var item in operations)
                _operationList.Enqueue(item);
        }
        public void Add(IGraphOperation operation) => _operationList.Enqueue(operation);
        public int RemainingOperationCount => _operationList.Count;

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

        public void SetInputTransformation(int id, I3DTensor tensor)
        {
            _inputTransformationCache[id] = tensor.Data;
        }

        public I3DTensor GetInputTransfomation(int id)
        {
            if (_inputTransformationCache.TryGetValue(id, out FloatTensor ret))
                return _lap.Create3DTensor(ret);
            return null;
        }
    }
}
