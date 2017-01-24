using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Output
{
    /// <summary>
    /// A classified vector
    /// </summary>
    public class VectorClassification
    {
        /// <summary>
        /// The vector that was classified
        /// </summary>
        public float[] Data { get; private set; }

        /// <summary>
        /// The associated classification
        /// </summary>
        public string Classification { get; private set; }

        internal VectorClassification(float[] data, string classification)
        {
            Data = data;
            Classification = classification;
        }
    }
}
