using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Action
{
    public class CopyMemoryToWire : IAction
    {
        readonly int _channel;
        readonly IWire _wire;

        public CopyMemoryToWire(int channel, IWire wire)
        {
            _channel = channel;
            _wire = wire;
        }

        public void Execute(IMatrix input, int channel, IBatchContext context)
        {
            var memory = context.ExecutionContext.GetMemory(_channel);
            _wire.Send(memory, _channel, context);
        }
    }
}
