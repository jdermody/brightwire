using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Engine
{
    class ExecutionContext : IExecutionContext
    {
        readonly bool _isChild = false;
        readonly List<IGraphOperation> _operationList = new List<IGraphOperation>();
        readonly Dictionary<string, IMatrix> _memory = new Dictionary<string, IMatrix>();
        readonly ConcurrentDictionary<int, IMatrix> _inputTransformationCache;
        readonly ILinearAlgebraProvider _lap;
        IGraphData _data;

        public ExecutionContext(ILinearAlgebraProvider lap, IExecutionContext parent = null)
        {
            _lap = lap;
            if(parent != null) {
                var p = (ExecutionContext)parent;
                _inputTransformationCache = p._inputTransformationCache;
                _isChild = true;
            } else
                _inputTransformationCache = new ConcurrentDictionary<int, IMatrix>();
        }

        public void Dispose()
        {
            _memory.Clear();
            if (!_isChild)
                _inputTransformationCache.Clear();
        }

        public ILinearAlgebraProvider LinearAlgebraProvider => _lap;
        public IGraphData Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public void Add(IReadOnlyList<IGraphOperation> operations) => _operationList.AddRange(operations);
        public void Add(IGraphOperation operation) => _operationList.Add(operation);
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
            for (int i = 0, len = _operationList.Count; i < len; i++) {
                var operation = _operationList[i];
                _operationList.RemoveAt(i);
                return operation;
            }
            return null;
        }

        public void SetMemory(string index, IMatrix memory)
        {
            if (memory == null) {
                if(_memory.ContainsKey(index))
                    _memory.Remove(index);
            }
            else
                _memory[index] = memory;
        }

        public void SetInputTransformation(int id, IMatrix matrix)
        {
            _inputTransformationCache[id] = matrix;
        }

        public IMatrix GetInputTransfomation(int id)
        {
            IMatrix ret;
            if (_inputTransformationCache.TryGetValue(id, out ret))
                return ret;
            return null;
        }
    }
}
