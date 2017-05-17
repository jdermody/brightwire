using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Models
{
    [ProtoContract]
    public class GraphModel
    {
        [ProtoMember(1)]
        public string Version { get; set; } = "2.0";

        [ProtoMember(2)]
        public string Name { get; set; }

        [ProtoMember(3)]
        public ExecutionGraph Graph { get; set; }

        [ProtoMember(4)]
        public DataSourceModel DataSource { get; set; }
    }
}
