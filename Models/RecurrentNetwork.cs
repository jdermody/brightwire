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
    public class RecurrentNetwork
    {
        [ProtoMember(1)]
        public RecurrentLayer[] Layer { get; set; }

        [ProtoMember(2)]
        public FloatArray Memory { get; set; }

        public void WriteTo(XmlWriter writer)
        {
            writer.WriteStartElement("recurrent-network");
            if (Layer != null) {
                foreach (var item in Layer)
                    item.WriteTo("layer", writer);
            }
            if (Memory != null)
                Memory.WriteTo("memory", writer);
            writer.WriteEndElement();
        }
    }
}
