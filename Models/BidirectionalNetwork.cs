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
    public class BidirectionalNetwork
    {
        [ProtoMember(1)]
        public int Padding { get; set; }

        [ProtoMember(2)]
        public BidirectionalLayer[] Layer { get; set; }

        [ProtoMember(3)]
        public FloatArray ForwardMemory { get; set; }

        [ProtoMember(4)]
        public FloatArray BackwardMemory { get; set; }

        public void WriteTo(XmlWriter writer)
        {
            writer.WriteStartElement("bidirectional-network");
            writer.WriteAttributeString("padding", Padding.ToString());
            if(Layer != null) {
                foreach (var layer in Layer)
                    layer.WriteTo(writer);
            }
            if (ForwardMemory != null)
                ForwardMemory.WriteTo("forward-memory", writer);
            if (BackwardMemory != null)
                BackwardMemory.WriteTo("backward-memory", writer);
            writer.WriteEndElement();
        }
    }
}
