using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Linear.Training
{
    internal class RegressionTrainer : ILinearRegressionTrainer
    {
        readonly ILinearAlgebraProvider _lap;
        readonly IMatrix _feature;
        readonly IVector _target;

        public RegressionTrainer(ILinearAlgebraProvider lap, IDataTable table)
        {
            _lap = lap;
            var numRows = table.RowCount;
            var numCols = table.ColumnCount;
            int classColumnIndex = table.TargetColumnIndex;

            var data = table.GetNumericColumns(Enumerable.Range(0, numCols).Where(c => c != classColumnIndex));
            _feature = lap.Create(numRows, numCols, (i, j) => j == 0 ? 1 : data[j - 1][i]);
            _target = lap.Create(table.GetColumn<float>(classColumnIndex));
        }

        public LinearRegression Solve()
        {
            // solve using normal method
            using (var lambdaMatrix = _lap.CreateIdentity(_feature.ColumnCount))
            using (var zero = _lap.Create(1, 0f)) {
                lambdaMatrix.UpdateColumn(0, zero.AsIndexable(), 0);

                using (var featureTranspose = _feature.Transpose())
                using (var pinv = featureTranspose.Multiply(_feature))
                using (var pinv2 = pinv.Add(lambdaMatrix))
                using (var pinv3 = pinv2.Inverse())
                using (var tc = _target.ToColumnMatrix())
                using (var a2 = featureTranspose.Multiply(tc))
                using (var ret = pinv3.Multiply(a2))
                using (var theta = ret.Column(0)) {
                    return new LinearRegression {
                        Theta = theta.Data
                    };
                }
            }
        }

        public LinearRegression GradientDescent(int iterations, float learningRate, float lambda = 0.1f, Func<float, bool> costCallback = null)
        {
            var regularisation = 1f - (learningRate * lambda) / _feature.RowCount;
            var theta = _lap.Create(_feature.ColumnCount, 0f);

            using (var regularisationVector = _lap.Create(theta.Count, i => i == 0 ? 1f : regularisation)) {
                for (var i = 0; i < iterations; i++) {
                    if (costCallback != null) {
                        var cost = ComputeCost(theta, lambda);
                        if (!costCallback(cost))
                            break;
                    }

                    using (var p = _feature.Multiply(theta))
                    using (var pc = p.Column(0))
                    using (var e = pc.Subtract(_target))
                    using (var e2 = e.ToRowMatrix())
                    using (var d = e2.Multiply(_feature))
                    using(var delta = d.Row(0)) {
                        delta.Multiply(learningRate);
                        using (var temp = theta.PointwiseMultiply(regularisationVector)) {
                            var theta2 = temp.Subtract(delta);
                            theta.Dispose();
                            theta = theta2;
                        }
                    }
                }
            }

            var ret = new LinearRegression {
                Theta = theta.Data
            };
            theta.Dispose();
            return ret;
        }

        public float ComputeCost(IVector theta, float lambda)
        {
            var regularisationCost = theta.AsIndexable().Values.Skip(1).Sum(v => v * v * lambda);

            var h = _feature.Multiply(theta);
            var diff = _target.Subtract(h.Column(0));
            diff.Add(regularisationCost);
            return diff.PointwiseMultiply(diff).AsIndexable().Values.Sum() / (2 * _feature.RowCount);
        }
    }
}
