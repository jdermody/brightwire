using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Input
{
    /// <summary>
    /// A list of indexes with a classification label
    /// </summary>
    [ProtoContract]
    public class IndexedClassification
    {
        /// <summary>
        /// The classification label
        /// </summary>
        [ProtoMember(1)]
        public string Name { get; set; }

        /// <summary>
        /// The array of indexes
        /// </summary>
        [ProtoMember(2)]
        public uint[] Data { get; set; }
    }
}
