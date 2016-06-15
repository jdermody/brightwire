using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ErrorMetrics
{
    public class RMSE : IErrorMetric
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
            return Convert.ToSingle(Math.Sqrt(output.Values.Zip(expectedOutput.Values, (d1, d2) => Math.Pow(d1 - d2, 2)).Average()));
        }
    }
}
