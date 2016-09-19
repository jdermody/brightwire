using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Models
{
    [ProtoContract]
    public class StringTable
    {
        [ProtoMember(1)]
        public string[] Data { get; set; }
    }
}
