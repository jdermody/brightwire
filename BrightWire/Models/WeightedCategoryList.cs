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
    /// A sparse array of weighted indices
    /// </summary>
    [ProtoContract]
    public class WeightedCategoryList
    {
        /// <summary>
        /// A weighted category
        /// </summary>
        [ProtoContract]
        public class Category
        {
            /// <summary>
            /// Category index
            /// </summary>
            [ProtoMember(1)]
            public uint CategoryIndex { get; set; }

            /// <summary>
            /// Category weight
            /// </summary>
            [ProtoMember(2)]
            public float Weight { get; set; }
        }

        /// <summary>
        /// The list of categories
        /// </summary>
        [ProtoMember(1)]
        public Category[] CategoryList { get; set; }

        /// <summary>
        /// The number of items in the list
        /// </summary>
        public int CategoryCount { get { return CategoryList?.Length ?? 0; } }

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

            if (CategoryList != null) {
                writer.WriteValue(String.Join("|", CategoryList
                    .OrderBy(d => d.CategoryIndex)
                    .Select(c => $"{c.CategoryIndex}:{c.Weight}")
                ));
            }
            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the data to a binary writer
        /// </summary>
        /// <param name="writer"></param>
        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(CategoryCount);
            if (CategoryList != null) {
                foreach (var item in CategoryList) {
                    writer.Write(item.CategoryIndex);
                    writer.Write(item.Weight);
                }
            }
        }

        /// <summary>
        /// Creates a weighted category list from a binary reader
        /// </summary>
        /// <param name="reader">The binary reader</param>
        public static WeightedCategoryList ReadFrom(BinaryReader reader)
        {
            var len = reader.ReadInt32();
            var ret = new Category[len];

            for (var i = 0; i < len; i++) {
                var category = new Category();
                category.CategoryIndex = reader.ReadUInt32();
                category.Weight = reader.ReadSingle();
                ret[i] = category;
            }

            return new WeightedCategoryList {
                CategoryList = ret
            };
        }
    }
}
