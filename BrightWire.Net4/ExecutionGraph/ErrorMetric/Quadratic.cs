using BrightWire.LinearAlgebra.Helper;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightWire.ExecutionGraph.ErrorMetric
{
    class Quadratic : IErrorMetric
    {
        public IMatrix CalculateGradient(IMatrix output, IMatrix targetOutput)
        {
            return targetOutput.Subtract(output);
        }

        public float Compute(FloatVector output, FloatVector targetOutput)
        {
            var diff = output.Data.Zip(targetOutput.Data, (x1, x2) => Math.Pow(x1 - x2, 2)).Sum();
            return BoundMath.Constrain(Convert.ToSingle(0.5 * diff));
        }

        public bool DisplayAsPercentage { get { return false; } }
    }
}
