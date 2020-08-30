using BrightData;
using BrightWire.LinearAlgebra.Helper;
using BrightWire.Models;
using System;
using System.Linq;

namespace BrightWire.ExecutionGraph.ErrorMetric
{
    /// <summary>
    /// Quadratic error
    /// https://en.wikipedia.org/wiki/Mean_squared_error#Loss_function
    /// </summary>
    class Quadratic : IErrorMetric
    {
        public IMatrix CalculateGradient(IContext context, IMatrix output, IMatrix targetOutput)
        {
            return targetOutput.Subtract(output);
        }

        public float Compute(Vector<float> output, Vector<float> targetOutput)
        {
            var diff = output.Data.Values.Zip(targetOutput.Data.Values, (x1, x2) => Math.Pow(x1 - x2, 2)).Sum();
            return BoundMath.Constrain(Convert.ToSingle(0.5 * diff));
        }

        public bool DisplayAsPercentage => false;
    }
}
