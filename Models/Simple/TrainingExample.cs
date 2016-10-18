using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Simple
{
    public class TrainingExample
    {
        public float[] Input { get; private set; }
        public float[] Output { get; private set; }

        public TrainingExample(float[] input, float[] output)
        {
            Input = input;
            Output = output;
        }
    }
}
