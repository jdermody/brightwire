using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models
{
    [ProtoContract]
    public class StringTable
    {
        [ProtoMember(1)]
        public string[] Data { get; set; }
    }
}
