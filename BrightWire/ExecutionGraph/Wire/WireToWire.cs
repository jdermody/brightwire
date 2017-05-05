using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BrightWire.ExecutionGraph.Wire
{
    class WireToWire : WireBase
    {
        public WireToWire(int inputSize, int outputSize, int inputChannel, IWire destination) 
            : base(inputSize, outputSize, inputChannel, destination)
        {
        }

        public override void Send(IMatrix signal, int channel, IBatchContext context)
        {
            // notify about the input
            (signal, channel) = _OnInput(signal, channel, context);

            Debug.Assert(channel == _channel);
            Debug.Assert(signal.ColumnCount == _inputSize);

            // notify about the output
            (signal, channel) = _OnOutput(signal, channel, context);

            if (_destination != null)
                _destination.Send(signal, channel, context);
        }
    }
}
