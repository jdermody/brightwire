using BrightData;
using BrightData.LinearAlegbra2;
using BrightData.LinearAlgebra;

namespace BrightWire.Linear
{
    /// <summary>
    /// Makes predictions from a previously trained model
    /// </summary>
    internal class LogisticRegressionPredictor : ILogisticRegressionClassifier
    {
        readonly IVector _theta;
        readonly LinearAlgebraProvider _lap;

        public LogisticRegressionPredictor(LinearAlgebraProvider lap, IVector theta)
        {
            _lap = lap;
            _theta = theta;
        }

        public void Dispose()
        {
            _theta.Dispose();
        }

        public IVector Predict(IMatrix input)
        {
            using var feature = _lap.CreateMatrix(input.RowCount, input.ColumnCount+1, (i, j) => j == 0 ? 1 : input[i, j - 1]);
            using var h0 = feature.Multiply(_theta);
            using var h1 = h0.Column(0);
            using var h = h1.Sigmoid();
            return _lap.CreateVector(h);
        }
    }
}
