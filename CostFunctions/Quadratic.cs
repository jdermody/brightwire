using BrightWire.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.CostFunction
{
    public class Quadratic : ICostFunction
    {
        public float Calculate(IIndexableVector output, IIndexableVector expectedOutput)
        {
            using (var diff = output.Subtract(expectedOutput))
                return 0.5f * BoundMath.Pow(diff.L2Norm(), 2);
        }
    }
}
