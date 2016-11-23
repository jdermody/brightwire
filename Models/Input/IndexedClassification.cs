using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Input
{
    [ProtoContract]
    public class IndexedClassification
    {
        [ProtoMember(1)]
        public string Name { get; set; }

        [ProtoMember(2)]
        public uint[] Data { get; set; }
    }
}
