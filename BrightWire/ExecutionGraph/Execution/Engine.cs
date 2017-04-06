using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Execution
{
    class Engine : IExecutionEngine
    {
        readonly IGraphInput _input;
        readonly IMiniBatchProvider _provider;

        public Engine(IMiniBatchProvider provider, IGraphInput input)
        {
            _input = input;
            _provider = provider;
        }

        public IGraphInput Input => _input;

        public IReadOnlyList<IIndexableVector> Execute(int batchSize = 128)
        {
            return _input.Execute(_provider, batchSize);
        }
    }
}
