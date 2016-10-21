using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Simple
{
    [ProtoContract]
    public class TrainingExample
    {
        [ProtoMember(1)]
        public float[] Input { get; set; }

        [ProtoMember(2)]
        public float[] Output { get; set; }

        public TrainingExample() { }
        public TrainingExample(float[] input, float[] output)
        {
            Input = input;
            Output = output;
        }
    }
}
