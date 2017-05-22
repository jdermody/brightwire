using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightWire.ExecutionGraph.ErrorMetric
{
    class Rmse : IErrorMetric
    {
        public IMatrix CalculateGradient(IMatrix output, IMatrix targetOutput)
        {
            return targetOutput.Subtract(output);
        }

        public float Compute(FloatVector output, FloatVector targetOutput)
        {
            return Convert.ToSingle(Math.Sqrt(output.Data.Zip(targetOutput.Data, (d1, d2) => Math.Pow(d1 - d2, 2)).Average()));
        }

        public bool DisplayAsPercentage { get { return false; } }
    }
}
