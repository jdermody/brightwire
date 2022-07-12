using BrightData;
using System;
using System.Linq;
using BrightData.Helper;
using BrightData.LinearAlgebra;

namespace BrightWire.ExecutionGraph.ErrorMetric
{
    /// <summary>
    /// Quadratic error
    /// https://en.wikipedia.org/wiki/Mean_squared_error#Loss_function
    /// </summary>
    internal class Quadratic : IErrorMetric
    {
        public IMatrix CalculateGradient(IGraphContext context, IMatrix output, IMatrix targetOutput)
        {
            return targetOutput.Subtract(output);
        }

        public float Compute(IVectorInfo output, IVectorInfo targetOutput)
        {
            var diff = output.Segment.Values.Zip(targetOutput.Segment.Values, (x1, x2) => Math.Pow(x1 - x2, 2)).Sum();
            return FloatMath.Constrain(Convert.ToSingle(0.5 * diff));
        }

        public float Compute(float[] output, float[] targetOutput)
        {
            var diff = output.Zip(targetOutput, (x1, x2) => Math.Pow(x1 - x2, 2)).Sum();
            return FloatMath.Constrain(Convert.ToSingle(0.5 * diff));
        }

        public bool DisplayAsPercentage => false;
    }
}
