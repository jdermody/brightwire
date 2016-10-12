using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Linear.Training
{
    internal class LogisticRegressionTrainer : ILogisticRegressionTrainer
    {
        readonly ILinearAlgebraProvider _lap;
        readonly IMatrix _feature;
        readonly IVector _target;

        public LogisticRegressionTrainer(ILinearAlgebraProvider lap, IDataTable table)
        {
            _lap = lap;
            var numRows = table.RowCount;
            var numCols = table.ColumnCount;
            var classColumnIndex = table.TargetColumnIndex;

            var data = table.GetNumericRows(Enumerable.Range(0, numCols).Where(c => c != classColumnIndex));
            _feature = lap.Create(numRows, numCols, (i, j) => j == 0 ? 1 : data[j - 1][i]);
            _target = lap.Create(table.GetNumericColumn(classColumnIndex));
        }

        public LogisticRegressionModel GradientDescent(int iterations, float learningRate, float lambda = 0.1f, Func<float, bool> costCallback = null)
        {
            var theta = _lap.Create(_feature.ColumnCount, 0f);

            for (var i = 0; i < iterations; i++) {
                if (costCallback != null) {
                    var cost = ComputeCost(theta, lambda);
                    if (!costCallback(cost))
                        break;
                }
                using (var d = _Derivative(theta, lambda)) {
                    d.Multiply(learningRate);
                    var theta2 = theta.Subtract(d);
                    theta.Dispose();
                    theta = theta2;
                }
            }
            var ret = new LogisticRegressionModel {
                Theta = theta.Data
            };
            theta.Dispose();
            return ret;
        }

        public float ComputeCost(IVector th, float lambda)
        {
            using (var h0 = _feature.Multiply(th))
            using (var h1 = h0.Column(0))
            using (var h = h1.Sigmoid())
            using (var hLog = h.Log())
            using (var t = _target.Clone()) {
                var a = _target.DotProduct(hLog);
                t.Multiply(-1f);
                t.Add(1f);
                h.Multiply(-1f);
                h.Add(1f);
                var b = t.DotProduct(hLog);
                var ret = -(a + b) / _feature.RowCount;
                if (lambda != 0)
                    ret += th.AsIndexable().Values.Skip(1).Select(v => v * v).Sum() * lambda / (2 * _feature.RowCount);
                return ret;
            }
        }

        IVector _Derivative(IVector th, float lambda)
        {
            using (var p0 = _feature.Multiply(th))
            using (var p1 = p0.Column(0))
            using (var p = p1.Sigmoid())
            using (var e0 = p.Subtract(_target))
            using (var e = e0.ToRowMatrix())
            using (var e2 = e.Multiply(_feature)) {
                e2.Multiply(1f / _feature.RowCount);
                var ret = e2.Row(0);
                if (lambda != 0) {
                    using (var reg = _lap.CreateIndexable(th.Count))
                    using (var thi = th.AsIndexable()) {
                        var term = lambda / _feature.RowCount;
                        for (var i = 1; i < th.Count; i++) {
                            reg[i] = thi[i] * term;
                        }
                        using(var reg2 = _lap.Create(reg))
                            ret.Add(reg2);   
                    }
                }
                return ret;
            }
        }
    }
}
