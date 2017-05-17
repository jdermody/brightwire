using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Models
{
    [ProtoContract]
    public class DataSourceModel
    {
        [ProtoMember(1)]
        public string Version { get; set; } = "2.0";

        [ProtoMember(2)]
        public string Name { get; set; }

        [ProtoMember(3)]
        public int InputSize { get; set; }

        [ProtoMember(4)]
        public int OutputSize { get; set; }

        [ProtoMember(5)]
        public ExecutionGraph Graph { get; set; }
    }
}
