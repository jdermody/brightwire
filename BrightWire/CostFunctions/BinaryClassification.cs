using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightData.Helper;

namespace BrightWire.CostFunctions
{
    public class BinaryClassification
    {
        public float Compute(float output, float target) => Math.Abs((output >= 0.5f ? 1f : 0f) - target) < FloatMath.AlmostZero ? 1.0f : 0.0f;
        public float Compute((float Output, float Target) result) => Compute(result.Output, result.Target);
        public float Compute(IReadOnlyList<(float Output, float Target)> output) => output.Select(Compute).Average();
    }
}
