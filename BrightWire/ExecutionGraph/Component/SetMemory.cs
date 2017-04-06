using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Component
{
    class SetMemory : IComponent
    {
        readonly int _channel;

        public SetMemory(int channel)
        {
            _channel = channel;
        }

        public void Dispose()
        {
            // nop
        }

        public IMatrix Execute(IMatrix input, IBatchContext context)
        {
            context.ExecutionContext.SetMemory(_channel, input);
            return input;
        }

        public IMatrix Train(IMatrix input, int channel, IBatchContext context)
        {
            return Execute(input, context);
        }
    }
}
