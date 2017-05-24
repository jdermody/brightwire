using BrightWire.TreeBased;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BrightWire.Models
{
    /// <summary>
    /// A decision tree model
    /// </summary>
    [ProtoContract]
    public class DecisionTree
    {
        /// <summary>
        /// A node in the decision tree
        /// </summary>
        [ProtoContract]
        public class Node
        {
            /// <summary>
            /// The nodes children
            /// </summary>
            [ProtoMember(1)]
            public Node[] Children { get; set; }

            /// <summary>
            /// The column index that is being split on
            /// </summary>
            [ProtoMember(2)]
            public int ColumnIndex { get; set; }

            /// <summary>
            /// The value to match this node
            /// </summary>
            [ProtoMember(3)]
            public string MatchLabel { get; set; }

            /// <summary>
            /// The value to split on
            /// </summary>
            [ProtoMember(4)]
            public double? Split { get; set; }

            /// <summary>
            /// This node's classification label
            /// </summary>
            [ProtoMember(5)]
            public string Classification { get; set; }

            /// <summary>
            /// Writes the node as XML
            /// </summary>
            /// <param name="writer">The XML writer</param>
            public void WriteTo(XmlWriter writer)
            {
                writer.WriteStartElement("node");
                if (MatchLabel != null)
                    writer.WriteAttributeString("matches", MatchLabel);

                if (Split.HasValue)
                    writer.WriteAttributeString("split", Split.Value.ToString());
                if(ColumnIndex >= 0)
                    writer.WriteAttributeString("column", ColumnIndex.ToString());
                if (Classification != null)
                    writer.WriteAttributeString("classification", Classification);

                if (Children != null) {
                    foreach (var child in Children)
                        child.WriteTo(writer);
                }
                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// The classification label column index
        /// </summary>
        [ProtoMember(1)]
        public int ClassColumnIndex { get; set; }

        /// <summary>
        /// The root of the tree
        /// </summary>
        [ProtoMember(2)]
        public Node Root { get; set; }

        /// <summary>
        /// Converts the tree to XML
        /// </summary>
        public string AsXml
        {
            get
            {
                var sb = new StringBuilder();
                if (Root != null)
                    Root.WriteTo(XmlWriter.Create(new StringWriter(sb)));
                return sb.ToString();
            }
        }

        /// <summary>
        /// Creates a classifier from the model
        /// </summary>
        /// <returns></returns>
        public IRowClassifier CreateClassifier()
        {
            return new DecisionTreeClassifier(this);
        }
    }
}
