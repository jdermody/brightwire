using System;
using BrightData;
using BrightWire.LinearAlgebra.Helper;
using BrightWire.Models;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace BrightWire.ExecutionGraph.ErrorMetric
{
    /// <summary>
    /// Binary classification rounds outputs to 1 or 0 and compares them against the target
    /// </summary>
    class BinaryClassification : IErrorMetric
    {
        public IMatrix CalculateGradient(IContext context, IMatrix output, IMatrix targetOutput)
        {
            return targetOutput.Subtract(output);
        }

        public float Compute(Vector<float> output, Vector<float> targetOutput)
        {
            float ret = 0;
            for (var i = 0; i < output.Size; i++) {
                var val = (output.Data[i] >= 0.5) ? 1.0f : 0.0f;
                ret += (Math.Abs(val - targetOutput.Data[i]) < BoundMath.ZERO_LIKE) ? 1.0f : 0.0f;
            }
            return ret / output.Size;
        }

        public bool DisplayAsPercentage => true;
    }
}
