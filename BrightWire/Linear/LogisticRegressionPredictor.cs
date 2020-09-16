using System.Collections.Generic;
using BrightData;

namespace BrightWire.Linear
{
    /// <summary>
    /// Makes predictions from a previously trained model
    /// </summary>
    class LogisticRegressionPredictor : ILogisticRegressionClassifier
    {
        readonly IFloatVector _theta;
        readonly ILinearAlgebraProvider _lap;

        public LogisticRegressionPredictor(ILinearAlgebraProvider lap, IFloatVector theta)
        {
            _lap = lap;
            _theta = theta;
        }

        public void Dispose()
        {
            _theta.Dispose();
        }

        public Vector<float> Predict(Matrix<float> input)
        {
            using var feature = _lap.CreateMatrix(input);
            using var h0 = feature.Multiply(_theta);
            using var h1 = h0.Column(0);
            using var h = h1.Sigmoid();
            return h.Data;
        }
    }
}
