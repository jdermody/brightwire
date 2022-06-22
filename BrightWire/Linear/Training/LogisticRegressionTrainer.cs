using System;
using System.Linq;
using BrightData;
using BrightData.Helper;
using BrightData.LinearAlegbra2;
using BrightWire.Models.Linear;

namespace BrightWire.Linear.Training
{
    /// <summary>
    /// Logistic regression
    /// https://en.wikipedia.org/wiki/Logistic_regression
    /// </summary>
    internal class LogisticRegressionTrainer : ILogisticRegressionTrainer
    {
        readonly LinearAlgebraProvider _lap;
        readonly IMatrix _feature;
        readonly IVector _target;

        public LogisticRegressionTrainer(IRowOrientedDataTable table)
        {
            _lap = table.Context.LinearAlgebraProvider2;

            // TODO: support target matrix instead of vector
            var (features, target) = table.AsMatrices();
            if(target.ColumnCount > 1)
                throw new ArgumentException("Multiple features are not currently supported");

            // ReSharper disable once AccessToDisposedClosure
            _feature = _lap.CreateMatrix(features.RowCount, features.ColumnCount + 1, (i, j) => j == 0 ? 1 : features[i, j - 1]);
            _target = _lap.CreateVector(target.Column(0));
            features.Dispose();
            target.Dispose();
        }

        public LogisticRegression GradientDescent(uint iterations, float learningRate, float lambda = 0.1f, Func<float, bool>? costCallback = null)
        {
            var theta = _lap.CreateVector(_feature.ColumnCount, 0f);

            for (uint i = 0; i < iterations; i++) {
                if (costCallback != null) {
                    var cost = ComputeCost(theta, lambda);
                    if (!costCallback(cost))
                        break;
                }

                using var d = Derivative(theta, lambda);
                d.Multiply(learningRate);
                var theta2 = theta.Subtract(d);
                theta.Dispose();
                theta = theta2;
            }
            var ret = new LogisticRegression {
                Theta = theta.Segment.ToNewArray()
            };
            theta.Dispose();
            return ret;
        }

        IVector Derivative(IVector th, float lambda)
        {
            using var p0 = _feature.Multiply(th);
            using var p1 = p0.Column(0);
            using var p = p1.Sigmoid();
            using var e = _lap.CreateMatrix(1, p.Size, p.Subtract(_target.Segment));
            using var e2 = e.Multiply(_feature);
            e2.Multiply(1f / _feature.RowCount);
            var ret = _lap.CreateVector(e2.Row(0));
            if (FloatMath.IsNotZero(lambda)) {
                var reg = new float[th.Size];
                var term = lambda / _feature.RowCount;
                for (uint i = 1; i < th.Size; i++) {
                    reg[i] = th[i] * term;
                }

                using var regVector = _lap.CreateVector(reg);
                ret.Add(regVector);
            }
            return ret;
        }

        public float ComputeCost(IVector th, float lambda)
        {
            using var h0 = _feature.Multiply(th);
            using var h1 = h0.Column(0);
            using var h = h1.Sigmoid();
            using var hLog = _lap.CreateVector(h.Log());
            using var t = _target.Clone();
            var a = _target.DotProduct(hLog);
            t.Multiply(-1f);
            t.Add(1f);
            h.Multiply(-1f);
            h.Add(1f);
            var b = t.DotProduct(hLog);
            var ret = -(a + b) / _feature.RowCount;
            if (FloatMath.IsNotZero(lambda))
                ret += th.Segment.Values.Skip(1).Select(v => v * v).Sum() * lambda / (2 * _feature.RowCount);
            return ret;
        }
    }
}
