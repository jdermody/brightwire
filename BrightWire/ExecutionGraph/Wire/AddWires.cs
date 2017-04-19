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
        int _destinationChannel;

        public AddWires(int inputSize, int destinationChannel, params IWire[] wires) : base(inputSize, inputSize, null, null)
        {
            foreach (var wire in wires)
                _input.Add(wire.InputChannel.Value, null);
            _destinationChannel = destinationChannel;
        }

        public override void Send(IMatrix signal, int channel, IBatchContext context)
        {
            _input[channel] = signal;
            if(_input.All(kv => kv.Value != null)) {
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

                // reset the list
                foreach(var item in channelList)
                    _input[item] = null;

                if (context.IsTraining) {
                    context.LearningContext.Log(writer => {
                        writer.WriteStartElement("add-wires");
                        writer.WriteAttributeString("channels", String.Join(", ", channelList));
                        if(context.LearningContext.LogMatrixValues)
                            writer.WriteRaw(output.AsIndexable().AsXml);
                    });
                    context.RegisterBackpropagation(new Backpropagation(channelList), _destinationChannel);
                }
                _destination.Send(output, _destinationChannel, context);
                if (context.IsTraining)
                    context.LearningContext.Log(writer => writer.WriteEndElement());
            }
        }
    }
}
