using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using BrightData;
using BrightWire.Helper;
using BrightWire.TreeBased;

namespace BrightWire.Models.TreeBased
{
    /// <summary>
    /// A decision tree model
    /// </summary>
    public class DecisionTree : ISerializable
    {
        /// <summary>
        /// A node in the decision tree
        /// </summary>
        public class Node : ISerializable
        {
            /// <summary>
            /// The nodes children
            /// </summary>
            public Node[] Children { get; set; }

            /// <summary>
            /// The column index that is being split on
            /// </summary>
            public uint? ColumnIndex { get; set; }

            /// <summary>
            /// The value to match this node
            /// </summary>
            public string MatchLabel { get; set; }

            /// <summary>
            /// The value to split on
            /// </summary>
            public double? Split { get; set; }

            /// <summary>
            /// This node's classification label
            /// </summary>
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
                    writer.WriteAttributeString("split", Split.Value.ToString(CultureInfo.InvariantCulture));
                if(ColumnIndex.HasValue)
                    writer.WriteAttributeString("column", ColumnIndex.Value.ToString());
                if (Classification != null)
                    writer.WriteAttributeString("classification", Classification);

                if (Children != null) {
                    foreach (var child in Children)
                        child.WriteTo(writer);
                }
                writer.WriteEndElement();
            }

            public void WriteTo(BinaryWriter writer) => ModelSerialisation.WriteTo(this, writer);

            public void Initialize(IBrightDataContext context, BinaryReader reader) => ModelSerialisation.ReadFrom(context, reader, this);
        }

        /// <summary>
        /// The classification label column index
        /// </summary>
        public uint ClassColumnIndex { get; set; }

        /// <summary>
        /// The root of the tree
        /// </summary>
        public Node Root { get; set; }

        /// <summary>
        /// Converts the tree to XML
        /// </summary>
        public string AsXml
        {
            get
            {
                var sb = new StringBuilder();
	            Root?.WriteTo(XmlWriter.Create(new StringWriter(sb)));
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

        public void WriteTo(BinaryWriter writer) => ModelSerialisation.WriteTo(this, writer);

        public void Initialize(IBrightDataContext context, BinaryReader reader) => ModelSerialisation.ReadFrom(context, reader, this);
    }
}
