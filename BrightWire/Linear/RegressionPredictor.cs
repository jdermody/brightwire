using System.Collections.Generic;
using System.Linq;

namespace BrightWire.Linear
{
    /// <summary>
    /// Makes predictions from a previously trained model
    /// </summary>
    class RegressionPredictor : ILinearRegressionPredictor
    {
        readonly IVector _theta;
        readonly ILinearAlgebraProvider _lap;

        public RegressionPredictor(ILinearAlgebraProvider lap, IVector theta)
        {
            _lap = lap;
            _theta = theta;
        }

        public void Dispose()
        {
            _theta.Dispose();
        }

        public float Predict(params float[] vals)
        {
            var v = _lap.CreateVector((uint)(vals.Length + 1), i => i == 0 ? 1 : vals[i - 1]);
            return v.DotProduct(_theta);
        }

        public float Predict(IReadOnlyList<float> vals)
        {
            var v = _lap.CreateVector((uint)(vals.Count + 1), i => i == 0 ? 1f : vals[(int)(i - 1)]);
            return v.DotProduct(_theta);
        }

        public float[] Predict(IReadOnlyList<IReadOnlyList<float>> input)
        {
            using (var v = _lap.CreateMatrix((uint)input.Count, (uint)(input[0].Count + 1), (i, j) => j == 0 ? 1 : input[(int)i][(int)(j - 1)]))
            using (var r = v.Multiply(_theta))
            using(var r2 = r.Row(0)) {
                return r.AsIndexable().Values.ToArray();
            }
        }
    }
}
