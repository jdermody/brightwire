using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Simple
{
    public class WeightedClassification
    {
        public string Classification { get; private set; }
        public float Weight { get; private set; }

        public WeightedClassification(string classification, float weight)
        {
            Classification = classification;
            Weight = weight;
        }
    }
}
