using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Input
{
    /// <summary>
    /// A training example
    /// </summary>
    [ProtoContract]
    public class TrainingExample
    {
        /// <summary>
        /// The input data
        /// </summary>
        [ProtoMember(1)]
        public float[] Input { get; set; }

        /// <summary>
        /// The expected output
        /// </summary>
        [ProtoMember(2)]
        public float[] Output { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public TrainingExample() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        public TrainingExample(float[] input, float[] output)
        {
            Input = input;
            Output = output;
        }
    }
}
