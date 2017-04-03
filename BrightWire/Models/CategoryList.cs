using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace BrightWire.Models
{
    /// <summary>
    /// A sparse array of indices
    /// </summary>
    [ProtoContract]
    public class CategoryList
    {
        /// <summary>
        /// The list of indices
        /// </summary>
        [ProtoMember(1)]
        public uint[] CategoryIndex { get; set; }

        /// <summary>
        /// The number of items in the list
        /// </summary>
        public int CategoryCount { get { return CategoryIndex?.Length ?? 0; } }

        /// <summary>
        /// ToString override
        /// </summary>
        public override string ToString()
        {
            return $"{CategoryCount} categories";
        }

        /// <summary>
        /// Writes the data to an XML writer
        /// </summary>
        /// <param name="name">The name to give the data</param>
        /// <param name="writer">The writer to write to</param>
        public void WriteTo(string name, XmlWriter writer)
        {
            writer.WriteStartElement(name ?? "category-list");

            if (CategoryIndex != null)
                writer.WriteValue(String.Join("|", CategoryIndex.OrderBy(d => d).Select(c => c.ToString())));
            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the data to a binary writer
        /// </summary>
        /// <param name="writer"></param>
        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(CategoryCount);
            if (CategoryIndex != null) {
                foreach (var item in CategoryIndex)
                    writer.Write(item);
            }
        }

        /// <summary>
        /// Creates a category list from a binary reader
        /// </summary>
        /// <param name="reader">The binary reader</param>
        public static CategoryList ReadFrom(BinaryReader reader)
        {
            var len = reader.ReadInt32();
            var ret = new uint[len];

            for (var i = 0; i < len; i++)
                ret[i] = reader.ReadUInt32();

            return new CategoryList {
                CategoryIndex = ret
            };
        }
    }
}
