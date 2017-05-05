using BrightWire.LinearAlgebra.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.ErrorMetric
{
    class CrossEntropy : IErrorMetric
    {
        public IMatrix CalculateGradient(IMatrix output, IMatrix targetOutput)
        {
            return targetOutput.Subtract(output);
        }

        public float Compute(IIndexableVector output, IIndexableVector targetOutput)
        {
            float ret = 0;
            var len = output.Count;
            for (var i = 0; i < len; i++) {
                var a = output[i];
                var y = targetOutput[i];
                ret += BoundMath.Constrain(-y * BoundMath.Log(a) - (1.0f - y) * BoundMath.Log(1.0f - a));
            }
            return ret / len;
        }

        public bool DisplayAsPercentage { get { return false; } }
    }
}
