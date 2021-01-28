using System;
using System.Linq;
using BrightData;
using BrightData.Helper;
using BrightTable;
using BrightWire.Models.Linear;

namespace BrightWire.Linear.Training
{
    internal class LegacyLogisticRegressionTrainer : ILogisticRegressionTrainer
    {
        readonly ILinearAlgebraProvider _lap;
        readonly IFloatMatrix _feature;
        readonly IFloatVector _target;

        public LegacyLogisticRegressionTrainer(ILinearAlgebraProvider lap, IRowOrientedDataTable table)
        {
            _lap = lap;
            var numRows = table.RowCount;
            var classColumnIndex = table.GetTargetColumnOrThrow();

            var numCols = table.ColumnCount;
            var featureColumns = numCols.AsRange().Where(c => c != classColumnIndex).ToArray();

            var data = table.GetColumnsAsVectors(featureColumns).ToArray();
            _feature = lap.CreateMatrix(numRows, numCols, (i, j) => j == 0 ? 1 : data[j - 1][i]);
            _target = lap.CreateVector(table.GetColumnsAsVectors(classColumnIndex).Single());
        }

        public LogisticRegression GradientDescent(uint iterations, float learningRate, float lambda = 0.1f, Func<float, bool>? costCallback = null)
        {
            var theta = _lap.CreateVector(_feature.ColumnCount, 0f);

            for (var i = 0; i < iterations; i++) {
                if (costCallback != null) {
                    var cost = ComputeCost(theta, lambda);
                    if (!costCallback(cost))
                        break;
                }

                using var d = _Derivative(theta, lambda);
                d.Multiply(learningRate);
                var theta2 = theta.Subtract(d);
                theta.Dispose();
                theta = theta2;
            }

            var ret = new LogisticRegression {
                Theta = theta.Data
            };
            theta.Dispose();
            return ret;
        }

        public float ComputeCost(IFloatVector th, float lambda)
        {
            using var h0 = _feature.Multiply(th);
            using var h1 = h0.Column(0);
            using var h = h1.Sigmoid();
            using var hLog = h.Log();
            using var t = _target.Clone();
            var a = _target.DotProduct(hLog);
            t.Multiply(-1f);
            t.Add(1f);
            h.Multiply(-1f);
            h.Add(1f);
            var b = t.DotProduct(hLog);
            var ret = -(a + b) / _feature.RowCount;
            if (FloatMath.IsNotZero(lambda))
                ret += th.AsIndexable().Values.Skip(1).Select(v => v * v).Sum() * lambda / (2 * _feature.RowCount);
            return ret;
        }

        IFloatVector _Derivative(IFloatVector th, float lambda)
        {
            using var p0 = _feature.Multiply(th);
            using var p1 = p0.Column(0);
            using var p = p1.Sigmoid();
            using var e0 = p.Subtract(_target);
            using var e = e0.ReshapeAsRowMatrix();
            using var e2 = e.Multiply(_feature);
            e2.Multiply(1f / _feature.RowCount);
            var ret = e2.Row(0);
            if (FloatMath.IsNotZero(lambda)) {
                var reg = new float[th.Count];
                using var thi = th.AsIndexable();
                var term = lambda / _feature.RowCount;
                for (uint i = 1; i < th.Count; i++) {
                    reg[i] = thi[i] * term;
                }

                using var regVector = _lap.CreateVector(reg);
                ret.Add(regVector);
            }

            return ret;
        }
    }
}