using ProtoBuf;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace BrightWire.Models
{
    /// <summary>
    /// A protobuf serialisable sparse array of indices
    /// </summary>
    [ProtoContract]
    public class IndexList
    {
        /// <summary>
        /// The list of indices
        /// </summary>
        [ProtoMember(1)]
        public uint[] Index { get; set; }

        /// <summary>
        /// Create a new index list with the specified indices
        /// </summary>
        /// <param name="index">Sparse list of indices</param>
        public static IndexList Create(uint[] index) => new IndexList { Index = index };

        /// <summary>
        /// The number of items in the list
        /// </summary>
        public int Count { get { return Index?.Length ?? 0; } }

        /// <summary>
        /// ToString override
        /// </summary>
        public override string ToString()
        {
            return $"{Count} indices";
        }

        /// <summary>
        /// Writes the data to an XML writer
        /// </summary>
        /// <param name="name">The name to give the data</param>
        /// <param name="writer">The writer to write to</param>
        public void WriteTo(string name, XmlWriter writer)
        {
            writer.WriteStartElement(name ?? "index-list");

            if (Index != null)
                writer.WriteValue(String.Join("|", Index.OrderBy(d => d).Select(c => c.ToString())));
            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the data to a binary writer
        /// </summary>
        /// <param name="writer"></param>
        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(Count);
            if (Index != null) {
                foreach (var item in Index)
                    writer.Write(item);
            }
        }

        /// <summary>
        /// Creates an index list from a binary reader
        /// </summary>
        /// <param name="reader">The binary reader</param>
        public static IndexList ReadFrom(BinaryReader reader)
        {
            var len = reader.ReadInt32();
            var ret = new uint[len];

            for (var i = 0; i < len; i++)
                ret[i] = reader.ReadUInt32();

            return IndexList.Create(ret);
        }

        /// <summary>
        /// Converts the index list to XML
        /// </summary>
        public string Xml
        {
            get
            {
                var sb = new StringBuilder();
                var settings = new XmlWriterSettings {
                    OmitXmlDeclaration = true
                };
                using (var writer = XmlWriter.Create(sb, settings))
                    WriteTo(null, writer);
                return sb.ToString();
            }
        }
    }
}
