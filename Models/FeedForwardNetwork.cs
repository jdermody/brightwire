using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Icbld.BrightWire.Models
{
    [ProtoContract]
    public class FeedForwardNetwork
    {
        [ProtoMember(1)]
        public NetworkLayer[] Layer { get; set; }

        public void WriteTo(XmlWriter writer)
        {
            writer.WriteStartElement("feed-forward-network");
            if(Layer != null) {
                foreach (var item in Layer)
                    item.WriteTo(writer);
            }
            writer.WriteEndElement();
        }
    }
}
