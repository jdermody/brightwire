using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightWire.ExecutionGraph.Wire
{
    class MultiplyWires : WireBase
    {
        class Backpropagation : IBackpropagation
        {
            readonly IMatrix _input1, _input2;
            readonly int _channel1, _channel2;

            public Backpropagation(int channel1, IMatrix input1, int channel2, IMatrix input2)
            {
                _channel1 = channel1;
                _input1 = input1;
                _channel2 = channel2;
                _input2 = input2;
            }

            public void Backward(IMatrix errorSignal, int channel, IBatchContext context, bool calculateOutput)
            {
                using (var delta1 = errorSignal.Multiply(_input2))
                using (var delta2 = errorSignal.Multiply(_input1)) {
                    context.Backpropagate(delta1, _channel1);
                    context.Backpropagate(delta2, _channel2);
                }
            }

            public void Dispose()
            {
                _input1.Dispose();
                _input2.Dispose();
            }
        }

        readonly int _channel1, _channel2;
        readonly int _destinationChannel;
        readonly IMatrix[] _input = new IMatrix[2];

        public MultiplyWires(int inputSize, int destinationChannel, IWire wire1, IWire wire2) : base(inputSize, inputSize, destinationChannel, null)
        {
            _channel1 = wire1.Channel;
            _channel2 = wire2.Channel;
            _destinationChannel = destinationChannel;
        }

        public override void Send(IMatrix signal, int channel, IBatchContext context)
        {
            (signal, channel) = _OnInput(signal, channel, context);

            if (channel == _channel1)
                _input[0] = signal;
            else if (channel == _channel2)
                _input[1] = signal;

            if(_input.All(v => v != null)) {
                var output = _input[0].PointwiseMultiply(_input[1]);
                (IMatrix finalOutput, int destinationChannel) = _OnOutput(output, _destinationChannel, context);

                if (context.IsTraining)
                    context.RegisterBackpropagation(new Backpropagation(_channel1, _input[0], _channel2, _input[1]), destinationChannel);

                _input[0] = _input[1] = null;

                _destination.Send(finalOutput, destinationChannel, context);
            }
        }
    }
}
