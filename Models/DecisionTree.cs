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
    [ProtoContract]
    public class DecisionTree
    {
        [ProtoContract]
        public class Node
        {
            [ProtoMember(1)]
            public Node[] Children { get; set; }

            [ProtoMember(2)]
            public int ColumnIndex { get; set; }

            [ProtoMember(3)]
            public string MatchLabel { get; set; }

            [ProtoMember(4)]
            public double? Split { get; set; }

            [ProtoMember(5)]
            public string Classification { get; set; }


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

        [ProtoMember(1)]
        public int ClassColumnIndex { get; set; }

        [ProtoMember(2)]
        public Node Root { get; set; }

        public string AsXml
        {
            get
            {
                var sb = new StringBuilder();
                if (Root != null)
                    Root.WriteTo(new XmlTextWriter(new StringWriter(sb)));
                return sb.ToString();
            }
        }
    }
}
