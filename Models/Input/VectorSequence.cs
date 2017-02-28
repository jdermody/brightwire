using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace BrightWire.Models.Input
{
    [ProtoContract]
    public class VectorSequence
    {
        [ProtoContract]
        public class Vector
        {
            [ProtoMember(1)]
            public float[] Data { get; set; }
        }

        [ProtoMember(1)]
        public Vector[] Sequence { get; set; }
    }
}
