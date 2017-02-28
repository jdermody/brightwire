using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Input
{
    /// <summary>
    /// A sequence of training examples
    /// </summary>
    [ProtoContract]
    public class TrainingSequence
    {
        /// <summary>
        /// The array of examples
        /// </summary>
        [ProtoMember(1)]
        public TrainingExample[] Sequence { get; set; }
    }
}
