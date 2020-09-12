using BrightData;
using BrightWire.Models;
using System;
using System.Linq;
using BrightData.Helper;

namespace BrightWire.ExecutionGraph.ErrorMetric
{
    /// <summary>
    /// Quadratic error
    /// https://en.wikipedia.org/wiki/Mean_squared_error#Loss_function
    /// </summary>
    class Quadratic : IErrorMetric
    {
        public IFloatMatrix CalculateGradient(IContext context, IFloatMatrix output, IFloatMatrix targetOutput)
        {
            return targetOutput.Subtract(output);
        }

        public float Compute(Vector<float> output, Vector<float> targetOutput)
        {
            var diff = output.Segment.Values.Zip(targetOutput.Segment.Values, (x1, x2) => Math.Pow(x1 - x2, 2)).Sum();
            return FloatMath.Constrain(Convert.ToSingle(0.5 * diff));
        }

        public bool DisplayAsPercentage => false;
    }
}
