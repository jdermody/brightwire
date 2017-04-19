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
            context.LearningContext?.Log(writer => {
                writer.WriteStartElement("write-memory");
                writer.WriteAttributeString("channel", _channel.ToString());
                context.LearningContext.Log(null, input);
                writer.WriteEndElement();
            });

            context.ExecutionContext.SetMemory(_channel, input);
        }
    }
}
