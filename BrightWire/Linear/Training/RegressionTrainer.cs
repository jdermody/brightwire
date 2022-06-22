using System;
using System.Linq;
using BrightData;
using BrightData.LinearAlegbra2;
using BrightWire.Models.Linear;

namespace BrightWire.Linear.Training
{
    /// <summary>
    /// Linear regression
    /// https://en.wikipedia.org/wiki/Linear_regression
    /// </summary>
    internal class RegressionTrainer : ILinearRegressionTrainer
    {
        readonly LinearAlgebraProvider _lap;
        readonly IMatrix _feature;
        readonly IVector _target;

        public RegressionTrainer(LinearAlgebraProvider lap, IDataTable table)
        {
            _lap = lap;
            var numRows = table.RowCount;
            var numCols = table.ColumnCount;
            var classColumnIndex = table.GetTargetColumnOrThrow();

            // TODO: support target matrix instead of vector
            var data = table.GetColumnsAsVectors(numCols.AsRange().Where(c => c != classColumnIndex).ToArray()).ToArray();
            _feature = lap.CreateMatrix(numRows, numCols, (i, j) => j == 0 ? 1 : data[j - 1][i]);
            _target = table.GetColumnsAsVectors(classColumnIndex).Single();
        }

		// normal method removed until GPU provider can properly calculate matrix inverses!
        //public LinearRegression Solve()
		//{
		//	// solve using normal method
		//	using (var lambdaMatrix = _lap.CreateIdentityMatrix(_feature.ColumnCount))
		//	using (var zero = _lap.CreateVector(1, 0f)) {
		//		lambdaMatrix.UpdateColumn(0, zero.AsIndexable(), 0);

		//		using (var featureTranspose = _feature.Transpose())
		//		using (var pinv = featureTranspose.Multiply(_feature))
		//		using (var pinv2 = pinv.Add(lambdaMatrix))
		//		using (var pinv3 = pinv2.Inverse())
		//		using (var tc = _target.ReshapeAsColumnMatrix())
		//		using (var a2 = featureTranspose.Multiply(tc))
		//		using (var ret = pinv3.Multiply(a2))
		//		using (var theta = ret.Column(0)) {
		//			return new LinearRegression {
		//				Theta = theta.Segment
		//			};
		//		}
		//	}
		//}

		public LinearRegression GradientDescent(int iterations, float learningRate, float lambda = 0.1f, Func<float, bool>? costCallback = null)
        {
            var regularisation = 1f - (learningRate * lambda) / _feature.RowCount;
            var theta = _lap.CreateVector(_feature.ColumnCount, 0f);

            using var regularisationVector = _lap.CreateVector(theta.Size, i => i == 0 ? 1f : regularisation);
            for (var i = 0; i < iterations; i++) {
                if (costCallback != null) {
                    var cost = ComputeCost(theta, lambda);
                    if (!costCallback(cost))
                        break;
                }

                using var p = _feature.Multiply(theta);
                using var pc = p.Column(0);
                using var e2 = _lap.CreateMatrix(1, pc.Size, pc.Subtract(_target.Segment));
                using var d = e2.Multiply(_feature);
                using var delta = d.Row(0);
                using var delta2 = _lap.CreateVector(delta.Multiply(learningRate));
                using var temp = theta.PointwiseMultiply(regularisationVector);
                var theta2 = temp.Subtract(delta2);
                theta.Dispose();
                theta = theta2;
            }

            var ret = new LinearRegression {
                Theta = theta.Segment.ToNewArray()
            };
            theta.Dispose();
            return ret;
        }

        public float ComputeCost(IVector theta, float lambda)
        {
            var regularisationCost = theta.Segment.Values.Skip(1).Sum(v => v * v * lambda);

            using var h = _feature.Multiply(theta);
            using var col = _lap.CreateVector(h.Column(0));
            using var diff = _target.Subtract(col);
            diff.Add(regularisationCost);
            using var squared = diff.PointwiseMultiply(diff);
            return squared.Segment.Values.Sum() / (2 * _feature.RowCount);
        }
    }
}
