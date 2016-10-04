using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ErrorMetrics
{
    public class BinaryClassification : IErrorMetric
    {
        public bool DisplayAsPercentage
        {
            get
            {
                return true;
            }
        }

        public bool HigherIsBetter
        {
            get
            {
                return true;
            }
        }

        public float Compute(IIndexableVector output, IIndexableVector expectedOutput)
        {
            float ret = 0;
            for (var i = 0; i < output.Count; i++) {
                var val = (output[i] >= 0.5) ? 1.0f : 0.0f;
                ret += (val == expectedOutput[i]) ? 1.0f : 0.0f;
            }
            return ret / output.Count;
        }

        public IMatrix CalculateDelta(IMatrix input, IMatrix expectedOutput)
        {
            return expectedOutput.Subtract(input);
        }
    }
}
