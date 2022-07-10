using System;
using BrightData;
using BrightData.Helper;
using BrightData.LinearAlgebra;

namespace BrightWire.ExecutionGraph.ErrorMetric
{
    /// <summary>
    /// Binary classification rounds outputs to 1 or 0 and compares them against the target
    /// </summary>
    internal class BinaryClassification : IErrorMetric
    {
        public IMatrix CalculateGradient(IGraphContext context, IMatrix output, IMatrix targetOutput)
        {
            return targetOutput.Subtract(output);
        }

        public float Compute(IVector output, IVector targetOutput)
        {
            float ret = 0;
            for (var i = 0; i < output.Size; i++) {
                var val = (output.Segment[i] >= 0.5) ? 1.0f : 0.0f;
                ret += (Math.Abs(val - targetOutput.Segment[i]) < FloatMath.AlmostZero) ? 1.0f : 0.0f;
            }
            return ret / output.Size;
        }

        public float Compute(float[] output, float[] targetOutput)
        {
            float ret = 0;
            for (var i = 0; i < output.Length; i++) {
                var val = (output[i] >= 0.5) ? 1.0f : 0.0f;
                ret += (Math.Abs(val - targetOutput[i]) < FloatMath.AlmostZero) ? 1.0f : 0.0f;
            }
            return ret / output.Length;
        }

        public bool DisplayAsPercentage => true;
    }
}
