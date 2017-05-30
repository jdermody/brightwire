using BrightWire.ExecutionGraph.Helper;
using BrightWire.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Engine
{
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
            IMatrix output;
            if (_memory.TryGetValue(index, out output))
                return output;
            return null;
        }

        public IGraphOperation GetNextOperation()
        {
            IGraphOperation ret;
            if (_operationList.TryDequeue(out ret))
                return ret;
            return null;
        }

        public void SetMemory(string index, IMatrix memory)
        {
            if (memory == null) {
                IMatrix temp;
                if (_memory.TryRemove(index, out temp))
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
            FloatTensor ret;
            if (_inputTransformationCache.TryGetValue(id, out ret))
                return _lap.CreateTensor(ret);
            return null;
        }
    }
}
