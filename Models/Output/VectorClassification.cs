using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Output
{
    public class VectorClassification
    {
        public float[] Data { get; private set; }
        public string Classification { get; private set; }

        internal VectorClassification(float[] data, string classification)
        {
            Data = data;
            Classification = classification;
        }
    }
}
