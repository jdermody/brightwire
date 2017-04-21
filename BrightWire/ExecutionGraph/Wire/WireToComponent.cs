using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BrightWire.ExecutionGraph.Wire
{
    class WireToComponent : WireBase
    {
        readonly IComponent _component;

        public WireToComponent(int inputSize, int outputSize, int inputChannel, IComponent component, IWire destination) 
            : base(inputSize, outputSize, inputChannel, destination)
        {
            _component = component;
        }

        public override void Send(IMatrix signal, int channel, IBatchContext context)
        {
            // notify about the input
            (signal, channel) = _OnInput(signal, channel, context);

            Debug.Assert(channel == _channel);
            Debug.Assert(signal.ColumnCount == _inputSize);

            IMatrix output = null;
            if (context.IsTraining)
                output = _component.Train(signal, channel, context);
            else
                output = _component.Execute(signal, channel, context);

            // notify about the output
            (output, channel) = _OnOutput(output, channel, context);

            if (_destination != null)
                _destination.Send(output, channel, context);
        }
    }
}
