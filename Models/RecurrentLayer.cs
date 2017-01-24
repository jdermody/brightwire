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
    /// A recurrent neural network layer
    /// </summary>
    [ProtoContract]
    public class RecurrentLayer
    {
        /// <summary>
        /// The layer type
        /// </summary>
        [ProtoMember(1)]
        public RecurrentLayerType Type { get; set; }

        /// <summary>
        /// The activation type
        /// </summary>
        [ProtoMember(2)]
        public ActivationType Activation { get; set; }

        /// <summary>
        /// The network layer data
        /// </summary>
        [ProtoMember(3)]
        public NetworkLayer[] Layer { get; set; }

        /// <summary>
        /// Writes the layer to the XML writer
        /// </summary>
        /// <param name="name">The name to give this layer</param>
        /// <param name="writer">The writer to write to</param>
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
