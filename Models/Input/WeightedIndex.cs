using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Input
{
    [ProtoContract]
    public class WeightedIndex
    {
        [ProtoMember(1)]
        public uint Index { get; set; }

        [ProtoMember(2)]
        public float Weight { get; set; }
    }
}
