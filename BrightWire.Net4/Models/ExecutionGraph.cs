using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Models
{
    [ProtoContract]
    public class ExecutionGraph
    {
        [ProtoContract]
        public class Node
        {
            [ProtoMember(1)]
            public string TypeName { get; set; }

            [ProtoMember(2)]
            public string Id { get; set; }

            [ProtoMember(3)]
            public string Name { get; set; }

            [ProtoMember(4)]
            public string Description { get; set; }

            [ProtoMember(2)]
            public byte[] Data { get; set; }
        }

        [ProtoContract]
        public class Wire
        {
            [ProtoMember(1)]
            public string FromId { get; set; }

            [ProtoMember(2)]
            public string ToId { get; set; }

            [ProtoMember(3)]
            public int InputChannel { get; set; }
        }
    }
}
