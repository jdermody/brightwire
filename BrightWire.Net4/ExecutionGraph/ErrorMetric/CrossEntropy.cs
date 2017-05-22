using BrightWire.LinearAlgebra.Helper;
using BrightWire.Models;
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

        public float Compute(FloatVector output, FloatVector targetOutput)
        {
            float ret = 0;
            var len = output.Size;
            for (var i = 0; i < len; i++) {
                var a = output.Data[i];
                var y = targetOutput.Data[i];
                ret += BoundMath.Constrain(-y * BoundMath.Log(a) - (1.0f - y) * BoundMath.Log(1.0f - a));
            }
            return ret / len;
        }

        public bool DisplayAsPercentage { get { return false; } }
    }
}
