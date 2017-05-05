using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightWire.ExecutionGraph.Wire
{
    class AddWires : WireBase
    {
        class Backpropagation : IBackpropagation
        {
            readonly IReadOnlyList<int> _channelList;

            public Backpropagation(IReadOnlyList<int> channelList)
            {
                _channelList = channelList;
            }

            public void Backward(IMatrix errorSignal, int channel, IBatchContext context, bool calculateOutput)
            {
                context.LearningContext.Log(writer => writer.WriteStartElement("add-wires-backpropagation"));
                foreach (var item in _channelList)
                    context.Backpropagate(errorSignal, item);
                context.LearningContext.Log(writer => writer.WriteEndElement());
            }

            public void Dispose()
            {
                // nop
            }
        }
        readonly Dictionary<int, IMatrix> _input = new Dictionary<int, IMatrix>();
        readonly int _destinationChannel;

        public AddWires(int inputSize, int destinationChannel, IWire wire1, IWire wire2) : base(inputSize, inputSize, destinationChannel, null)
        {
            _input.Add(wire1.Channel, null);
            _input.Add(wire2.Channel, null);

            _destinationChannel = destinationChannel;
        }

        public override void Send(IMatrix signal, int channel, IBatchContext context)
        {
            (signal, channel) = _OnInput(signal, channel, context);
            _input[channel] = signal;

            if(_input.All(kv => kv.Value != null)) {
                // add the input matrices together
                IMatrix output = null;
                var channelList = new List<int>();
                foreach(var kv in _input) {
                    var next = kv.Value;
                    channelList.Add(kv.Key);
                    if (output == null)
                        output = next;
                    else {
                        output.AddInPlace(next);
                        next.Dispose();
                    }
                }

                // notify about the output
                (IMatrix finalOutput, int destinationChannel) = _OnOutput(output, _destinationChannel, context);

                // reset the list
                foreach (var item in channelList)
                    _input[item] = null;

                // add the backpropagation step
                if (context.IsTraining) {
                    context.LearningContext.Log(writer => {
                        writer.WriteStartElement("add-wires");
                        writer.WriteAttributeString("channels", String.Join(", ", channelList));
                        if(context.LearningContext.LogMatrixValues)
                            writer.WriteRaw(finalOutput.AsIndexable().AsXml);
                    });
                    context.RegisterBackpropagation(new Backpropagation(channelList), destinationChannel);
                }

                // send the output
                _destination.Send(finalOutput, destinationChannel, context);
                if (context.IsTraining)
                    context.LearningContext.Log(writer => writer.WriteEndElement());
            }
        }
    }
}
