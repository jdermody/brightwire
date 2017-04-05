using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.ErrorMetric
{
    class BinaryClassification : IErrorMetric
    {
        public IMatrix CalculateDelta(IMatrix output, IMatrix targetOutput)
        {
            return targetOutput.Subtract(output);
        }

        public float Compute(IIndexableVector output, IIndexableVector targetOutput)
        {
            float ret = 0;
            for (var i = 0; i < output.Count; i++) {
                var val = (output[i] >= 0.5) ? 1.0f : 0.0f;
                ret += (val == targetOutput[i]) ? 1.0f : 0.0f;
            }
            return ret / output.Count;
        }

        public bool DisplayAsPercentage { get { return true; } }
    }
}
