using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Action
{
    public class SendToWire : IAction
    {
        readonly int? _channel;
        readonly IWire _wire;

        public SendToWire(IWire wire, int? channel = null)
        {
            _wire = wire;
            _channel = channel;
        }

        public void Execute(IMatrix input, int channel, IBatchContext context)
        {
            _wire.Send(input, _channel ?? channel, context);
        }
    }
}
