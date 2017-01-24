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
    /// A recurrent neural network
    /// </summary>
    [ProtoContract]
    public class RecurrentNetwork
    {
        /// <summary>
        /// The layer data
        /// </summary>
        [ProtoMember(1)]
        public RecurrentLayer[] Layer { get; set; }

        /// <summary>
        /// The initial network memory
        /// </summary>
        [ProtoMember(2)]
        public FloatArray Memory { get; set; }

        /// <summary>
        /// Writes the network to XML
        /// </summary>
        /// <param name="writer">The XML writer</param>
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
