using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightWire.ExecutionGraph.ErrorMetric
{
    class Rmse : IErrorMetric
    {
        public IMatrix CalculateDelta(IMatrix output, IMatrix targetOutput)
        {
            return targetOutput.Subtract(output);
        }

        public float Compute(IIndexableVector output, IIndexableVector targetOutput)
        {
            return Convert.ToSingle(Math.Sqrt(output.Values.Zip(targetOutput.Values, (d1, d2) => Math.Pow(d1 - d2, 2)).Average()));
        }

        public bool DisplayAsPercentage { get { return false; } }
    }
}
