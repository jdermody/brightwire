using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Output
{
    public class WeightedClassification
    {
        public string Classification { get; private set; }
        public float Weight { get; private set; }

        internal WeightedClassification(string classification, float weight)
        {
            Classification = classification;
            Weight = weight;
        }

        /// <summary>
        /// Creates a new weighted classification
        /// </summary>
        public static WeightedClassification Create(string classification, float weight)
        {
            return new WeightedClassification(classification, weight);
        }
    }
}
