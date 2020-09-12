using System;
using BrightData;
using BrightData.Helper;

namespace BrightWire.ExecutionGraph.ErrorMetric
{
    /// <summary>
    /// Binary classification rounds outputs to 1 or 0 and compares them against the target
    /// </summary>
    class BinaryClassification : IErrorMetric
    {
        public IFloatMatrix CalculateGradient(IContext context, IFloatMatrix output, IFloatMatrix targetOutput)
        {
            return targetOutput.Subtract(output);
        }

        public float Compute(Vector<float> output, Vector<float> targetOutput)
        {
            float ret = 0;
            for (var i = 0; i < output.Size; i++) {
                var val = (output.Segment[i] >= 0.5) ? 1.0f : 0.0f;
                ret += (Math.Abs(val - targetOutput.Segment[i]) < FloatMath.AlmostZero) ? 1.0f : 0.0f;
            }
            return ret / output.Size;
        }

        public bool DisplayAsPercentage => true;
    }
}
