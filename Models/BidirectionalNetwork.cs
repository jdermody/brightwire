using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BrightWire.Models
{
    /// <summary>
    /// A layer in a bidirectional neural network
    /// </summary>
    [ProtoContract]
    public class BidirectionalNetwork
    {
        /// <summary>
        /// Layer padding
        /// </summary>
        [ProtoMember(1)]
        public int Padding { get; set; }

        /// <summary>
        /// The layer data
        /// </summary>
        [ProtoMember(2)]
        public BidirectionalLayer[] Layer { get; set; }

        /// <summary>
        /// The initial forward memory
        /// </summary>
        [ProtoMember(3)]
        public FloatArray ForwardMemory { get; set; }

        /// <summary>
        /// The initial backward memory
        /// </summary>
        [ProtoMember(4)]
        public FloatArray BackwardMemory { get; set; }

        /// <summary>
        /// Writes the layer as XML
        /// </summary>
        /// <param name="writer">The XML writer</param>
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
