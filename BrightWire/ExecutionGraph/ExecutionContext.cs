using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph
{
    class ExecutionContext : IExecutionContext
    {
        readonly List<IGraphOperation> _operationList = new List<IGraphOperation>();
        readonly Dictionary<int, IMatrix> _memory = new Dictionary<int, IMatrix>();
        readonly ILinearAlgebraProvider _lap;

        public ExecutionContext(ILinearAlgebraProvider lap)
        {
            _lap = lap;
        }

        public ILinearAlgebraProvider LinearAlgebraProvider => _lap;
        public void Add(IReadOnlyList<IGraphOperation> operations) => _operationList.AddRange(operations);
        public void Add(IGraphOperation operation) => _operationList.Add(operation);

        public IMatrix GetMemory(int index)
        {
            return _memory[index];
        }

        public IGraphOperation GetNextOperation()
        {
            for (int i = 0, len = _operationList.Count; i < len; i++) {
                var operation = _operationList[i];
                if (operation.CanContinue) {
                    _operationList.RemoveAt(i);
                    return operation;
                }
            }
            return null;
        }

        public void SetMemory(int index, IMatrix memory)
        {
            _memory[index] = memory;
        }
    }
}
