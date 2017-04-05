using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Execution
{
    class Engine : IExecutionEngine
    {
        protected readonly IReadOnlyList<IGraphInput> _input;

        public Engine(IReadOnlyList<IGraphInput> input)
        {
            _input = input;
        }

        public IReadOnlyList<IIndexableVector> Execute(int batchSize = 128)
        {
            var ret = new List<IIndexableVector>();
            foreach (var input in _input)
                ret.AddRange(input.Execute(batchSize));
            return ret;
        }
    }
}
