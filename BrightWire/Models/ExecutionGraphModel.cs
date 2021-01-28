﻿using System.IO;
using BrightData;
using BrightWire.Helper;

namespace BrightWire.Models
{
    /// <summary>
    /// A serialised execution graph
    /// </summary>
    public class ExecutionGraphModel : ISerializable
    {
        /// <summary>
        /// A node within the graph
        /// </summary>
        public class Node : ISerializable
        {
            public Node()
            {

            }

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
            public string TypeName { get; set; }

            /// <summary>
            /// The unique id within the graph
            /// </summary>
            public string Id { get; set; }

            /// <summary>
            /// Node friendly name
            /// </summary>
            public string? Name { get; set; }

            /// <summary>
            /// A short description of the node
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// The node's parameters
            /// </summary>
            public byte[]? Data { get; set; }

            /// <inheritdoc />
            public void WriteTo(BinaryWriter writer) => ModelSerialisation.WriteTo(this, writer);

            /// <inheritdoc />
            public void Initialize(IBrightDataContext context, BinaryReader reader) => ModelSerialisation.ReadFrom(context, reader, this);
        }

        /// <summary>
        /// Wires connect nodes (aka edges)
        /// </summary>
        public class Wire : ISerializable
        {
            /// <summary>
            /// The source node id
            /// </summary>
            public string FromId { get; set; }

            /// <summary>
            /// The target node id
            /// </summary>
            public string ToId { get; set; }

            /// <summary>
            /// The channel on the target node to send the source node's output
            /// </summary>
            public uint InputChannel { get; set; }

            /// <inheritdoc />
	        public override string ToString()
            {
                return $"{FromId} -> {ToId} on channel {InputChannel}";
            }

            /// <inheritdoc />
            public void Initialize(IBrightDataContext context, BinaryReader reader) => ModelSerialisation.ReadFrom(context, reader, this);

            /// <inheritdoc />
            public void WriteTo(BinaryWriter writer) => ModelSerialisation.WriteTo(this, writer);

            /// <inheritdoc />
            public override int GetHashCode()
            {
                return (FromId + ToId + InputChannel).GetHashCode();
            }

	        /// <inheritdoc />
            public override bool Equals(object obj)
            {
                if (obj is Wire other)
                    return FromId == other.FromId && ToId == other.ToId && InputChannel == other.InputChannel;
                return false;
            }
        }

        /// <summary>
        /// Segment contract version number
        /// </summary>
        public string Version { get; set; } = "3.0";

        /// <summary>
        /// The name of the graph
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The primary input node
        /// </summary>
        public Node InputNode { get; set; }

        /// <summary>
        /// Other connected nodes
        /// </summary>
        public Node[] OtherNodes { get; set; }

        /// <summary>
        /// A list of the wires that connect the nodes in the graph
        /// </summary>
        public Wire[] Wires { get; set; }

        /// <inheritdoc />
        public void WriteTo(BinaryWriter writer) => ModelSerialisation.WriteTo(this, writer);

        /// <inheritdoc />
        public void Initialize(IBrightDataContext context, BinaryReader reader) => ModelSerialisation.ReadFrom(context, reader, this);
    }
}
