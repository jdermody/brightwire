using System.Collections.Generic;

namespace BrightWire.Linear
{
    /// <summary>
    /// Makes predictions from a previously trained model
    /// </summary>
    class LogisticRegressionPredictor : ILogisticRegressionClassifier
    {
        readonly IVector _theta;
        readonly ILinearAlgebraProvider _lap;

        public LogisticRegressionPredictor(ILinearAlgebraProvider lap, IVector theta)
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
            return Predict(new[] { vals })[0];
        }

        public float Predict(IReadOnlyList<float> vals)
        {
            return Predict(new[] { vals })[0];
        }

        public float[] Predict(IReadOnlyList<IReadOnlyList<float>> input)
        {
            using (var feature = _lap.CreateMatrix((uint)input.Count, (uint)input[0].Count + 1, (i, j) => j == 0 ? 1 : input[(int)i][(int)j - 1]))
            using (var h0 = feature.Multiply(_theta))
            using (var h1 = h0.Column(0))
            using (var h = h1.Sigmoid())
            using(var h2 = h.AsIndexable()) {
                return h2.ToArray();
            }
        }
    }
}
