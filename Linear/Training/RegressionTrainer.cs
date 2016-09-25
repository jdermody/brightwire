using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Linear.Training
{
    public class RegressionTrainer
    {
        readonly ILinearAlgebraProvider _lap;
        readonly IMatrix _feature;
        readonly IVector _target;

        public RegressionTrainer(ILinearAlgebraProvider lap, IIndexableDataTable table, int classColumnIndex)
        {
            _lap = lap;
            var numRows = table.RowCount;
            var numCols = table.ColumnCount;

            var data = table.GetNumericRows(Enumerable.Range(0, numCols).Where(c => c != classColumnIndex));
            _feature = lap.Create(numRows, numCols + 1, (i, j) => j == 0 ? 1 : data[i][j - 1]);
            _target = lap.Create(table.GetNumericColumn(classColumnIndex));
        }

        public IVector Solve()
        {
            // solve using normal method
            using (var lambdaMatrix = _lap.CreateIdentity(_feature.ColumnCount))
            using (var featureTranspose = _feature.Transpose())
            using (var pinv = featureTranspose.Multiply(_feature))
            using (var pinv2 = pinv.Add(lambdaMatrix))
            using (var pinv3 = pinv2.Inverse())
            using (var tc = _target.ToColumnMatrix())
            using (var a2 = featureTranspose.Multiply(tc))
            using (var ret = pinv3.Multiply(a2))
                return ret.Column(0);
        }

        public IVector GradientDescent(int iterations, float learningRate, float lambda = 0.1f)
        {
            var regularisation = 1f - (learningRate * lambda) / _feature.RowCount;
            var theta = _lap.Create(_feature.ColumnCount, 0f);

            using (var regularisationVector = _lap.Create(theta.Count, i => i == 0 ? 1f : regularisation)) {
                for (var i = 0; i < iterations; i++) {
                    var cost = _ComputeCost(theta, lambda);

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

            return theta;
        }

        float _ComputeCost(IVector theta, float lambda)
        {
            var regularisationCost = theta.AsIndexable().Values.Skip(1).Sum(v => v * v * lambda);

            var h = _feature.Multiply(theta);
            var diff = _target.Subtract(h.Column(0));
            diff.Add(regularisationCost);
            return diff.PointwiseMultiply(diff).AsIndexable().Values.Sum() / (2 * _feature.RowCount);
        }
    }
}
