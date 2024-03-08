using BrightData;
using System;
using System.Linq;
using BrightData.Helper;

namespace BrightWire.ExecutionGraph.ErrorMetric
{
    /// <summary>
    /// Quadratic error
    /// https://en.wikipedia.org/wiki/Mean_squared_error#Loss_function
    /// </summary>
    internal class Quadratic : IErrorMetric
    {
        public IMatrix<float> CalculateGradient(IMatrix<float> output, IMatrix<float> targetOutput)
        {
            return targetOutput.Subtract(output);
        }

        public float Compute(IReadOnlyVector<float> output, IReadOnlyVector<float> targetOutput)
        {
            var diff = output.ReadOnlySegment.Values.Zip(targetOutput.ReadOnlySegment.Values, (x1, x2) => MathF.Pow(x1 - x2, 2)).Sum();
            return Math<float>.Constrain(0.5f * diff);
        }

        public float Compute(float[] output, float[] targetOutput)
        {
            var diff = output.Zip(targetOutput, (x1, x2) => MathF.Pow(x1 - x2, 2)).Sum();
            return Math<float>.Constrain(0.5f * diff);
        }

        public bool DisplayAsPercentage => false;
    }
}
