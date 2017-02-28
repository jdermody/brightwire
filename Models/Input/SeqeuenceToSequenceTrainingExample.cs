using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Input
{
    [ProtoContract]
    public class SeqeuenceToSequenceTrainingExample
    {
        [ProtoMember(1)]
        public VectorSequence Input { get; set; }

        [ProtoMember(2)]
        public VectorSequence ExpectedOutput { get; set; }
    }
}
