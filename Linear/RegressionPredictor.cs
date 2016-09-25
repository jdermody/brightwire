using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Linear
{
    public class RegressionPredictor
    {
        readonly IVector _theta;
        readonly ILinearAlgebraProvider _lap;

        public RegressionPredictor(ILinearAlgebraProvider lap, IVector theta)
        {
            _lap = lap;
            _theta = theta;
        }

        public float Predict(IVector theta, params float[] vals)
        {
            var v = _lap.Create(vals.Length + 1, i => i == 0 ? 1 : vals[i - 1]);
            return v.DotProduct(theta);
        }

        public float Predict(IVector theta, IReadOnlyList<float> vals)
        {
            var v = _lap.Create(vals.Count + 1, i => i == 0 ? 1f : vals[i - 1]);
            return v.DotProduct(theta);
        }
    }
}
