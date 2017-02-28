using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Input
{
    [ProtoContract]
    public class SequenceClassificationTrainingExample
    {
        /// <summary>
        /// Sequence of training examples
        /// </summary>
        [ProtoMember(1)]
        public VectorSequence Input { get; set; }

        /// <summary>
        /// The expected output
        /// </summary>
        [ProtoMember(2)]
        public float[] Output { get; set; }
    }
}
