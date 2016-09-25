using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Linear
{
    public class Regression
    {
        readonly IMatrix _feature;
        readonly IVector _target;
        readonly int _numInputs;
        readonly ILinearAlgebraProvider _lap;
        double _firstCost = 0, _lastCost = 0;

        public Regression(ILinearAlgebraProvider lap, IReadOnlyList<float[]> input, float[] output)
        {
            _lap = lap;
            var numRows = input.Count;
            if (numRows == 0)
                throw new ArgumentException("No input data");
            var numCols = input[0].Length;
            if (numCols == 0)
                throw new ArgumentException("No input data - no columns");
            if (input.Count != output.Length)
                throw new ArgumentException(String.Format("Number of rows between input and output do not match: {0} to {1}", numRows, output.Length));

            _feature = lap.Create(numRows, numCols + 1, (i, j) => j == 0 ? 1 : input[i][j - 1]);
            _target = lap.Create(output.Length, i => output[i]);
            _numInputs = numRows;
        }

        public double FirstCost { get { return _firstCost; } }
        public double LastCost { get { return _lastCost; } }

        public IVector Solve()
        {
            var lambdaMatrix = _lap.CreateIdentity(_feature.ColumnCount);

            // solve using normal method
            var featureTranspose = _feature.Transpose();
            var pinv = featureTranspose.Multiply(_feature).Add(lambdaMatrix).Inverse();
            var a2 = featureTranspose.Multiply(_target.ToColumnMatrix());
            var ret = pinv.Multiply(a2);
            return ret.Column(0);
        }

        public IVector GradientDescent(IVector theta, int iterations, float learningRate, float lambda = 0.1f)
        {
            var regularisation = 1f - (learningRate * lambda) / _feature.RowCount;
            var regularisationVector = _lap.Create(theta.Count, i => i == 0 ? 1f : regularisation);

            _firstCost = _ComputeCost(theta, lambda);
            for (var i = 0; i < iterations; i++) {
                var p = _feature.Multiply(theta).Column(0);
                var e = p.Subtract(_target).ToRowMatrix();
                var d = e.Multiply(_feature);
                var delta = d.Row(0);
                delta.Multiply(learningRate);//.Divide(_numInputs);
                theta = theta.PointwiseMultiply(regularisationVector).Subtract(delta);
                _lastCost = _ComputeCost(theta, lambda);
            }
            return theta;
        }

        float _ComputeCost(IVector theta, float lambda)
        {
            var regularisationCost = theta.AsIndexable().Values.Skip(1).Sum(v => v * v * lambda);

            var h = _feature.Multiply(theta);
            var diff = _target.Subtract(h.Column(0));
            diff.Add(regularisationCost);
            return diff.PointwiseMultiply(diff).AsIndexable().Values.Sum() / (2 * _numInputs);
        }

        public float GetCost(IVector theta, float lambda = 0.1f)
        {
            return _ComputeCost(theta, lambda);
        }

        public float Predict(IVector theta, params float[] vals)
        {
            var v = _lap.Create(vals.Length + 1, i => i == 0 ? 1 : vals[i - 1]);
            return v.DotProduct(theta);
        }

        public float Predict(IVector theta, IReadOnlyList<float> vals)
        {
            var v = _lap.Create(vals.Count + 1, i => i == 0 ? 1f : vals[i - 1]);
            return v.DotProduct(theta);
        }
    }
}
