using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using BrightData;
using BrightWire.Helper;

namespace BrightWire.Models
{
    /// <summary>
    /// A serialised execution graph
    /// </summary>
    public class ExecutionGraphModel : IAmSerializable
    {
        /// <summary>
        /// A node within the graph
        /// </summary>
        public class Node : IAmSerializable
        {
            /// <summary>
            /// Default constructor
            /// </summary>
            public Node()
            {

            }

            /// <summary>
            /// Initialize from binary reader
            /// </summary>
            /// <param name="reader"></param>
            public Node(BinaryReader reader)
            {
                TypeName = reader.ReadString();
                Id = reader.ReadString();
                Name = reader.ReadString();
                Description = reader.ReadString();
                var len = reader.ReadInt32();
                if (len > 0)
                    Data = reader.ReadBytes(len);
            }

            /// <summary>
            /// The .NET type name of the node type
            /// </summary>
            public string TypeName { get; set; } = "";

            /// <summary>
            /// The unique id within the graph
            /// </summary>
            public string Id { get; set; } = "";

            /// <summary>
            /// Node friendly name
            /// </summary>
            public string? Name { get; set; }

            /// <summary>
            /// A short description of the node
            /// </summary>
            public string? Description { get; set; }

            /// <summary>
            /// The node's parameters
            /// </summary>
            public byte[]? Data { get; set; }

            /// <inheritdoc />
            public void WriteTo(BinaryWriter writer) => ModelSerialisation.WriteTo(this, writer);

            /// <inheritdoc />
            public void Initialize(BrightDataContext context, BinaryReader reader) => ModelSerialisation.ReadFrom(context, reader, this);
        }

        /// <summary>
        /// Wires connect nodes (aka edges)
        /// </summary>
        public class Wire : IAmSerializable
        {
            /// <summary>
            /// The source node id
            /// </summary>
            public string FromId { get; internal set; } = "";

            /// <summary>
            /// The target node id
            /// </summary>
            public string ToId { get; internal set; } = "";

            /// <summary>
            /// The channel on the target node to send the source node's output
            /// </summary>
            public uint InputChannel { get; internal set; }

            /// <inheritdoc />
	        public override string ToString()
            {
                return $"{FromId} -> {ToId} on channel {InputChannel}";
            }

            /// <inheritdoc />
            public void Initialize(BrightDataContext context, BinaryReader reader) => ModelSerialisation.ReadFrom(context, reader, this);

            /// <inheritdoc />
            public void WriteTo(BinaryWriter writer) => ModelSerialisation.WriteTo(this, writer);

            /// <inheritdoc />
            public override int GetHashCode()
            {
                return (FromId + ToId + InputChannel).GetHashCode();
            }

	        /// <inheritdoc />
            public override bool Equals(object? obj)
            {
                if (obj is Wire other)
                    return FromId == other.FromId && ToId == other.ToId && InputChannel == other.InputChannel;
                return false;
            }
        }

        /// <summary>
        /// Segment contract version number
        /// </summary>
        public string Version { get; set; } = "4.0";

        /// <summary>
        /// The name of the graph
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// The primary input node
        /// </summary>
        public Node InputNode { get; set; } = new();

        /// <summary>
        /// Other connected nodes
        /// </summary>
        public Node[] OtherNodes { get; set; } = [];

        /// <summary>
        /// A list of the wires that connect the nodes in the graph
        /// </summary>
        public Wire[] Wires { get; set; } = [];

        /// <inheritdoc />
        public void WriteTo(BinaryWriter writer) => ModelSerialisation.WriteTo(this, writer);

        /// <inheritdoc />
        public void Initialize(BrightDataContext context, BinaryReader reader) => ModelSerialisation.ReadFrom(context, reader, this);

        /// <summary>
        /// Visits each node in the graph
        /// </summary>
        /// <param name="param"></param>
        /// <param name="onEnter"></param>
        /// <param name="onLeave"></param>
        /// <typeparam name="T"></typeparam>
        public void VisitNodes<T>(T param, Action<T, Node, uint> onEnter, Action<T, Node> onLeave)
        {
            var nodes = OtherNodes.ToDictionary(n => n.Id);
            var wires = Wires.ToLookup(w => w.FromId);
            var visited = new HashSet<Node>();
            Visit(param, InputNode, 0,visited, nodes, wires, onEnter, onLeave);
        }

        void Visit<T>(T param, Node node, uint channel, HashSet<Node> visited, Dictionary<string, Node> nodes, ILookup<string, Wire> wires, Action<T, Node, uint> onEnter, Action<T, Node> onLeave)
        {
            if (visited.Add(node)) {
                onEnter(param, node, channel);
                foreach (var next in wires[node.Id].Select(w => (Node: nodes[w.ToId], Channel: w.InputChannel))) {
                    Visit(param, next.Node, next.Channel, visited, nodes, wires, onEnter, onLeave);
                }

                onLeave(param, node);
            }
        }

        /// <summary>
        /// XML representation of the graph
        /// </summary>
        public string AsXml
        {
            get
            {
                var sb = new StringBuilder();
                using (var writer = XmlWriter.Create(sb)) {
                    writer.WriteStartElement("graph");
                    VisitNodes(writer, (w, n, c) => {
                        w.WriteStartElement("node");
                        w.WriteAttributeString("channel", c.ToString());
                        w.WriteAttributeString("type", n.TypeName);
                        if(!String.IsNullOrWhiteSpace(n.Name))
                            w.WriteAttributeString("name", n.Name);
                        if(!String.IsNullOrWhiteSpace(n.Description))
                            w.WriteAttributeString("description", n.Description);
                    }, (w, _) => w.WriteEndElement());
                    writer.WriteEndElement();
                }

                return sb.ToString();
            }
        }
    }
}
