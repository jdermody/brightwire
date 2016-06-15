using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BrightWire.Models
{
    [ProtoContract]
    public class BidirectionalLayer
    {
        [ProtoMember(1)]
        public RecurrentLayer Forward { get; set; }

        [ProtoMember(2)]
        public RecurrentLayer Backward { get; set; }

        public void WriteTo(XmlWriter writer)
        {
            writer.WriteStartElement("bidirectional-layer");
            if (Forward != null)
                Forward.WriteTo("forward", writer);
            if (Backward != null)
                Backward.WriteTo("backward", writer);
            writer.WriteEndElement();
        }
    }
}
