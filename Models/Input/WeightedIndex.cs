using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Input
{
    /// <summary>
    /// A sparse vector index
    /// </summary>
    [ProtoContract]
    public class WeightedIndex
    {
        /// <summary>
        /// The vector index
        /// </summary>
        [ProtoMember(1)]
        public uint Index { get; set; }

        /// <summary>
        /// The weight at the index
        /// </summary>
        [ProtoMember(2)]
        public float Weight { get; set; }
    }
}
