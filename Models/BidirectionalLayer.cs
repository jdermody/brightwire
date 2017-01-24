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
    /// A layer of bidirectional neural network
    /// </summary>
    [ProtoContract]
    public class BidirectionalLayer
    {
        /// <summary>
        /// The forward layer data
        /// </summary>
        [ProtoMember(1)]
        public RecurrentLayer Forward { get; set; }

        /// <summary>
        /// The backward layer data
        /// </summary>
        [ProtoMember(2)]
        public RecurrentLayer Backward { get; set; }

        /// <summary>
        /// Writes the network to XML
        /// </summary>
        /// <param name="writer">The XML writer</param>
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
