using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Action
{
    public class SetMemory : IAction
    {
        readonly int _channel;

        public SetMemory(int channel)
        {
            _channel = channel;
        }

        public void Execute(IMatrix input, int channel, IBatchContext context)
        {
            context.ExecutionContext.SetMemory(_channel, input);
        }
    }
}
