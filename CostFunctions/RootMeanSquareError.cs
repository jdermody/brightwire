using Icbld.BrightWire.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icbld.BrightWire.CostFunction
{
    public class RootMeanSquareError : ICostFunction
    {
        public float Calculate(IIndexableVector output, IIndexableVector expectedOutput)
        {
            return Convert.ToSingle(Math.Sqrt(output.Values.Zip(expectedOutput.Values, (a, e) => BoundMath.Pow(a - e, 2)).Average()));
        }
    }
}
