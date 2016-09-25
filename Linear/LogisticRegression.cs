using BrightWire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Linear
{
    public class LogisticRegression
    {
        readonly IMatrix _feature;
        readonly IVector _target;
        readonly int _numInputs;
        readonly ILinearAlgebraProvider _lap;
        double _firstCost, _lastCost;

        public LogisticRegression(ILinearAlgebraProvider lap, IReadOnlyList<float[]> input, bool[] output)
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
            _target = lap.Create(output.Length, i => output[i] ? 1f : 0f);
            _numInputs = numRows;
        }

        public double FirstCost { get { return _firstCost; } }
        public double LastCost { get { return _lastCost; } }

        const double VERY_SMALL = -1.0E200;
        const double VERY_BIG = 1.0E200;
        internal double _Bound(double d)
        {
            if (d < VERY_SMALL)
                return VERY_SMALL;
            else if (d > VERY_BIG)
                return VERY_BIG;
            return d;
        }

        internal double _BoundExp(double d)
        {
            return _Bound(Math.Exp(d));
        }

        internal double _BoundLog(double d)
        {
            if (d == 0)
                d = VERY_SMALL;
            return _Bound(Math.Log(d));
        }

        public float CalculateCost(IVector theta, float? lambda = 0.1f)
        {
            return _ComputeCost(theta, lambda);
        }

        public float[] Predict(IVector theta, IReadOnlyList<float[]> input)
        {
            var feature = _lap.Create(input.Count, input[0].Length + 1, (i, j) => j == 0 ? 1 : input[i][j - 1]);
            var h = feature.Multiply(theta).Column(0).Sigmoid();
            return h.AsIndexable().ToArray();
        }

        public float[] Derivative(float[] theta, float? lambda = 0.1f)
        {
            return _Derivative(_lap.Create(theta), lambda).AsIndexable().ToArray();
        }

        IVector _Derivative(IVector th, float? lambda)
        {
            var p = _feature.Multiply(th).Column(0).Sigmoid();
            var e = p.Subtract(_target).ToRowMatrix();
            var e2 = e.Multiply(_feature);
            e2.Multiply(1f / _numInputs);
            var ret = e2.Row(0);

            if (lambda.HasValue) {
                var reg = _lap.CreateIndexable(th.Count);
                var term = lambda.Value / _numInputs;
                for (var i = 1; i < th.Count; i++) {
                    reg[i] = th.AsIndexable()[1] * term;
                }
                ret.Add(reg);
            }
            return ret;
        }

        float _ComputeCost(IVector th, float? lambda)
        {
            var h = _feature.Multiply(th).Column(0).Sigmoid();
            var a = _target.DotProduct(h.Log());
            var t = _target.Clone();
            t.Multiply(-1f);
            t.Add(1f);
            h.Multiply(-1f);
            h.Add(1f);

            var b = t.DotProduct(h.Log());
            var ret = -(a + b) / _numInputs;

            if (lambda.HasValue)
                ret += th.AsIndexable().Values.Skip(1).Select(v => v * v).Sum() * lambda.Value / (2 * _numInputs);

            return ret;
        }

        public IVector GradientDescent(IVector theta, int iterations, float learningRate, float? lambda = 0.1f)
        {
            _firstCost = _ComputeCost(theta, null);
            for (var i = 0; i < iterations; i++) {
                var d = _Derivative(theta, lambda);
                d.Multiply(learningRate);
                theta = theta.Subtract(d);
                _lastCost = _ComputeCost(theta, null);
            }
            return theta;
        }
    }
}
