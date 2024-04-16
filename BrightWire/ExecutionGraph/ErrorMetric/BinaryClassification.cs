using System;
using BrightData;
using BrightData.Helper;

namespace BrightWire.ExecutionGraph.ErrorMetric
{
    /// <summary>
    /// Binary classification rounds outputs to 1 or 0 and compares them against the target
    /// </summary>
    internal class BinaryClassification : IErrorMetric
    {
        public IMatrix<float> CalculateGradient(IMatrix<float> output, IMatrix<float> targetOutput)
        {
            return targetOutput.Subtract(output);
        }

        public float Compute(IReadOnlyVector<float> output, IReadOnlyVector<float> targetOutput)
        {
            float ret = 0;
            for (var i = 0; i < output.Size; i++) {
                var val = (output.ReadOnlySegment[i] >= 0.5) ? 1.0f : 0.0f;
                ret += (MathF.Abs(val - targetOutput.ReadOnlySegment[i]) < Math<float>.AlmostZero) ? 1.0f : 0.0f;
            }
            return ret / output.Size;
        }

        public float Compute(float[] output, float[] targetOutput)
        {
            float ret = 0;
            for (var i = 0; i < output.Length; i++) {
                var val = (output[i] >= 0.5) ? 1.0f : 0.0f;
                ret += (MathF.Abs(val - targetOutput[i]) < Math<float>.AlmostZero) ? 1.0f : 0.0f;
            }
            return ret / output.Length;
        }

        public bool DisplayAsPercentage => true;
    }
}
