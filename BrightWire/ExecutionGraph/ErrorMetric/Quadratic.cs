using BrightWire.LinearAlgebra.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.ErrorMetric
{
    class Quadratic : IErrorMetric
    {
        public IMatrix CalculateDelta(IMatrix output, IMatrix targetOutput)
        {
            return targetOutput.Subtract(output);
        }

        public float Compute(IIndexableVector output, IIndexableVector targetOutput)
        {
            using (var diff = output.Subtract(targetOutput))
                return 0.5f * BoundMath.Pow(diff.L2Norm(), 2);
        }

        public bool DisplayAsPercentage { get { return false; } }
    }
}
