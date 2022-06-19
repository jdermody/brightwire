using System.Linq;
using BrightData;
using BrightData.LinearAlegbra2;

namespace BrightWire.Linear
{
    /// <summary>
    /// Makes predictions from a previously trained model
    /// </summary>
    internal class RegressionPredictor : ILinearRegressionPredictor
    {
        readonly IVector _theta;
        readonly LinearAlgebraProvider _lap;

        public RegressionPredictor(LinearAlgebraProvider lap, IVector theta)
        {
            _lap = lap;
            _theta = theta;
        }

        public void Dispose()
        {
            _theta.Dispose();
        }

        public float Predict(params float[] input)
        {
            var v = _lap.CreateVector((uint)(input.Length + 1), i => i == 0 ? 1 : input[i - 1]);
            return v.DotProduct(_theta);
        }

        public float[] Predict(float[][] input)
        {
            using var v = _lap.CreateMatrix((uint)input.Length, (uint)(input[0].Length + 1), (i, j) => j == 0 ? 1 : input[(int)i][(int)(j - 1)]);
            using var r = v.Multiply(_theta);
            using var r2 = r.Row(0);
            return r.Segment.Values.ToArray();
        }
    }
}
