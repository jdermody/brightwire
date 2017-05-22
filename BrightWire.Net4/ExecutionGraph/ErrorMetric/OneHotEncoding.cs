using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.ErrorMetric
{
    class OneHotEncoding : IErrorMetric
    {
        public IMatrix CalculateGradient(IMatrix output, IMatrix targetOutput)
        {
            return targetOutput.Subtract(output);
        }

        public float Compute(FloatVector output, FloatVector expectedOutput)
        {
            return output.MaximumIndex() == expectedOutput?.MaximumIndex() ? 1f : 0f;
        }

        public bool DisplayAsPercentage { get { return true; } }
    }
}
