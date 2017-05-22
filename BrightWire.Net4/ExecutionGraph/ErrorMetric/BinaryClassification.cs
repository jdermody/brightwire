using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.ErrorMetric
{
    class BinaryClassification : IErrorMetric
    {
        public IMatrix CalculateGradient(IMatrix output, IMatrix targetOutput)
        {
            return targetOutput.Subtract(output);
        }

        public float Compute(FloatVector output, FloatVector targetOutput)
        {
            float ret = 0;
            for (var i = 0; i < output.Size; i++) {
                var val = (output.Data[i] >= 0.5) ? 1.0f : 0.0f;
                ret += (val == targetOutput.Data[i]) ? 1.0f : 0.0f;
            }
            return ret / output.Size;
        }

        public bool DisplayAsPercentage { get { return true; } }
    }
}
