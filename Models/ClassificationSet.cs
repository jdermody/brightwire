using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Models
{
    [ProtoContract]
    public class ClassificationSet
    {
        [ProtoContract]
        public class Classification
        {
            [ProtoMember(1)]
            public string Name { get; set; }

            [ProtoMember(2)]
            public uint[] Data { get; set; }
        }

        [ProtoMember(1)]
        public Classification[] Classifications { get; set; }
    }
}
