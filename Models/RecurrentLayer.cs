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
    public class RecurrentLayer
    {
        [ProtoMember(1)]
        public RecurrentLayerType Type { get; set; }

        [ProtoMember(2)]
        public ActivationType Activation { get; set; }

        [ProtoMember(3)]
        public NetworkLayer[] Layer { get; set; }

        public void WriteTo(string name, XmlWriter writer)
        {
            writer.WriteStartElement(name);
            writer.WriteAttributeString("type", Type.ToString());
            writer.WriteAttributeString("activation", Activation.ToString());
            if (Layer != null) {
                foreach (var item in Layer)
                    item.WriteTo(writer);
            }
            writer.WriteEndElement();
        }
    }
}
