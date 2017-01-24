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
    /// A feed forward neural network
    /// </summary>
    [ProtoContract]
    public class FeedForwardNetwork
    {
        /// <summary>
        /// The network layers
        /// </summary>
        [ProtoMember(1)]
        public NetworkLayer[] Layer { get; set; }

        /// <summary>
        /// Writes the model to XML
        /// </summary>
        /// <param name="writer">The XML writer</param>
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
