using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ErrorMetrics
{
    public class OneHot : IErrorMetric
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
            return output.MaximumIndex() == expectedOutput.MaximumIndex() ? 1f : 0f;
        }

        public IMatrix CalculateDelta(IMatrix input, IMatrix expectedOutput)
        {
            return expectedOutput.Subtract(input);
        }
    }
}
