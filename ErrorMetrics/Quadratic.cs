using BrightWire.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ErrorMetrics
{
    public class Quadratic : IErrorMetric
    {
        public bool DisplayAsPercentage
        {
            get
            {
                return false;
            }
        }

        public bool HigherIsBetter
        {
            get
            {
                return false;
            }
        }

        public float Compute(IIndexableVector output, IIndexableVector expectedOutput)
        {
            using (var diff = output.Subtract(expectedOutput))
                return 0.5f * BoundMath.Pow(diff.L2Norm(), 2);
        }

        public IMatrix CalculateDelta(IMatrix input, IMatrix expectedOutput)
        {
            return expectedOutput.Subtract(input);
        }
    }
}
